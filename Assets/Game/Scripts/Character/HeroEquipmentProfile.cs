using UnityEngine;

[CreateAssetMenu(fileName = "HeroEquipment", menuName = "AdventureParty/Hero Equipment Profile")]
public class HeroEquipmentProfile : ScriptableObject
{
    [Tooltip("Equipment set index. Each number represents one full equipment set.")]
    public int setIndex;

    public string setName = "Equipment";

    [Header("1 - Head (HelmetAttachment;HeadCovering)")]
    public string head;

    [Header("2 - Body (ShoulderAttach;Torso;ArmUpperRight;ArmUpperLeft)")]
    public string body;

    [Header("Legacy shoulder field (merged into body at runtime)")]
    public string shoulder;

    [Header("3 - Forearm (ArmLowerRight;ArmLowerLeft;HandRight;HandLeft; optional Hips)")]
    public string forearm;

    [Header("4 - Hips (Hips;HipsAttachment)")]
    public string hips;

    [Header("5 - Leg (LegRight;LegLeft; optional KneeAttach)")]
    public string leg;

    [Header("6 - Back (BackAttachment)")]
    public string back;

    public bool hideHairWhenHeadEquipped;

    public string GetResolvedBody()
    {
        return EquipmentPartParser.MergeCombined(body, shoulder);
    }

    [Header("8 - Main Hand (legacy, use HeroWeaponVisual on character)")]
    public GameObject mainHandPrefab;

    [Header("9 - Off Hand (legacy, use HeroWeaponVisual on character)")]
    public GameObject offHandPrefab;

    public void ApplyFromSlotGroups(System.Collections.Generic.Dictionary<SyntyEquipmentSlot, System.Collections.Generic.List<string>> groups)
    {
        head = JoinSlot(groups, SyntyEquipmentSlot.Head);
        body = EquipmentPartParser.MergeCombined(
            JoinSlot(groups, SyntyEquipmentSlot.Body),
            JoinSlot(groups, SyntyEquipmentSlot.Shoulder));
        shoulder = string.Empty;
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
