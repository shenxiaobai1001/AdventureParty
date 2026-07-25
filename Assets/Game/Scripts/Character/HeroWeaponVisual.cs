using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single back mount for equipped weapons; combat loadout drives main/off-hand attachment.
/// </summary>
public class HeroWeaponVisual : MonoBehaviour
{
    public enum AttachTarget
    {
        Hand,
        OffHand,
        BackMount,
    }

    [Serializable]
    public class LegacyWeaponSlot
    {
        public string displayName = "Weapon";
        public Transform backMountSocket;
        public Transform handSocket;
        public GameObject weaponPrefab;
        public Transform weaponInScene;
    }

    [Header("Mount Sockets")]
    [Tooltip("Single back mount for stowed weapons (max 3 visible).")]
    public Transform backMountSocket;

    [Tooltip("Main-hand grip socket.")]
    public Transform mainHandSocket;

    [Tooltip("Off-hand grip socket (dual-wield left weapon).")]
    public Transform offHandSocket;

    [Tooltip("Dedicated left-hand shield mount under Hand_L (Socket_WeaponHand_Shield).")]
    public Transform shieldHandSocket;

    [Tooltip("Dedicated left-hand bow mount under Hand_L (Socket_WeaponHand_Bow).")]
    public Transform bowHandSocket;

    [Header("Legacy (migrated on Awake)")]
    public LegacyWeaponSlot mainHand = new LegacyWeaponSlot { displayName = "Main Hand" };
    public LegacyWeaponSlot offHand = new LegacyWeaponSlot { displayName = "Off Hand" };

    [Header("Auto-create Back Mount")]
    public Vector3 backMountLocalPosition = new Vector3(0.06f, 0.08f, -0.12f);
    public Vector3 backMountLocalEuler = new Vector3(-90f, 0f, 90f);

    [Header("Auto-create Hand Mounts")]
    public Vector3 mainHandLocalPosition = Vector3.zero;
    public Vector3 mainHandLocalEuler = Vector3.zero;
    public Vector3 offHandLocalPosition = Vector3.zero;
    public Vector3 offHandLocalEuler = Vector3.zero;

    [Tooltip("Initial local pose for Socket_WeaponHand_Shield under Hand_L.")]
    public Vector3 shieldHandLocalPosition = new Vector3(0.05f, 0.01f, -0.07f);
    public Vector3 shieldHandLocalEuler = new Vector3(-70.5f, -168f, -5f);

    [Tooltip("Initial local pose for Socket_WeaponHand_Bow under Hand_L.")]
    public Vector3 bowHandLocalPosition = new Vector3(-0.1f, -0.03f, 0.03f);
    public Vector3 bowHandLocalEuler = new Vector3(-0.07f, -88f, 85f);

    static readonly Vector3 DefaultShieldHandLocalPosition = new Vector3(0.05f, 0.01f, -0.07f);
    static readonly Vector3 DefaultShieldHandLocalEuler = new Vector3(-70.5f, -168f, -5f);
    static readonly Vector3 DefaultBowHandLocalPosition = new Vector3(-0.1f, -0.03f, 0.03f);
    static readonly Vector3 DefaultBowHandLocalEuler = new Vector3(-0.07f, -88f, 85f);

    readonly List<MountedWeapon> mountedWeapons = new List<MountedWeapon>();
    ResolvedCombatLoadout _loadout = ResolvedCombatLoadout.Empty;
    MountedWeapon drawnPrimary;
    MountedWeapon drawnOffHand;
    MountedWeapon pendingWeapon;
    AttachTarget? pendingAttachTarget;

    /// <summary>Explicit right-hand equip (no preference system).</summary>
    public SyntyWeaponItemData equippedRight;

    /// <summary>Explicit left-hand equip (shield always here).</summary>
    public SyntyWeaponItemData equippedLeft;

    public ResolvedCombatLoadout CurrentLoadout => _loadout;

    public bool HasDrawnMeleeWeapon =>
        (drawnPrimary?.instance && drawnPrimary.weaponData
         && !MeleeSweepDefaults.IsRangedOrNonMelee(drawnPrimary.weaponData.category))
        || (drawnOffHand?.instance && drawnOffHand.weaponData
            && !MeleeSweepDefaults.IsRangedOrNonMelee(drawnOffHand.weaponData.category)
            && drawnOffHand.weaponData.category != WeaponCategory.Shield);

