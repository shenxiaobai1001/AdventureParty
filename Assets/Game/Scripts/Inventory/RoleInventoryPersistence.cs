using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

public static class RoleInventoryPersistence
{
    public static CharacterInventoryData Export(Inventory inventory)
    {
        var data = new CharacterInventoryData();
        if (!inventory)
            return data;

        var seen = new HashSet<Item>();
        foreach (var item in inventory.items)
        {
            if (!item || !seen.Add(item) || !item.data || !item.inventoryGrid)
                continue;

            var equipmentItem = item.data as SyntyEquipmentItemData;
            data.items.Add(new PlacedInventoryItemData
            {
                instanceId = System.Guid.NewGuid().ToString("N"),
                equipmentItemId = equipmentItem ? equipmentItem.equipmentItemId : 0,
                itemData = item.data,
                gridId = item.inventoryGrid.name,
                position = item.indexPosition,
                rotated = item.isRotated,
                stackCount = item.stackCount
            });
        }

        return data;
    }

    public static void Import(Inventory inventory, CharacterInventoryData data)
    {
        if (!inventory)
            return;

        inventory.ClearAllItems();

        if (data == null || data.items == null)
            return;

        foreach (var record in data.items)
        {
            if (!record.itemData || string.IsNullOrEmpty(record.gridId))
                continue;

            var grid = inventory.FindGridByName(record.gridId);
            if (!grid)
                continue;

            inventory.PlaceItemInGrid(
                grid,
                record.position,
                record.itemData,
                Mathf.Max(1, record.stackCount),
                record.rotated);
        }
    }

    public static float GetLiveTotalWeight(Inventory inventory)
    {
        if (!inventory)
            return 0f;

        var total = 0f;
        var seen = new HashSet<Item>();
        foreach (var item in inventory.items)
        {
            if (!item || !seen.Add(item) || !item.data)
                continue;

            total += item.data.weight * Mathf.Max(1, item.stackCount);
        }

        return total;
    }
}
