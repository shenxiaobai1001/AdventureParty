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

    [Tooltip("Off-hand grip socket.")]
    public Transform offHandSocket;

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

    readonly List<MountedWeapon> mountedWeapons = new List<MountedWeapon>();
    ResolvedCombatLoadout _loadout = ResolvedCombatLoadout.Empty;
    MountedWeapon drawnPrimary;
    MountedWeapon drawnOffHand;
    MountedWeapon pendingWeapon;
    AttachTarget? pendingAttachTarget;

    public ResolvedCombatLoadout CurrentLoadout => _loadout;
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

    public void SyncFromWeaponGrid(IReadOnlyList<WeaponGridEntry> entries)
    {
        EnsureSockets();
        var preservePrimary = drawnPrimary?.weaponData;
        var preserveOff = drawnOffHand?.weaponData;
        ClearSpawnedWeapons();
        ClearBackMountChildren();

        _loadout = CombatLoadoutResolver.Resolve(entries);
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

        drawnPrimary = FindMount(preservePrimary) ?? FindMount(_loadout.primaryHand);
        drawnOffHand = FindMount(preserveOff) ?? FindMount(_loadout.offHand);

        if (drawnPrimary != null || drawnOffHand != null)
            ApplyCombatHandLayout();
        else
            PlaceAllOnBack();
    }

    public void RefreshWeaponDetection()
    {
        EnsureSockets();
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

        drawnPrimary = FindMount(_loadout.primaryHand);
        drawnOffHand = FindMount(_loadout.offHand);
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
        pendingAttachTarget = AttachTarget.Hand;
    }

    public void RequestSheathDrawnWeaponsToBack()
    {
        pendingWeapon = drawnPrimary ?? FindMount(_loadout.primaryHand);
        pendingAttachTarget = AttachTarget.BackMount;
    }

    public void ApplyPendingAttach()
    {
        if (pendingAttachTarget == null)
            return;

        if (pendingAttachTarget == AttachTarget.Hand)
        {
            drawnPrimary = FindMount(_loadout.primaryHand);
            drawnOffHand = FindMount(_loadout.offHand);
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
    }

    void AttachToOffHand(MountedWeapon mount)
    {
        if (mount?.instance == null || !offHandSocket)
            return;

        var transform = mount.instance.transform;
        transform.SetParent(offHandSocket, false);
        WeaponHandLayout.Apply(transform, mount.weaponData.category, isOffHand: true);
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