    /// <summary>Adds melee sweep sources for currently drawn hand weapons.</summary>
    public void CollectDrawnMeleeSweepSources(List<MeleeSweepSource> into)
    {
        if (into == null)
            return;

        TryAddDrawnSource(drawnPrimary, into);
        TryAddDrawnSource(drawnOffHand, into);
    }

    static void TryAddDrawnSource(MountedWeapon mount, List<MeleeSweepSource> into)
    {
        if (mount?.instance == null || !mount.weaponData)
            return;
        if (MeleeSweepDefaults.IsRangedOrNonMelee(mount.weaponData.category))
            return;

        var source = mount.instance.GetComponent<MeleeSweepSource>();
        if (!source)
            source = MeleeSweepSource.EnsureOnWeapon(mount.instance, mount.weaponData);
        if (source && source.contributesToMelee)
            into.Add(source);
    }
    public bool HasDrawableWeapon => _loadout.HasDrawableWeapon;
    public bool HasMainWeapon => HasDrawableWeapon;

    void Awake()
    {
        MigrateLegacySockets();
        EnsureSockets();
    }

    void Start()
    {
        PlaceAllOnBack();
    }

    /// <summary>
    /// Equip to a hand. Fails if the target hand (or locked other hand) is occupied — caller must unequip first.
    /// Prefer <see cref="WeaponEquipService.TrySmartEquip"/> for UI equip.
    /// </summary>
    public bool TryEquip(SyntyWeaponItemData weapon, WeaponHand hand, out string reason)
    {
        if (!WeaponProficiencyMapper.CanEquip(weapon, hand, equippedRight, equippedLeft, out reason))
            return false;

        if (hand == WeaponHand.Right)
        {
            equippedRight = weapon;
            var rule = WeaponProficiencyMapper.GetHandRule(weapon.category);
            if (rule == WeaponHandRule.TwoHand || rule == WeaponHandRule.RightLocksLeft)
                equippedLeft = null;
        }
        else
        {
            equippedLeft = weapon;
            if (WeaponProficiencyMapper.GetHandRule(weapon.category) == WeaponHandRule.LeftTwoHand)
                equippedRight = null;
        }

        reason = null;
        RebuildLoadoutFromEquip(null);
        NotifyEquipChanged();
        return true;
    }

    public void Unequip(WeaponHand hand)
    {
        if (hand == WeaponHand.Right)
            equippedRight = null;
        else
            equippedLeft = null;

        RebuildLoadoutFromEquip(null);
        NotifyEquipChanged();
    }

    /// <summary>Set both hands directly (smart equip / replace). Clears illegal shield-only.</summary>
    public void ForceSetHands(SyntyWeaponItemData right, SyntyWeaponItemData left)
    {
        // Bow always lives on the left (archer draw hand is right).
        if (right && right.category == WeaponCategory.Bow)
        {
            left = right;
            right = null;
        }

        if (left && left.category == WeaponCategory.Bow)
            right = null;

        if (left && left.category == WeaponCategory.Shield
            && (right == null || !WeaponEquipService.IsShieldPairablePrimary(right.category)))
            left = null;

        if (right && WeaponProficiencyMapper.OccupiesBothHands(right.category)
            && !WeaponProficiencyMapper.IsLeftHandPrimary(right.category))
            left = null;

        equippedRight = right;
        equippedLeft = left;
        RebuildLoadoutFromEquip(null);
        NotifyEquipChanged();
    }

    public static event System.Action EquipChanged;

    void NotifyEquipChanged()
    {
        EquipChanged?.Invoke();
    }

    public void SyncFromWeaponGrid(IReadOnlyList<WeaponGridEntry> entries)
    {
        EnsureSockets();
        // Only keep hand attach if weapons were already drawn (E). Do NOT fall back to
        // primaryHand — that would snap newly equipped weapons into hands immediately.
        var preservePrimary = drawnPrimary?.weaponData;
        var preserveOff = drawnOffHand?.weaponData;
        ClearSpawnedWeapons();
        ClearBackMountChildren();

        PruneEquipNotInGrid(entries);
        // Do NOT auto-equip from grid — weapons in bar are holstered until UI Equip.

        RebuildLoadoutFromEquip(entries);
        NotifyEquipChanged();
        if (!backMountSocket || entries == null || entries.Count == 0)
            return;

        var spawnList = BuildSpawnList(entries, _loadout);
        var crossedIndex = 0;
        for (var i = 0; i < spawnList.Count; i++)
        {
            var weaponData = spawnList[i];
            if (!weaponData || !weaponData.syntySourcePrefab)
                continue;

            var instance = Instantiate(weaponData.syntySourcePrefab, backMountSocket);
            instance.name = weaponData.name;

            var mount = new MountedWeapon
            {
                weaponData = weaponData,
                instance = instance,
                crossedWeaponIndex = weaponData.category == WeaponCategory.Shield ? -1 : crossedIndex,
            };

            if (mount.crossedWeaponIndex >= 0)
                crossedIndex++;

            mountedWeapons.Add(mount);
        }

        drawnPrimary = FindMount(preservePrimary);
        drawnOffHand = FindMount(preserveOff);

        if (drawnPrimary != null || drawnOffHand != null)
        {
            // Re-resolve against current loadout (bow must sit on left / off-hand).
            AssignDrawnFromLoadout();
            ApplyCombatHandLayout();
        }
        else
            PlaceAllOnBack();
    }

