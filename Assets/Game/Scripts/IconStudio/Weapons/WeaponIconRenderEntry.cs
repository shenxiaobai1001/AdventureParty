using System;
using UnityEngine;

[Serializable]
public class WeaponIconRenderEntry
{
    public int weaponId;
    public string name;
    public WeaponPack pack;
    public WeaponCategory category;
    public string syntyPrefabPath;
    public Vector2Int gridSize;
    public bool renderVertical;

    public string DisplayLabel => $"{weaponId:0000} {name}";

    public string FileName => iconFileName;

    public string iconFileName;

    public static WeaponIconRenderEntry FromRow(WeaponItemRow row)
    {
        if (row == null)
            return null;

        return new WeaponIconRenderEntry
        {
            weaponId = row.id,
            name = row.name,
            pack = row.GetPack(),
            category = row.GetCategory(),
            syntyPrefabPath = row.GetSyntyPrefabAssetPath(),
            gridSize = row.GetGridSize(),
            renderVertical = row.UsesVerticalIconRender(),
            iconFileName = row.icon,
        };
    }
}
