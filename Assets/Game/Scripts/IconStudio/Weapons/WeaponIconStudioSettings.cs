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
                return FramePadding + 0.05f;
            case WeaponCategory.Polearm2H:
            case WeaponCategory.GreatSword2H:
            case WeaponCategory.HeavyWeapon2H:
            case WeaponCategory.FirearmRifle:
                return FramePadding + 0.1f;
            default:
                return FramePadding;
        }
    }
}
