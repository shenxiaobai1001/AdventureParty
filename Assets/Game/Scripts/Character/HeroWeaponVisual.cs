using System;
using UnityEngine;

/// <summary>
/// Two weapon slots: back mount sockets (casual) and hand mount sockets (combat, tunable rotation).
/// </summary>
public class HeroWeaponVisual : MonoBehaviour
{
    public enum AttachTarget
    {
        Hand,
        BackMount,
    }

    [Serializable]
    public class WeaponSlot
    {
        public string displayName = "Weapon";

        [Tooltip("Back mount socket on spine/back bone. Weapon rests here in casual stance.")]
        public Transform backMountSocket;

        [Tooltip("Hand mount socket (child of hand bone) for tuning grip angle in combat.")]
        public Transform handSocket;

        [Tooltip("Weapon prefab spawned at back mount on play. Ignored if Weapon In Scene is set.")]
        public GameObject weaponPrefab;

        [Tooltip("Optional weapon already placed under back mount in the prefab hierarchy.")]
        public Transform weaponInScene;

        [NonSerialized] GameObject _instance;
        [NonSerialized] AttachTarget? _pendingAttach;

        public bool HasWeapon => _instance || weaponInScene;

        public Transform ActiveTransform => _instance ? _instance.transform : weaponInScene;

        public void ResolveSceneWeapon()
        {
            if (weaponInScene || !backMountSocket)
                return;

            for (var i = 0; i < backMountSocket.childCount; i++)
            {
                var child = backMountSocket.GetChild(i);
                if (!child)
                    continue;

                weaponInScene = child;
                return;
            }
        }

        public void Setup(GameObject owner)
        {
            ResolveSceneWeapon();

            if (weaponInScene)
            {
                _instance = null;
                return;
            }

            ClearInstance(owner);

            if (!weaponPrefab || !backMountSocket)
                return;

            _instance = UnityEngine.Object.Instantiate(weaponPrefab, backMountSocket);
            ResetLocalTransform(_instance.transform);
        }

        public void Clear(GameObject owner)
        {
            if (weaponInScene)
                return;

            ClearInstance(owner);
            _pendingAttach = null;
        }

        public void RequestAttach(AttachTarget target)
        {
            _pendingAttach = target;
        }

        public void ApplyPendingAttach()
        {
            if (_pendingAttach == null)
                return;

            Attach(_pendingAttach.Value);
            _pendingAttach = null;
        }

        public void Attach(AttachTarget target)
        {
            var weapon = ActiveTransform;
            if (!weapon)
                return;

            var parent = target == AttachTarget.Hand ? handSocket : backMountSocket;
            if (!parent)
                return;

            weapon.SetParent(parent, false);
            ResetLocalTransform(weapon);
        }

        void ClearInstance(GameObject owner)
        {
            if (!_instance)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(_instance);
            else
                UnityEngine.Object.DestroyImmediate(_instance);

            _instance = null;
        }

        static void ResetLocalTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localEulerAngles = Vector3.zero;
            t.localScale = Vector3.one;
        }
    }

    [Header("Weapon Slots")]
    public WeaponSlot mainHand = new WeaponSlot { displayName = "Main Hand" };
    public WeaponSlot offHand = new WeaponSlot { displayName = "Off Hand" };

    [Header("Auto-create back mounts (when sockets missing)")]
    public Vector3 mainBackLocalPosition = new Vector3(0.06f, 0.08f, -0.12f);
    public Vector3 mainBackLocalEuler = new Vector3(-90f, 0f, 90f);
    public Vector3 offBackLocalPosition = new Vector3(-0.06f, 0.08f, -0.12f);
    public Vector3 offBackLocalEuler = new Vector3(-90f, 0f, -90f);

    [Header("Auto-create hand mounts (when sockets missing)")]
    public Vector3 mainHandLocalPosition = Vector3.zero;
    public Vector3 mainHandLocalEuler = Vector3.zero;
    public Vector3 offHandLocalPosition = Vector3.zero;
    public Vector3 offHandLocalEuler = Vector3.zero;

    public bool HasMainWeapon => mainHand.HasWeapon;

    void Awake()
    {
        EnsureSockets();
    }

    void Start()
    {
        PlaceWeaponsForCasualStance();
    }

    public void RefreshWeaponDetection()
    {
        EnsureSockets();
        mainHand.Setup(gameObject);
        offHand.Setup(gameObject);
    }

    public void PlaceWeaponsForCasualStance()
    {
        RefreshWeaponDetection();
        mainHand.Attach(AttachTarget.BackMount);
        offHand.Attach(AttachTarget.BackMount);
    }

    public void PlaceWeaponsForCombatStance()
    {
        mainHand.Attach(AttachTarget.Hand);
        offHand.Attach(AttachTarget.Hand);
    }

    public void ClearAllWeapons()
    {
        mainHand.Clear(gameObject);
        offHand.Clear(gameObject);
    }

    public void RequestAttachOnSwitch(AttachTarget target)
    {
        if (mainHand.HasWeapon)
            mainHand.RequestAttach(target);

        if (offHand.HasWeapon)
            offHand.RequestAttach(target);
    }

    public void ApplyPendingAttach()
    {
        mainHand.ApplyPendingAttach();
        offHand.ApplyPendingAttach();
    }

    void EnsureSockets()
    {
        var handRight = FindBone("Hand_R");
        var handLeft = FindBone("Hand_L");

        mainHand.handSocket = EnsureHandMountSocket(
            mainHand.handSocket, handRight, "Socket_WeaponHand_Main",
            mainHandLocalPosition, mainHandLocalEuler);

        offHand.handSocket = EnsureHandMountSocket(
            offHand.handSocket, handLeft, "Socket_WeaponHand_Off",
            offHandLocalPosition, offHandLocalEuler);

        if (!mainHand.backMountSocket)
            mainHand.backMountSocket = FindOrCreateBackSocket("Socket_WeaponMount_Main", mainBackLocalPosition, mainBackLocalEuler);

        if (!offHand.backMountSocket)
            offHand.backMountSocket = FindOrCreateBackSocket("Socket_WeaponMount_Off", offBackLocalPosition, offBackLocalEuler);
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

#if UNITY_EDITOR
    void OnValidate()
    {
        var mainSocket = FindBone("Socket_WeaponHand_Main");
        if (mainSocket && (!mainHand.handSocket || mainHand.handSocket.name == "Hand_R"))
            mainHand.handSocket = mainSocket;

        var offSocket = FindBone("Socket_WeaponHand_Off");
        if (offSocket && (!offHand.handSocket || offHand.handSocket.name == "Hand_L"))
            offHand.handSocket = offSocket;
    }
#endif
}
