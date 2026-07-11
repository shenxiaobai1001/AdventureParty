using System.Collections.Generic;

/// <summary>
/// Six non-weapon equipment slots for Synty modular hero parts.
/// Body includes shoulder attachments, torso, and upper arms.
/// Multiple part names in one slot are stored semicolon-separated.
/// </summary>
public enum SyntyEquipmentSlot
{
    Head = 1,
    Body = 2,
    /// <summary>Legacy slot id kept for old item ids (setId*100+3). Parts belong to Body.</summary>
    Shoulder = 3,
    Forearm = 4,
    Hips = 5,
    Leg = 6,
    Back = 7,
}

public static class SyntyEquipmentPartClassifier
{
    public static SyntyEquipmentSlot? Classify(string partName)
    {
        if (string.IsNullOrEmpty(partName))
            return null;

        if (partName.StartsWith("Chr_HelmetAttachment_") || partName.StartsWith("Chr_HeadCoverings_"))
            return SyntyEquipmentSlot.Head;

        if (partName.StartsWith("Chr_Torso_")
            || partName.StartsWith("Chr_ArmUpperRight_")
            || partName.StartsWith("Chr_ArmUpperLeft_")
            || partName.StartsWith("Chr_ShoulderAttachRight_")
            || partName.StartsWith("Chr_ShoulderAttachLeft_"))
            return SyntyEquipmentSlot.Body;

        if (partName.StartsWith("Chr_ArmLowerRight_")
            || partName.StartsWith("Chr_ArmLowerLeft_")
            || partName.StartsWith("Chr_HandRight_")
            || partName.StartsWith("Chr_HandLeft_"))
            return SyntyEquipmentSlot.Forearm;

        if (partName.StartsWith("Chr_Hips_") || partName.StartsWith("Chr_HipsAttachment_"))
            return SyntyEquipmentSlot.Hips;

        if (partName.StartsWith("Chr_LegRight_")
            || partName.StartsWith("Chr_LegLeft_")
            || partName.StartsWith("Chr_KneeAttachRight_")
            || partName.StartsWith("Chr_KneeAttachLeft_"))
            return SyntyEquipmentSlot.Leg;

        if (partName.StartsWith("Chr_BackAttachment_"))
            return SyntyEquipmentSlot.Back;

        return null;
    }

    public static bool IsAttachmentPart(string partName)
    {
        return partName.StartsWith("Chr_HelmetAttachment_")
            || partName.StartsWith("Chr_HeadCoverings_")
            || partName.StartsWith("Chr_ShoulderAttach")
            || partName.StartsWith("Chr_BackAttachment_")
            || partName.StartsWith("Chr_HipsAttachment_")
            || partName.StartsWith("Chr_KneeAttach");
    }

    public static void SortPartsForSlot(SyntyEquipmentSlot slot, List<string> parts)
    {
        parts.Sort((a, b) => GetPartOrder(NormalizeSlot(slot), a).CompareTo(GetPartOrder(NormalizeSlot(slot), b)));
    }

    public static SyntyEquipmentSlot NormalizeSlot(SyntyEquipmentSlot slot)
    {
        return slot == SyntyEquipmentSlot.Shoulder ? SyntyEquipmentSlot.Body : slot;
    }

    static int GetPartOrder(SyntyEquipmentSlot slot, string partName)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Body:
                if (partName.Contains("Torso_")) return 0;
                if (partName.Contains("ShoulderAttachRight_")) return 1;
                if (partName.Contains("ShoulderAttachLeft_")) return 2;
                if (partName.Contains("ArmUpperRight_")) return 3;
                if (partName.Contains("ArmUpperLeft_")) return 4;
                return 5;

            case SyntyEquipmentSlot.Forearm:
                if (partName.Contains("ArmLowerRight_")) return 0;
                if (partName.Contains("ArmLowerLeft_")) return 1;
                if (partName.Contains("HandRight_")) return 2;
                if (partName.Contains("HandLeft_")) return 3;
                return 4;

            case SyntyEquipmentSlot.Leg:
                if (partName.Contains("LegRight_")) return 0;
                if (partName.Contains("KneeAttachRight_")) return 1;
                if (partName.Contains("LegLeft_")) return 2;
                if (partName.Contains("KneeAttachLeft_")) return 3;
                return 4;

            default:
                return 0;
        }
    }
}
