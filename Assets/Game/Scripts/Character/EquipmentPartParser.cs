using System.Collections.Generic;

public static class EquipmentPartParser
{
    public const char Separator = ';';

    public static string[] Split(string combined)
    {
        if (string.IsNullOrWhiteSpace(combined))
            return System.Array.Empty<string>();

        return combined.Split(Separator);
    }

    public static string Join(IEnumerable<string> parts)
    {
        if (parts == null)
            return string.Empty;

        var list = new List<string>();
        foreach (var part in parts)
        {
            if (!string.IsNullOrWhiteSpace(part))
                list.Add(part.Trim());
        }

        return string.Join(Separator.ToString(), list);
    }

    public static Dictionary<SyntyEquipmentSlot, List<string>> GroupBySlot(IEnumerable<string> partNames)
    {
        var groups = new Dictionary<SyntyEquipmentSlot, List<string>>();

        foreach (var partName in partNames)
        {
            if (string.IsNullOrWhiteSpace(partName))
                continue;

            var slot = SyntyEquipmentPartClassifier.Classify(partName.Trim());
            if (!slot.HasValue)
                continue;

            if (!groups.TryGetValue(slot.Value, out var list))
            {
                list = new List<string>();
                groups[slot.Value] = list;
            }

            list.Add(partName.Trim());
        }

        foreach (var pair in groups)
            SyntyEquipmentPartClassifier.SortPartsForSlot(pair.Key, pair.Value);

        return groups;
    }
}
