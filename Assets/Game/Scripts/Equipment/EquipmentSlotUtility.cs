using UInventoryGrid;

/// <summary>
/// Maps between Synty slots, UI grid names, and inventory item types.
/// </summary>
public static class EquipmentSlotUtility
{
    public static int ComposeItemId(int setId, SyntyEquipmentSlot slot)
    {
        return setId * 100 + (int)slot;
    }

    public static bool TryParseItemId(int itemId, out int setId, out SyntyEquipmentSlot slot)
    {
        setId = itemId / 100;
        var slotValue = itemId % 100;
        if (slotValue < (int)SyntyEquipmentSlot.Head || slotValue > (int)SyntyEquipmentSlot.Back)
        {
            slot = default;
            return false;
        }

        slot = (SyntyEquipmentSlot)slotValue;
        return setId > 0;
    }

    public static string GetSlotLabel(SyntyEquipmentSlot slot)
    {
        return IconRenderEntry.GetSlotLabel(slot);
    }

    public static string GetGridName(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: return "Helmet";
            case SyntyEquipmentSlot.Body: return "Body";
            case SyntyEquipmentSlot.Shoulder: return "Shoulder";
            case SyntyEquipmentSlot.Forearm: return "Forearm";
            case SyntyEquipmentSlot.Hips: return "Hips";
            case SyntyEquipmentSlot.Leg: return "Legs";
            case SyntyEquipmentSlot.Back: return "Back";
            default: return slot.ToString();
        }
    }

    public static bool TryGetSlotFromGridName(string gridName, out SyntyEquipmentSlot slot)
    {
        switch (gridName)
        {
            case "Helmet": slot = SyntyEquipmentSlot.Head; return true;
            case "Body": slot = SyntyEquipmentSlot.Body; return true;
            case "Shoulder": slot = SyntyEquipmentSlot.Shoulder; return true;
            case "Forearm": slot = SyntyEquipmentSlot.Forearm; return true;
            case "Hips": slot = SyntyEquipmentSlot.Hips; return true;
            case "Legs": slot = SyntyEquipmentSlot.Leg; return true;
            case "Back": slot = SyntyEquipmentSlot.Back; return true;
            default:
                slot = default;
                return false;
        }
    }

    public static bool TryParseSlot(string slotName, out SyntyEquipmentSlot slot)
    {
        if (string.IsNullOrWhiteSpace(slotName))
        {
            slot = default;
            return false;
        }

        return System.Enum.TryParse(slotName.Trim(), true, out slot);
    }

    public static ItemType ToItemType(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: return ItemType.Head;
            case SyntyEquipmentSlot.Body: return ItemType.Body;
            case SyntyEquipmentSlot.Shoulder: return ItemType.Shoulder;
            case SyntyEquipmentSlot.Forearm: return ItemType.Forearm;
            case SyntyEquipmentSlot.Hips: return ItemType.Hips;
            case SyntyEquipmentSlot.Leg: return ItemType.Leg;
            case SyntyEquipmentSlot.Back: return ItemType.BackSlot;
            default: return ItemType.All;
        }
    }

    public static string GetItemFileStem(int setId, SyntyEquipmentSlot slot)
    {
        return $"set_{setId:D3}_{GetSlotLabel(slot)}";
    }
}