    public void RefreshWeaponDetection()
    {
        EnsureSockets();
    }

    void PruneEquipNotInGrid(IReadOnlyList<WeaponGridEntry> entries)
    {
        if (entries == null)
            return;

        if (equippedRight && !IsInGrid(entries, equippedRight))
            equippedRight = null;
        if (equippedLeft && !IsInGrid(entries, equippedLeft))
            equippedLeft = null;
    }

    static bool IsInGrid(IReadOnlyList<WeaponGridEntry> entries, SyntyWeaponItemData weapon)
    {
        if (entries == null || !weapon)
            return false;

        for (var i = 0; i < entries.Count; i++)
        {
            if (entries[i].WeaponData == weapon)
                return true;
        }

        return false;
    }

    void RebuildLoadoutFromEquip(IReadOnlyList<WeaponGridEntry> entries)
    {
        _loadout = CombatLoadoutResolver.Resolve(equippedRight, equippedLeft);
        if (entries == null)
            return;

        foreach (var entry in entries)
        {
            var w = entry.WeaponData;
            if (!w || w == equippedRight || w == equippedLeft)
                continue;
            _loadout.backWeapons.Add(w);
        }
    }

    public void PlaceWeaponsForCasualStance()
    {
        drawnPrimary = null;
        drawnOffHand = null;
        PlaceAllOnBack();
    }

    public void PlaceWeaponsForCombatStance()
    {
        if (!_loadout.HasDrawableWeapon)
            return;

        AssignDrawnFromLoadout();
        ApplyCombatHandLayout();
    }

    public void PlaceAllOnBack()
    {
        drawnPrimary = null;
        drawnOffHand = null;
        pendingWeapon = null;
        pendingAttachTarget = null;

        foreach (var mount in mountedWeapons)
            AttachToBack(mount);
    }

    public void ClearAllWeapons()
    {
        ClearSpawnedWeapons();
    }

    public void RequestAttachOnSwitch(AttachTarget target)
    {
        if (target == AttachTarget.Hand)
            RequestDrawCombatLoadout();
        else if (target == AttachTarget.OffHand)
            RequestDrawCombatLoadout();
        else
            RequestSheathDrawnWeaponsToBack();
    }

    public void RequestDrawCombatLoadout()
    {
        pendingWeapon = FindMount(_loadout.primaryHand);
        // Bow is held in the left hand; other primaries use the main (right) hand.
        pendingAttachTarget = IsLeftHandPrimaryLoadout()
            ? AttachTarget.OffHand
            : AttachTarget.Hand;
    }

    public void RequestSheathDrawnWeaponsToBack()
    {
        pendingWeapon = drawnPrimary ?? drawnOffHand ?? FindMount(_loadout.primaryHand);
        pendingAttachTarget = AttachTarget.BackMount;
    }

    public void ApplyPendingAttach()
    {
        if (pendingAttachTarget == null)
            return;

        if (pendingAttachTarget == AttachTarget.Hand || pendingAttachTarget == AttachTarget.OffHand)
        {
            AssignDrawnFromLoadout();
            ApplyCombatHandLayout();
        }
        else
        {
            drawnPrimary = null;
            drawnOffHand = null;
            PlaceAllOnBack();
        }

        pendingWeapon = null;
        pendingAttachTarget = null;
    }

    void AssignDrawnFromLoadout()
    {
        if (IsLeftHandPrimaryLoadout())
        {
            // Bow: mesh on left (off) hand; right stays free for string draw.
            drawnPrimary = null;
            drawnOffHand = FindMount(_loadout.primaryHand);
            return;
        }

        drawnPrimary = FindMount(_loadout.primaryHand);
        drawnOffHand = FindMount(_loadout.offHand);
    }

