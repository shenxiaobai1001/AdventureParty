using UnityEngine;

public static class WeaponIconStudioSettings
{
    public const string OutputRoot = "Assets/Game/Art/Icons/Weapons";

    public const int PixelsPerCell = 64;

    public const float CameraDistance = 8f;

    public const float MinCameraStandOff = 4f;

    public const float NearPlaneMargin = 0.75f;

    public const float PreviewExtraPadding = 0.25f;

    public const float FramePadding = 1.75f;

    public static Vector2Int GetOutputPixelSize(Vector2Int gridSize)
    {
        return new Vector2Int(gridSize.x * PixelsPerCell, gridSize.y * PixelsPerCell);
    }

    public static float GetFramePadding(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Shield:
            case WeaponCategory.Bow:
            case WeaponCategory.Crossbow:
                return FramePadding + 0.05f;
            case WeaponCategory.Spear:
            case WeaponCategory.Staff:
            case WeaponCategory.GreatSword:
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe:
            case WeaponCategory.LongGun:
                return FramePadding + 0.1f;
            default:
                return FramePadding;
        }
    }
}
