using UInventoryGrid;
using UnityEngine;

public static class WeaponInventoryBridge
{
    public static bool TryPickup(WeaponWorldPickup pickup, Inventory inventory, CharacterEntry characterEntry = null)
    {
        if (!pickup || !inventory)
            return false;

        var itemData = pickup.ItemData as SyntyWeaponItemData;
        if (!itemData)
            return false;

        if (!inventory.AddItem(itemData, 1, false, false))
            return false;

        EquipmentInventoryBridge.PersistInventoryToCharacter(inventory, characterEntry);
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
}
