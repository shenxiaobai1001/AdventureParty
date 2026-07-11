using UInventoryGrid;
using UnityEngine;

[CreateAssetMenu(fileName = "SyntyWeaponItem", menuName = "AdventureParty/Synty Weapon Item")]
public class SyntyWeaponItemData : ItemData
{
    [Header("Synty Weapon")]
    public int weaponItemId;
    public WeaponPack pack;
    public WeaponCategory category;
    public string syntyPrefabPath;
    public GameObject syntySourcePrefab;
    public GameObject worldPickupPrefab;
    public bool renderVertical;

    public void ApplyFromRow(WeaponItemRow row)
    {
        if (row == null)
            return;

        weaponItemId = row.id;
        pack = row.GetPack();
        category = row.GetCategory();
        syntyPrefabPath = row.GetSyntyPrefabAssetPath();
        itemName = row.name;
        description = row.name;
        weight = row.weight > 0f ? row.weight : 1f;
        itemType = ItemType.Weapon;
        var grid = row.GetGridSize();
        size = new SizeInt(grid.x, grid.y);
        renderVertical = row.UsesVerticalIconRender();
        stackable = false;
        maxStack = 1;
    }
}
