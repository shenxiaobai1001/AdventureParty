using UInventoryGrid;
using UnityEngine;

[CreateAssetMenu(fileName = "SyntyWeaponItem", menuName = "AdventureParty/Synty Weapon Item")]
public class SyntyWeaponItemData : ItemData
{
    [Header("Synty Weapon")]
    public int weaponItemId;
    public WeaponPack pack;
    public WeaponCategory category;
    [Header("Combat Proficiency")]
    public WeaponProficiencyType proficiencyType;
    public bool proficiencyOverride;
    public string syntyPrefabPath;
    public GameObject syntySourcePrefab;
    public GameObject worldPickupPrefab;
    public bool renderVertical;
    public string iconFileName;

    public Sprite ResolveIcon()
    {
        if (icon)
            return icon;

        var path = !string.IsNullOrWhiteSpace(iconFileName)
            ? WeaponIconResolver.BuildAssetPath(iconFileName)
            : string.Empty;

        if (string.IsNullOrEmpty(path) && weaponItemId > 0)
        {
            WeaponItemData.Instance.EnsureLoaded();
            if (WeaponItemData.Instance.TryGetItem(weaponItemId, out var row))
                path = row.GetIconAssetPath();
        }

        var resolved = WeaponIconResolver.LoadSprite(path);
        if (resolved)
            icon = resolved;

        return resolved;
    }

    public void ApplyFromRow(WeaponItemRow row)
    {
        if (row == null)
            return;

        weaponItemId = row.id;
        pack = row.GetPack();
        category = row.GetCategory();
        proficiencyType = row.GetProficiencyType();
        proficiencyOverride = row.HasProficiencyOverride();
        syntyPrefabPath = row.GetSyntyPrefabAssetPath();
        itemName = row.name;
        description = row.name;
        weight = row.weight > 0f ? row.weight : 1f;
        itemType = ItemType.Weapon;
        var grid = row.GetGridSize();
        size = new SizeInt(grid.x, grid.y);
        renderVertical = row.UsesVerticalIconRender();
        iconFileName = row.icon;
        stackable = false;
        maxStack = 1;
    }
}
