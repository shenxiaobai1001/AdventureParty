using UnityEngine;

[CreateAssetMenu(fileName = "HeroEquipment", menuName = "AdventureParty/Hero Equipment Profile")]
public class HeroEquipmentProfile : ScriptableObject
{
    [Tooltip("Equipment set index. Each number represents one full equipment set.")]
    public int setIndex;

    public string setName = "Equipment";

    [Header("1 - Head (HelmetAttachment;HeadCovering)")]
    public string head;

    [Header("2 - Body (Torso;ArmUpperRight;ArmUpperLeft)")]
    public string body;

    [Header("3 - Shoulder (ShoulderAttachRight;ShoulderAttachLeft)")]
    public string shoulder;

    [Header("4 - Forearm (ArmLowerRight;ArmLowerLeft;HandRight;HandLeft; optional Hips)")]
    public string forearm;

    [Header("5 - Hips (Hips;HipsAttachment)")]
    public string hips;

    [Header("6 - Leg (LegRight;LegLeft; optional KneeAttach)")]
    public string leg;

    [Header("7 - Back (BackAttachment)")]
    public string back;

    public bool hideHairWhenHeadEquipped;

    [Header("8 - Main Hand (legacy, use HeroWeaponVisual on character)")]
    public GameObject mainHandPrefab;

    [Header("9 - Off Hand (legacy, use HeroWeaponVisual on character)")]
    public GameObject offHandPrefab;

    public void ApplyFromSlotGroups(System.Collections.Generic.Dictionary<SyntyEquipmentSlot, System.Collections.Generic.List<string>> groups)
    {
        head = JoinSlot(groups, SyntyEquipmentSlot.Head);
        body = JoinSlot(groups, SyntyEquipmentSlot.Body);
        shoulder = JoinSlot(groups, SyntyEquipmentSlot.Shoulder);
        forearm = JoinSlot(groups, SyntyEquipmentSlot.Forearm);
        hips = JoinSlot(groups, SyntyEquipmentSlot.Hips);
        leg = JoinSlot(groups, SyntyEquipmentSlot.Leg);
        back = JoinSlot(groups, SyntyEquipmentSlot.Back);
    }

    static string JoinSlot(
        System.Collections.Generic.Dictionary<SyntyEquipmentSlot, System.Collections.Generic.List<string>> groups,
        SyntyEquipmentSlot slot)
    {
        if (!groups.TryGetValue(slot, out var parts) || parts == null || parts.Count == 0)
            return string.Empty;

        SyntyEquipmentPartClassifier.SortPartsForSlot(slot, parts);
        return EquipmentPartParser.Join(parts);
    }
}
