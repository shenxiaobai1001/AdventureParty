using UnityEngine;

/// <summary>
/// Blade / limb segment used for melee capsule sweeps.
/// Auto-fit from mesh bounds when tip/root not set.
/// Manual tune: adjust <see cref="localRoot"/> / <see cref="localTip"/> / <see cref="radius"/>
/// on this component, or overrides on <see cref="SyntyWeaponItemData"/>.
/// </summary>
public class MeleeSweepSource : MonoBehaviour
{
    [Header("Capsule (local space of this transform)")]
    [Tooltip("Near end of the blade / fist (grip side).")]
    public Vector3 localRoot = Vector3.zero;

    [Tooltip("Far end of the blade / fist (tip / knuckles).")]
    public Vector3 localTip = new Vector3(0f, 0f, 0.75f);

    [Tooltip("Capsule radius around the blade axis.")]
    public float radius = 0.08f;

    [Header("State")]
    [Tooltip("When false, ignored by MeleeAttackController (ranged/shield).")]
    public bool contributesToMelee = true;

    [Tooltip("Weapon item that owns this instance (optional).")]
    public SyntyWeaponItemData weaponData;

    [Tooltip("Limb sources stay on bones; weapon sources live on drawn meshes.")]
    public bool isLimb;

    [SerializeField] bool autoFitted;
    [SerializeField] bool drawGizmos = true;

    Vector3 _prevRootWorld;
    Vector3 _prevTipWorld;
    bool _hasPrev;

    public Vector3 RootWorld => transform.TransformPoint(localRoot);
    public Vector3 TipWorld => transform.TransformPoint(localTip);

    public void CapturePrevious()
    {
        _prevRootWorld = RootWorld;
        _prevTipWorld = TipWorld;
        _hasPrev = true;
    }

    public void ResetPrevious()
    {
        _hasPrev = false;
    }

    public bool TryGetSweepSegment(out Vector3 rootA, out Vector3 tipA, out Vector3 rootB, out Vector3 tipB)
    {
        rootB = RootWorld;
        tipB = TipWorld;
        if (_hasPrev)
        {
            rootA = _prevRootWorld;
            tipA = _prevTipWorld;
        }
        else
        {
            rootA = rootB;
            tipA = tipB;
        }

        return true;
    }

    public void ApplyFromWeaponData(SyntyWeaponItemData data)
    {
        weaponData = data;
        if (!data)
            return;

        if (MeleeSweepDefaults.IsRangedOrNonMelee(data.category))
        {
            contributesToMelee = false;
            return;
        }

        contributesToMelee = true;

        if (data.overrideMeleeSweep)
        {
            localRoot = data.meleeSweepLocalRoot;
            localTip = data.meleeSweepLocalTip;
            radius = Mathf.Max(0.02f, data.meleeSweepRadius);
            autoFitted = false;
            return;
        }

        if (!autoFitted)
            AutoFitFromRenderers(data.category);
    }

    public void ApplyLimbDefaults(float length, float rad)
    {
        isLimb = true;
        contributesToMelee = true;
        localRoot = Vector3.zero;
        localTip = new Vector3(0f, 0f, length);
        radius = rad;
        autoFitted = true;
    }

    public void AutoFitFromRenderers(WeaponCategory category)
    {
        MeleeSweepDefaults.GetDefaults(category, out var defRoot, out var defTip, out var defRadius);

        var renderers = GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
        {
            localRoot = defRoot;
            localTip = defTip;
            radius = defRadius;
            autoFitted = true;
            return;
        }

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        // Longest world-axis of AABB, expressed in this local space via tip/root along that axis.
        var size = bounds.size;
        int axis = 0;
        if (size.y >= size.x && size.y >= size.z)
            axis = 1;
        else if (size.z >= size.x && size.z >= size.y)
            axis = 2;

        var worldRoot = bounds.center;
        var worldTip = bounds.center;
        var ext = bounds.extents;
        if (axis == 0)
        {
            worldRoot.x -= ext.x * 0.85f;
            worldTip.x += ext.x * 0.85f;
        }
        else if (axis == 1)
        {
            worldRoot.y -= ext.y * 0.85f;
            worldTip.y += ext.y * 0.85f;
        }
        else
        {
            worldRoot.z -= ext.z * 0.85f;
            worldTip.z += ext.z * 0.85f;
        }

        localRoot = transform.InverseTransformPoint(worldRoot);
        localTip = transform.InverseTransformPoint(worldTip);

        var minExtent = Mathf.Min(size.x, size.y, size.z);
        radius = Mathf.Clamp(minExtent * 0.25f, 0.04f, Mathf.Max(defRadius, 0.14f));
        autoFitted = true;
    }

    public static MeleeSweepSource EnsureOnWeapon(GameObject weaponInstance, SyntyWeaponItemData data)
    {
        if (!weaponInstance)
            return null;

        var source = weaponInstance.GetComponent<MeleeSweepSource>();
        if (!source)
            source = weaponInstance.AddComponent<MeleeSweepSource>();

        source.ApplyFromWeaponData(data);
        return source;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = contributesToMelee ? new Color(1f, 0.4f, 0.1f, 0.85f) : new Color(0.5f, 0.5f, 0.5f, 0.4f);
        var a = RootWorld;
        var b = TipWorld;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, radius);
        Gizmos.DrawWireSphere(b, radius);
    }
#endif
}
