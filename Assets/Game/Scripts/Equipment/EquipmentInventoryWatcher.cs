using UInventoryGrid;
using UnityEngine;

/// <summary>
/// Keeps the bound hero's 3D equipment in sync with UIRolePanel inventory grids.
/// </summary>
[RequireComponent(typeof(Inventory))]
public class EquipmentInventoryWatcher : MonoBehaviour
{
    Inventory inventory;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
    }

    void OnEnable()
    {
        if (inventory)
            inventory.ItemGridChanged += HandleItemGridChanged;
    }

    void OnDisable()
    {
        if (inventory)
            inventory.ItemGridChanged -= HandleItemGridChanged;
    }

    void HandleItemGridChanged(Item item, InventoryGrid previousGrid, InventoryGrid currentGrid)
    {
        var hero = ResolveBoundHero();
        if (!hero)
            return;

        var affectsEquipment =
            (previousGrid != null && RoleInventoryTypes.IsEquipmentGrid(previousGrid.name)) ||
            (currentGrid != null && RoleInventoryTypes.IsEquipmentGrid(currentGrid.name));

        if (!affectsEquipment)
            return;

        EquipmentInventoryBridge.ApplyInventoryToHero(inventory, hero);
    }

    public void RefreshBoundHero()
    {
        var hero = ResolveBoundHero();
        if (!hero || !inventory)
            return;

        EquipmentInventoryBridge.ApplyInventoryToHero(inventory, hero);
    }

    PlayerHeroEntity ResolveBoundHero()
    {
        var panel = GetComponent<UIRolePanelController>();
        if (panel)
            return panel.ResolveBoundHero();

        return null;
    }
}
