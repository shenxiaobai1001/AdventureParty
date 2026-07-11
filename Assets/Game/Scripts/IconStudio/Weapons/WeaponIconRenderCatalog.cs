using System.Collections.Generic;

public static class WeaponIconRenderCatalog
{
    public static List<WeaponIconRenderEntry> BuildFromWeaponItems()
    {
        var result = new List<WeaponIconRenderEntry>();

        if (!WeaponItemData.Instance.EnsureLoaded())
            return result;

        foreach (var row in WeaponItemData.Instance.items.GetListInfo())
        {
            if (row == null || string.IsNullOrWhiteSpace(row.GetSyntyPrefabAssetPath()))
                continue;

            result.Add(WeaponIconRenderEntry.FromRow(row));
        }

        return result;
    }
}
