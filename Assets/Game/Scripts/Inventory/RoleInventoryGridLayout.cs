using UnityEngine;

/// <summary>
/// Canonical UIRolePanel inventory grid sizes and names.
/// Visual RectTransform sizes live on the prefab; logical placement uses these grid dimensions.
/// </summary>
public static class RoleInventoryGridLayout
{
    public const string HelmetGrid = "Helmet";
    public const string BodyGrid = "Body";
    public const string ForearmGrid = "Forearm";
    public const string HipsGrid = "Hips";
    public const string LegsGrid = "Legs";
    public const string BackGrid = "Back";
    public const string WeaponGrid = "Weapon";
    public const string NormalBackGrid = "NormalBack";

    public static readonly Vector2Int HelmetSize = new Vector2Int(3, 3);
    public static readonly Vector2Int BodySize = new Vector2Int(4, 6);
    public static readonly Vector2Int ForearmSize = new Vector2Int(2, 3);
    public static readonly Vector2Int HipsSize = new Vector2Int(4, 5);
    public static readonly Vector2Int LegsSize = new Vector2Int(4, 3);
    public static readonly Vector2Int BackSize = new Vector2Int(4, 3);
    public static readonly Vector2Int WeaponSize = new Vector2Int(10, 4);
    public static readonly Vector2Int NormalBackSize = new Vector2Int(16, 6);

    public static Vector2Int GetCanonicalSize(string gridName)
    {
        switch (gridName)
        {
            case HelmetGrid: return HelmetSize;
            case BodyGrid: return BodySize;
            case ForearmGrid: return ForearmSize;
            case HipsGrid: return HipsSize;
            case LegsGrid: return LegsSize;
            case BackGrid: return BackSize;
            case WeaponGrid: return WeaponSize;
            case NormalBackGrid: return NormalBackSize;
            default: return new Vector2Int(1, 1);
        }
    }
}
