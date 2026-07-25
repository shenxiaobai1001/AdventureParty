using System;
using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

[Serializable]
public class PlacedInventoryItemData
{
    public string instanceId;
    public int equipmentItemId;
    public ItemData itemData;
    public string gridId;
    public Vector2Int position;
    public bool rotated;
    public int stackCount = 1;
}

[Serializable]
public class CharacterInventoryData
{
    public List<PlacedInventoryItemData> items = new List<PlacedInventoryItemData>();
    public float maxCarryWeight = 39f;

    /// <summary>Persisted weapon-bar equip (hand slots). ItemData refs to SyntyWeaponItemData.</summary>
    public ItemData equippedRightWeapon;
    public ItemData equippedLeftWeapon;

    public float GetTotalWeight()
    {
        var total = 0f;
        foreach (var item in items)
        {
            if (!item.itemData)
                continue;

            total += item.itemData.weight * Mathf.Max(1, item.stackCount);
        }

        return total;
    }

    public string GetWeightStateLabel(float totalWeight)
    {
        if (maxCarryWeight <= 0f)
            return "轻";

        var ratio = totalWeight / maxCarryWeight;
        if (ratio >= 1f)
            return "重";
        if (ratio >= 0.7f)
            return "中";

        return "轻";
    }
}
