using UInventoryGrid;
using UnityEngine;

/// <summary>
/// Keeps HeroWeaponVisual in sync with the Weapon inventory grid.
/// </summary>
[RequireComponent(typeof(Inventory))]
public class WeaponInventoryWatcher : MonoBehaviour
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
        var affectsWeapons =
            (previousGrid != null && RoleInventoryTypes.IsWeaponGrid(previousGrid.name)) ||
            (currentGrid != null && RoleInventoryTypes.IsWeaponGrid(currentGrid.name));

        if (!affectsWeapons)
            return;

        RefreshBoundHero();
    }

    public void RefreshBoundHero()
    {
        var hero = ResolveBoundHero();
        if (!hero || !inventory)
            return;

        WeaponInventoryBridge.ApplyInventoryToHero(inventory, hero);
    }

    PlayerHeroEntity ResolveBoundHero()
    {
        var panel = GetComponent<UIRolePanelController>();
        return panel ? panel.ResolveBoundHero() : null;
    }
}
