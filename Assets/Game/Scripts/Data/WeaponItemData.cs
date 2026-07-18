using UInventoryGrid;
using UnityEngine;

public class WeaponItemData : Singleton<WeaponItemData>
{
    public ConfigTable<WeaponItemRow> items = new ConfigTable<WeaponItemRow>();

    bool _loaded;

    public bool Init()
    {
        _loaded = items.Load("WeaponItems.csv");
        return _loaded;
    }

    public bool EnsureLoaded()
    {
        if (_loaded && items.GetListInfo().Count > 0)
            return true;

        return Init() && items.GetListInfo().Count > 0;
    }

    public WeaponItemRow GetItem(int itemId)
    {
        return items.GetInfo(itemId);
    }

    public bool TryGetItem(int itemId, out WeaponItemRow row)
    {
        return items.TryGetInfo(itemId, out row);
    }
}

public class WeaponItemRow : NamedData
{
    public string pack;
    public string category;
    public string proficiencyType;
    public int proficiencyOverride;
    public string syntyPrefab;
    public string icon;
    public string worldPrefab;
    public int gridW;
    public int gridH;
    public string itemType;
    public float weight;
    public int renderVertical;

    public WeaponPack GetPack()
    {
        return System.Enum.TryParse(pack, true, out WeaponPack parsed) ? parsed : WeaponPack.Hero;
    }

    public WeaponCategory GetCategory()
    {
        return System.Enum.TryParse(category, true, out WeaponCategory parsed)
            ? parsed
            : WeaponCategory.Misc1H;
    }

    public WeaponProficiencyType GetProficiencyType()
    {
        if (HasProficiencyOverride())
        {
            if (System.Enum.TryParse(proficiencyType, true, out WeaponProficiencyType overridden))
                return overridden;

            if (string.Equals(proficiencyType, "Dagger", System.StringComparison.OrdinalIgnoreCase))
                return WeaponProficiencyType.MartialArts;
        }

        return WeaponClassifier.GetProficiencyType(GetCategory());
    }

    public bool HasProficiencyOverride()
    {
        return proficiencyOverride != 0;
    }

    public Vector2Int GetGridSize()
    {
        return new Vector2Int(Mathf.Max(1, gridW), Mathf.Max(1, gridH));
    }

    public bool UsesVerticalIconRender()
    {
        return renderVertical != 0;
    }

    public string GetIconAssetPath()
    {
        if (string.IsNullOrWhiteSpace(icon))
            return string.Empty;

        var path = icon.Trim().Replace('\\', '/');
        if (!path.StartsWith("Assets/"))
            path = WeaponIconStudioSettings.OutputRoot + "/" + path;

        return path;
    }

    public string GetWorldPrefabAssetPath()
    {
        if (string.IsNullOrWhiteSpace(worldPrefab))
            return string.Empty;

        return worldPrefab.Trim().Replace('\\', '/');
    }

    public string GetSyntyPrefabAssetPath()
    {
        return syntyPrefab?.Trim().Replace('\\', '/');
    }
}
