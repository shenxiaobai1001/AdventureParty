using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SyntyPartCatalog", menuName = "AdventureParty/Synty Equipment Part Catalog")]
public class SyntyEquipmentPartCatalog : ScriptableObject
{
    [Header("1 - Head")]
    public string[] headParts = System.Array.Empty<string>();

    [Header("2 - Body")]
    public string[] bodyParts = System.Array.Empty<string>();

    [Header("3 - Shoulder")]
    public string[] shoulderParts = System.Array.Empty<string>();

    [Header("4 - Forearm")]
    public string[] forearmParts = System.Array.Empty<string>();

    [Header("5 - Hips")]
    public string[] hipsParts = System.Array.Empty<string>();

    [Header("6 - Leg")]
    public string[] legParts = System.Array.Empty<string>();

    [Header("7 - Back")]
    public string[] backParts = System.Array.Empty<string>();

    public string[] GetParts(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: return headParts;
            case SyntyEquipmentSlot.Body: return bodyParts;
            case SyntyEquipmentSlot.Shoulder: return shoulderParts;
            case SyntyEquipmentSlot.Forearm: return forearmParts;
            case SyntyEquipmentSlot.Hips: return hipsParts;
            case SyntyEquipmentSlot.Leg: return legParts;
            case SyntyEquipmentSlot.Back: return backParts;
            default: return System.Array.Empty<string>();
        }
    }

    public void SetParts(SyntyEquipmentSlot slot, string[] parts)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: headParts = parts; break;
            case SyntyEquipmentSlot.Body: bodyParts = parts; break;
            case SyntyEquipmentSlot.Shoulder: shoulderParts = parts; break;
            case SyntyEquipmentSlot.Forearm: forearmParts = parts; break;
            case SyntyEquipmentSlot.Hips: hipsParts = parts; break;
            case SyntyEquipmentSlot.Leg: legParts = parts; break;
            case SyntyEquipmentSlot.Back: backParts = parts; break;
        }
    }
}

[System.Serializable]
public class EquipmentSetEntry
{
    public int setIndex;
    public string setName;
    public string head;
    public string body;
    public string shoulder;
    public string forearm;
    public string hips;
    public string leg;
    public string back;

    public void CopyTo(HeroEquipmentProfile profile)
    {
        EquipmentSetRow.FromEntry(this)?.CopyTo(profile);
    }

    public static EquipmentSetEntry FromProfile(HeroEquipmentProfile profile)
    {
        return new EquipmentSetEntry
        {
            setIndex = profile.setIndex,
            setName = profile.setName,
            head = profile.head,
            body = profile.body,
            shoulder = profile.shoulder,
            forearm = profile.forearm,
            hips = profile.hips,
            leg = profile.leg,
            back = profile.back,
        };
    }
}

[CreateAssetMenu(fileName = "EquipmentSetDatabase", menuName = "AdventureParty/Equipment Set Database")]
public class EquipmentSetDatabase : ScriptableObject
{
    public List<EquipmentSetEntry> sets = new List<EquipmentSetEntry>();

    public EquipmentSetEntry GetSet(int setIndex)
    {
        foreach (var entry in sets)
        {
            if (entry.setIndex == setIndex)
                return entry;
        }

        return null;
    }
}
