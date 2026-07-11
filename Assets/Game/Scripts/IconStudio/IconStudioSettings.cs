using UnityEngine;

public static class IconStudioSettings
{
    public const string StaticPartsRoot =
        "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Characters_ModularParts_Static";

    public const string OutputRoot = "Assets/Game/Art/Icons/Equipment";

    public const int PixelsPerCell = 64;

    public const float CameraDistance = 8f;

    public const float MinCameraStandOff = 4f;

    public const float NearPlaneMargin = 0.75f;

    public const float PreviewExtraPadding = 0.25f;

    /// <summary>Orthographic half-height multiplier — larger values pull the camera farther back.</summary>
    public const float FramePadding = 1.75f;

    public static readonly Vector3 DefaultFrontEuler = new Vector3(0f, 180f, 0f);

    public static readonly Vector3 BackSlotEuler = new Vector3(0f, 0f, 0f);

    public static Quaternion GetDefaultStageRotation(SyntyEquipmentSlot slot)
    {
        return slot == SyntyEquipmentSlot.Back
            ? Quaternion.Euler(BackSlotEuler)
            : Quaternion.Euler(DefaultFrontEuler);
    }

    public static float GetFramePadding(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Body:
            case SyntyEquipmentSlot.Hips:
            case SyntyEquipmentSlot.Forearm:
                return FramePadding + 0.08f;
            default:
                return FramePadding;
        }
    }

    public static Vector2Int GetGridSize(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: return new Vector2Int(3, 3);
            case SyntyEquipmentSlot.Body: return new Vector2Int(4, 6);
            case SyntyEquipmentSlot.Forearm: return new Vector2Int(2, 3);
            case SyntyEquipmentSlot.Hips: return new Vector2Int(4, 5);
            case SyntyEquipmentSlot.Leg: return new Vector2Int(4, 3);
            case SyntyEquipmentSlot.Back: return new Vector2Int(4, 3);
            default: return new Vector2Int(3, 3);
        }
    }

    public static Vector2Int GetOutputPixelSize(SyntyEquipmentSlot slot)
    {
        var grid = GetGridSize(slot);
        return new Vector2Int(grid.x * PixelsPerCell, grid.y * PixelsPerCell);
    }
}
