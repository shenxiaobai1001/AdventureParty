using System.Collections.Generic;

public static class IconRenderCatalog
{
    static readonly SyntyEquipmentSlot[] SlotOrder =
    {
        SyntyEquipmentSlot.Head,
        SyntyEquipmentSlot.Body,
        SyntyEquipmentSlot.Forearm,
        SyntyEquipmentSlot.Hips,
        SyntyEquipmentSlot.Leg,
        SyntyEquipmentSlot.Back,
    };

    public static List<IconRenderEntry> BuildFromEquipmentSets()
    {
        var result = new List<IconRenderEntry>();

        if (!EquipmentData.Instance.EnsureLoaded())
            return result;

        var list = EquipmentData.Instance.sets.GetListInfo();
        foreach (var row in list)
        {
            if (row == null)
                continue;

            foreach (var slot in SlotOrder)
            {
                var parts = row.GetSlotParts(slot);
                if (parts == null || parts.Length == 0)
                    continue;

                var hasAny = false;
                foreach (var part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        hasAny = true;
                        break;
                    }
                }

                if (!hasAny)
                    continue;

                result.Add(new IconRenderEntry
                {
                    setId = row.id,
                    setName = row.name,
                    slot = slot,
                    parts = parts,
                });
            }
        }

        return result;
    }
}
