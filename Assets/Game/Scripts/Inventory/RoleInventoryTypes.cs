using System.Collections.Generic;
using UInventoryGrid;

/// <summary>
/// Maps UIRolePanel grid names to inventory item types for the role equipment system.
/// </summary>
public static class RoleInventoryTypes
{
    static readonly Dictionary<string, ItemType> GridItemTypes = new Dictionary<string, ItemType>
    {
        { "Helmet", ItemType.Head },
        { "Body", ItemType.Body },
        { "Hips", ItemType.Hips },
        { "Legs", ItemType.Leg },
        { "Forearm", ItemType.Forearm },
        { "Back", ItemType.BackSlot },
        { "Weapon", ItemType.Weapon },
        { "NormalBack", ItemType.All },
    };

    static readonly HashSet<string> LegacyGridNames = new HashSet<string>
    {
        "Shoulder",
        "Weapon1",
        "Weapon2",
    };

    public static bool TryGetItemTypeForGrid(string gridName, out ItemType itemType)
    {
        if (GridItemTypes.TryGetValue(gridName, out itemType))
            return true;

        itemType = ItemType.All;
        return false;
    }

    public static void ConfigureGrid(InventoryGrid grid)
    {
        if (!grid)
            return;

        grid.allowedItemTypes.Clear();

        if (!TryGetItemTypeForGrid(grid.name, out var itemType) || itemType == ItemType.All)
        {
            grid.allowedItemTypes.Add(ItemType.All);
            return;
        }

        grid.allowedItemTypes.Add(itemType);

        if (grid.name == "Body")
            grid.allowedItemTypes.Add(ItemType.Shoulder);

        if (grid.name == "Weapon")
        {
            grid.allowedItemTypes.Add(ItemType.WeaponPrimary);
            grid.allowedItemTypes.Add(ItemType.WeaponSecondary);
        }
    }

    public static void ConfigureAllGrids(InventoryGrid[] grids)
    {
        if (grids == null)
            return;

        foreach (var grid in grids)
            ConfigureGrid(grid);
    }

    public static bool IsEquipmentGrid(string gridName)
    {
        return gridName != "NormalBack"
            && gridName != "Weapon"
            && GridItemTypes.ContainsKey(gridName);
    }

    public static bool IsWeaponGrid(string gridName)
    {
        return gridName == "Weapon";
    }

    public static string NormalizeGridId(string gridId)
    {
        if (string.IsNullOrEmpty(gridId))
            return gridId;

        switch (gridId)
        {
            case "Shoulder":
                return "Body";
            case "Weapon1":
            case "Weapon2":
                return "Weapon";
            default:
                return gridId;
        }
    }

    public static bool IsLegacyGridId(string gridId)
    {
        return LegacyGridNames.Contains(gridId);
    }
}
