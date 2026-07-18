using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

/// <summary>
/// Inventory weapon entry used to rebuild back-mounted weapon visuals.
/// </summary>
public readonly struct WeaponGridEntry
{
    public WeaponGridEntry(SyntyWeaponItemData weaponData, int gridOrder)
    {
        WeaponData = weaponData;
        GridOrder = gridOrder;
    }

    public SyntyWeaponItemData WeaponData { get; }
    public int GridOrder { get; }
}

/// <summary>
/// Bridges weapon world pickups and weapon-grid equipment with HeroWeaponVisual.
/// </summary>
public static class WeaponInventoryBridge

{

    /// <summary>Max weapons parented to the hero at once (hands + back).</summary>

    public const int MaxEquippedWeapons = 5;



    /// <summary>Max weapons shown on the back mount when all are stowed.</summary>

    public const int MaxEquippedBackWeapons = 3;



    public static void ApplyInventoryToHero(Inventory inventory, PlayerHeroEntity hero)

    {

        if (!inventory || !hero || !hero.weaponVisual)

            return;



        hero.weaponVisual.SyncFromWeaponGrid(CollectWeaponGridEntries(inventory));

    }



    public static List<WeaponGridEntry> CollectWeaponGridEntries(Inventory inventory)

    {

        var entries = new List<WeaponGridEntry>();

        if (!inventory)

            return entries;



        var grid = inventory.FindGridByName(RoleInventoryGridLayout.WeaponGrid);

        if (!grid || grid.items == null)

            return entries;



        var seen = new HashSet<ItemData>();

        var width = grid.items.GetLength(0);

        var height = grid.items.GetLength(1);



        for (var y = 0; y < height; y++)

        {

            for (var x = 0; x < width; x++)

            {

                var item = grid.items[x, y];

                if (!item || !item.data || seen.Contains(item.data))

                    continue;



                if (item.data is not SyntyWeaponItemData weaponData)

                    continue;



                seen.Add(item.data);

                entries.Add(new WeaponGridEntry(weaponData, y * 1000 + x));

            }

        }



        return entries;

    }



    public static bool TryPickup(WeaponWorldPickup pickup, Inventory inventory, CharacterEntry characterEntry = null)

    {

        if (!pickup || !inventory)

            return false;



        var itemData = pickup.ItemData as SyntyWeaponItemData;

        if (!itemData)

            return false;



        if (!inventory.AddItem(itemData, 1, false, false))

            return false;



        PersistInventoryToCharacter(inventory, characterEntry);

        Object.Destroy(pickup.gameObject);

        return true;

    }



    public static bool TryDropToWorld(SyntyWeaponItemData itemData, Vector3 worldPosition, Quaternion rotation)

    {

        if (!itemData || !itemData.worldPickupPrefab)

            return false;



        var pickup = Object.Instantiate(itemData.worldPickupPrefab, worldPosition, rotation);

        var pickupComponent = pickup.GetComponent<WeaponWorldPickup>();

        if (pickupComponent)

            pickupComponent.BindItemData(itemData);



        return true;

    }



    public static void PersistInventoryToCharacter(Inventory inventory, CharacterEntry characterEntry)

    {

        EquipmentInventoryBridge.PersistInventoryToCharacter(inventory, characterEntry);

    }

}