    bool IsLeftHandPrimaryLoadout()
        => _loadout.primaryHand && WeaponProficiencyMapper.IsLeftHandPrimary(_loadout.primaryHand.category);

    void ApplyCombatHandLayout()
    {
        foreach (var mount in mountedWeapons)
        {
            if (mount == drawnPrimary)
                AttachToMainHand(mount);
            else if (mount == drawnOffHand)
                AttachToOffHand(mount);
            else
                AttachToBack(mount);
        }
    }

    void AttachToBack(MountedWeapon mount)
    {
        if (mount?.instance == null || !backMountSocket)
            return;

        var transform = mount.instance.transform;
        transform.SetParent(backMountSocket, false);

        if (mount.crossedWeaponIndex < 0)
            WeaponBackMountLayout.Apply(transform, mount.weaponData.category, 0);
        else
            WeaponBackMountLayout.Apply(transform, mount.weaponData.category, mount.crossedWeaponIndex);
    }

    void AttachToMainHand(MountedWeapon mount)
    {
        if (mount?.instance == null || !mainHandSocket)
            return;

        var transform = mount.instance.transform;
        transform.SetParent(mainHandSocket, false);
        WeaponHandLayout.Apply(transform, mount.weaponData.category, isOffHand: false);
        MeleeSweepSource.EnsureOnWeapon(mount.instance, mount.weaponData);
    }

    void AttachToOffHand(MountedWeapon mount)
    {
        if (mount?.instance == null)
            return;

        var category = mount.weaponData ? mount.weaponData.category : WeaponCategory.Sword;
        Transform socket;
        if (category == WeaponCategory.Shield)
            socket = shieldHandSocket ? shieldHandSocket : offHandSocket;
        else if (category == WeaponCategory.Bow)
            socket = bowHandSocket ? bowHandSocket : offHandSocket;
        else
            socket = offHandSocket;

        if (!socket)
            return;

        var transform = mount.instance.transform;
        transform.SetParent(socket, false);
        WeaponHandLayout.Apply(transform, category, isOffHand: true);
        MeleeSweepSource.EnsureOnWeapon(mount.instance, mount.weaponData);
    }

    MountedWeapon FindMount(SyntyWeaponItemData weaponData)
    {
        if (!weaponData)
            return null;

        foreach (var mount in mountedWeapons)
        {
            if (mount.weaponData == weaponData)
                return mount;
        }

        return null;
    }

    static List<SyntyWeaponItemData> BuildSpawnList(IReadOnlyList<WeaponGridEntry> entries, ResolvedCombatLoadout loadout)
    {
        var result = new List<SyntyWeaponItemData>();
        var seen = new HashSet<SyntyWeaponItemData>();

        void TryAdd(SyntyWeaponItemData weapon)
        {
            if (!weapon || seen.Contains(weapon) || result.Count >= WeaponInventoryBridge.MaxEquippedWeapons)
                return;

            seen.Add(weapon);
            result.Add(weapon);
        }

        TryAdd(loadout.primaryHand);
        TryAdd(loadout.offHand);

        var sorted = new List<WeaponGridEntry>(entries);
        sorted.Sort((a, b) => a.GridOrder.CompareTo(b.GridOrder));
        foreach (var entry in sorted)
            TryAdd(entry.WeaponData);

        return result;
    }

