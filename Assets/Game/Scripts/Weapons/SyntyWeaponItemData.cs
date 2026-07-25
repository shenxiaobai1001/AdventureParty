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

    [Header("Melee Sweep (optional override)")]
    [Tooltip("When true, use the local root/tip/radius below instead of mesh auto-fit.")]
    public bool overrideMeleeSweep;
    [Tooltip("Blade near end in weapon-instance local space.")]
    public Vector3 meleeSweepLocalRoot = Vector3.zero;
    [Tooltip("Blade tip in weapon-instance local space.")]
    public Vector3 meleeSweepLocalTip = new Vector3(0f, 0f, 0.75f);
    [Tooltip("Capsule radius around the blade.")]
    public float meleeSweepRadius = 0.08f;

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
