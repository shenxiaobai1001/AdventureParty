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
        { "Shoulder", ItemType.Shoulder },
        { "Body", ItemType.Body },
        { "Hips", ItemType.Hips },
        { "Legs", ItemType.Leg },
        { "Forearm", ItemType.Forearm },
        { "Back", ItemType.BackSlot },
        { "Weapon1", ItemType.WeaponPrimary },
        { "Weapon2", ItemType.WeaponSecondary },
        { "NormalBack", ItemType.All },
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
        return gridName != "NormalBack" && GridItemTypes.ContainsKey(gridName);
    }
}
