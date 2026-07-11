using System;

[Serializable]
public class IconRenderEntry
{
    public int setId;
    public string setName;
    public SyntyEquipmentSlot slot;
    public string[] parts;

    public string DisplayLabel => $"{setId:000} {setName} — {GetSlotLabel(slot)}";

    public string FileName => $"set_{setId:D3}_{slot}.png";

    public static string GetSlotLabel(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: return "Head";
            case SyntyEquipmentSlot.Body: return "Body";
            case SyntyEquipmentSlot.Shoulder: return "Shoulder";
            case SyntyEquipmentSlot.Forearm: return "Forearm";
            case SyntyEquipmentSlot.Hips: return "Hips";
            case SyntyEquipmentSlot.Leg: return "Leg";
            case SyntyEquipmentSlot.Back: return "Back";
            default: return slot.ToString();
        }
    }
}