    void ClearBackMountChildren()
    {
        if (!backMountSocket)
            return;

        for (var i = backMountSocket.childCount - 1; i >= 0; i--)
        {
            var child = backMountSocket.GetChild(i);
            if (!child)
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }

    void ClearSpawnedWeapons()
    {
        foreach (var mount in mountedWeapons)
        {
            if (!mount.instance)
                continue;

            if (Application.isPlaying)
                Destroy(mount.instance);
            else
                DestroyImmediate(mount.instance);
        }

        mountedWeapons.Clear();
        drawnPrimary = null;
        drawnOffHand = null;
        pendingWeapon = null;
        pendingAttachTarget = null;
        _loadout = ResolvedCombatLoadout.Empty;
    }

    void MigrateLegacySockets()
    {
        if (!backMountSocket)
        {
            backMountSocket = FindBone("Socket_WeaponMount_Back")
                ?? FindBone("Socket_WeaponMount_Main")
                ?? mainHand.backMountSocket;
        }

        if (!mainHandSocket)
            mainHandSocket = FindBone("Socket_WeaponHand_Main") ?? mainHand.handSocket;

        if (!offHandSocket)
            offHandSocket = FindBone("Socket_WeaponHand_Off") ?? offHand.handSocket;

        if (!shieldHandSocket)
            shieldHandSocket = FindBone("Socket_WeaponHand_Shield");

        if (!bowHandSocket)
            bowHandSocket = FindBone("Socket_WeaponHand_Bow");
    }

    void EnsureSockets()
    {
        MigrateLegacySockets();

        var handRight = FindBone("Hand_R");
        var handLeft = FindBone("Hand_L");

        mainHandSocket = EnsureHandMountSocket(
            mainHandSocket, handRight, "Socket_WeaponHand_Main",
            mainHandLocalPosition, mainHandLocalEuler);

        offHandSocket = EnsureHandMountSocket(
            offHandSocket, handLeft, "Socket_WeaponHand_Off",
            offHandLocalPosition, offHandLocalEuler);

        shieldHandSocket = EnsureHandMountSocket(
            shieldHandSocket, handLeft, "Socket_WeaponHand_Shield",
            shieldHandLocalPosition, shieldHandLocalEuler);

        bowHandSocket = EnsureHandMountSocket(
            bowHandSocket, handLeft, "Socket_WeaponHand_Bow",
            bowHandLocalPosition, bowHandLocalEuler);

        // Old scene instances may still serialize (0,0,0); migrate to playtested defaults.
        if (shieldHandLocalPosition == Vector3.zero && shieldHandLocalEuler == Vector3.zero)
        {
            shieldHandLocalPosition = DefaultShieldHandLocalPosition;
            shieldHandLocalEuler = DefaultShieldHandLocalEuler;
        }

        if (bowHandLocalPosition == Vector3.zero && bowHandLocalEuler == Vector3.zero)
        {
            bowHandLocalPosition = DefaultBowHandLocalPosition;
            bowHandLocalEuler = DefaultBowHandLocalEuler;
        }

        if (shieldHandSocket)
        {
            shieldHandSocket.localPosition = shieldHandLocalPosition;
            shieldHandSocket.localEulerAngles = shieldHandLocalEuler;
            shieldHandSocket.localScale = Vector3.one;
        }

        if (bowHandSocket)
        {
            bowHandSocket.localPosition = bowHandLocalPosition;
            bowHandSocket.localEulerAngles = bowHandLocalEuler;
            bowHandSocket.localScale = Vector3.one;
        }

        if (!backMountSocket)
        {
            backMountSocket = FindOrCreateBackSocket(
                "Socket_WeaponMount_Back",
                backMountLocalPosition,
                backMountLocalEuler);
        }
    }

    Transform EnsureHandMountSocket(
        Transform currentSocket,
        Transform handBone,
        string socketName,
        Vector3 localPosition,
        Vector3 localEuler)
    {
        var existing = FindBone(socketName);
        if (existing)
            return existing;

        if (currentSocket && currentSocket.name == socketName)
            return currentSocket;

        if (!handBone)
            return currentSocket;

        if (!currentSocket || currentSocket.name == "Hand_R" || currentSocket.name == "Hand_L")
            return FindOrCreateHandSocketUnderBone(handBone, socketName, localPosition, localEuler);

        return currentSocket;
    }

    Transform FindOrCreateHandSocketUnderBone(Transform handBone, string socketName, Vector3 localPosition, Vector3 localEuler)
    {
        for (var i = 0; i < handBone.childCount; i++)
        {
            var child = handBone.GetChild(i);
            if (child.name == socketName)
                return child;
        }

        var socketGo = new GameObject(socketName);
        var socket = socketGo.transform;
        socket.SetParent(handBone, false);
        socket.localPosition = localPosition;
        socket.localEulerAngles = localEuler;
        return socket;
    }

    Transform FindOrCreateBackSocket(string socketName, Vector3 localPosition, Vector3 localEuler)
    {
        var existing = FindBone(socketName);
        if (existing)
            return existing;

        var backBone = FindBone("Back_Attachment") ?? FindBone("Spine_02") ?? FindBone("Spine_03");
        if (!backBone)
            return null;

        var socketGo = new GameObject(socketName);
        var socket = socketGo.transform;
        socket.SetParent(backBone, false);
        socket.localPosition = localPosition;
        socket.localEulerAngles = localEuler;
        return socket;
    }

    Transform FindBone(string boneName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == boneName)
                return t;
        }

        return null;
    }

    class MountedWeapon
    {
        public SyntyWeaponItemData weaponData;
        public GameObject instance;
        public int crossedWeaponIndex = -1;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        MigrateLegacySockets();
    }
#endif
}
