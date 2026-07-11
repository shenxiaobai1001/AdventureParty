using UnityEngine;

/// <summary>
/// Icon framing via bone transform after mannequin dress. Delegates to EquipmentSlotPose.
/// </summary>
public static class IconStudioMannequinPose
{
    public static void Apply(Transform root, SyntyEquipmentSlot slot)
    {
        EquipmentSlotPose.Apply(root, slot);
    }
}
