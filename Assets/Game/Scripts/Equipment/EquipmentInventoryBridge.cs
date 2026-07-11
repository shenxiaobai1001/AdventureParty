using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

/// <summary>
/// Syncs role-panel inventory equipment grids with ModularHeroVisual on the bound hero.
/// </summary>
public static class EquipmentInventoryBridge
{
    static readonly string[] EquipmentGridNames =
    {
        "Helmet", "Body", "Shoulder", "Forearm", "Hips", "Legs", "Back",
    };

    public static HeroEquipmentProfile BuildWornProfile(Inventory inventory)
    {
        var profile = ScriptableObject.CreateInstance<HeroEquipmentProfile>();
        if (!inventory)
            return profile;

        foreach (var gridName in EquipmentGridNames)
        {
            if (!EquipmentSlotUtility.TryGetSlotFromGridName(gridName, out var slot))
                continue;

            var item = GetPrimaryItemInGrid(inventory, gridName);
            if (!IsSyntyEquipment(item, out var equipmentItem))
                continue;

            ApplyItemToProfile(profile, equipmentItem);
        }

        return profile;
    }

    public static void ApplyInventoryToHero(Inventory inventory, PlayerHeroEntity hero)
    {
        if (!hero || !hero.visual)
            return;

        var profile = BuildWornProfile(inventory);
        hero.activeEquipment = profile;
        hero.visual.ApplyAppearance(hero.appearance);
        hero.visual.ApplyEquipment(profile, hero.appearance);
    }

    public static bool TryDropToWorld(SyntyEquipmentItemData itemData, Vector3 worldPosition, Quaternion rotation)
    {
        if (!itemData || !itemData.worldPickupPrefab)
            return false;

        var pickup = Object.Instantiate(itemData.worldPickupPrefab, worldPosition, rotation);
        var pickupComponent = pickup.GetComponent<EquipmentWorldPickup>();
        if (pickupComponent)
            pickupComponent.BindItemData(itemData);

        return true;
    }

    public static bool TryPickup(EquipmentWorldPickup pickup, Inventory inventory, CharacterEntry characterEntry = null)
    {
        if (!pickup || !inventory)
            return false;

        var itemData = pickup.ItemData;
        if (!itemData)
            return false;

        if (!inventory.AddItem(itemData, 1, false, false))
            return false;

        PersistInventoryToCharacter(inventory, characterEntry);
        Object.Destroy(pickup.gameObject);
        return true;
    }

    public static void PersistInventoryToCharacter(Inventory inventory, CharacterEntry characterEntry)
    {
        if (!inventory || characterEntry == null)
            return;

        var exported = RoleInventoryPersistence.Export(inventory);
        exported.maxCarryWeight = characterEntry.inventory.maxCarryWeight;
        characterEntry.inventory = exported;
    }

    static Item GetPrimaryItemInGrid(Inventory inventory, string gridName)
    {
        var grid = inventory.FindGridByName(gridName);
        if (!grid || grid.items == null)
            return null;

        var width = grid.items.GetLength(0);
        var height = grid.items.GetLength(1);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var item = grid.items[x, y];
                if (item)
                    return item;
            }
        }

        return null;
    }

    static bool IsSyntyEquipment(Item item, out SyntyEquipmentItemData equipmentItem)
    {
        equipmentItem = null;
        if (!item || !item.data)
            return false;

        equipmentItem = item.data as SyntyEquipmentItemData;
        return equipmentItem != null;
    }

    static void ApplyItemToProfile(HeroEquipmentProfile profile, SyntyEquipmentItemData item)
    {
        if (!profile || !item)
            return;

        profile.setIndex = item.setId;
        profile.setName = item.itemName;
        profile.hideHairWhenHeadEquipped = item.equipmentSlot == SyntyEquipmentSlot.Head;

        switch (item.equipmentSlot)
        {
            case SyntyEquipmentSlot.Head: profile.head = item.parts; break;
            case SyntyEquipmentSlot.Body: profile.body = item.parts; break;
            case SyntyEquipmentSlot.Shoulder: profile.shoulder = item.parts; break;
            case SyntyEquipmentSlot.Forearm: profile.forearm = item.parts; break;
            case SyntyEquipmentSlot.Hips: profile.hips = item.parts; break;
            case SyntyEquipmentSlot.Leg: profile.leg = item.parts; break;
            case SyntyEquipmentSlot.Back: profile.back = item.parts; break;
        }
    }
}
