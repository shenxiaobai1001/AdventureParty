#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GenerateWeaponAssets
{
    const string HeroWeaponsRoot = "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Weapons";
    const string KingdomWeaponsRoot = "Assets/Synty/PolygonFantasyKingdom/Prefabs/Weapons";
    const string ItemsCsvPath = "Assets/Game/Resources_moved/Config/WeaponItems.csv";
    const string ItemsFolder = "Assets/Game/Data/Weapons/Items";
    const string WorldFolder = "Assets/Game/Prefabs/Weapons/World";
    const string IconsFolder = "Assets/Game/Art/Icons/Weapons";

    [MenuItem("Game/Weapon/1. Generate WeaponItems.csv From Synty")]
    public static void GenerateItemsCsvFromSynty()
    {
        var rows = ScanWeaponRows();
        WriteItemsCsv(rows);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon Items",
            $"Generated {rows.Count} rows:\n{ItemsCsvPath}",
            "OK");
    }

    [MenuItem("Game/Weapon/2. Generate ItemData + World Prefabs")]
    public static void GenerateItemAssetsAndWorldPrefabs()
    {
        if (!File.Exists(ItemsCsvPath))
            GenerateItemsCsvFromSynty();

        var rows = LoadItemRowsFromCsv();
        if (rows.Count == 0)
        {
            EditorUtility.DisplayDialog("Weapon Items", "No rows in WeaponItems.csv", "OK");
            return;
        }

        EnsureFolder(ItemsFolder);
        EnsureFolder(WorldFolder);

        var createdItems = 0;
        var createdWorld = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            EditorUtility.DisplayProgressBar("Generate Weapon Assets", row.name, i / (float)rows.Count);

            var itemAsset = CreateOrUpdateItemAsset(row);
            if (itemAsset)
                createdItems++;

            if (CreateOrUpdateWorldPrefab(row, itemAsset))
                createdWorld++;
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon Assets",
            $"ItemData assets: {createdItems}\nWorld prefabs: {createdWorld}",
            "OK");
    }

    [MenuItem("Game/Weapon/Generate All (CSV + Assets)")]
    public static void GenerateAll()
    {
        GenerateItemsCsvFromSynty();
        GenerateItemAssetsAndWorldPrefabs();
    }

    public static List<WeaponItemRow> ScanWeaponRows()
    {
        var rows = new List<WeaponItemRow>();
        ScanRoot(HeroWeaponsRoot, WeaponPack.Hero, rows);
        ScanRoot(KingdomWeaponsRoot, WeaponPack.Kingdom, rows);
        return rows;
    }

    static void ScanRoot(string root, WeaponPack pack, List<WeaponItemRow> rows)
    {
        if (!AssetDatabase.IsValidFolder(root))
            return;

        var localRows = new List<WeaponItemRow>();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { root });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
            if (!path.StartsWith(root))
                continue;

            var prefabName = Path.GetFileNameWithoutExtension(path);
            if (!WeaponClassifier.ShouldIncludePrefab(path, prefabName))
                continue;

            var category = WeaponClassifier.Classify(prefabName, pack);
            var grid = WeaponClassifier.GetGridSize(category);
            var stem = WeaponClassifier.GetAssetStem(pack, prefabName);

            localRows.Add(new WeaponItemRow
            {
                name = WeaponClassifier.GetDisplayName(prefabName, pack),
                pack = pack.ToString(),
                category = category.ToString(),
                syntyPrefab = path,
                icon = stem + ".png",
                worldPrefab = $"{WorldFolder}/{stem}_World.prefab",
                gridW = grid.x,
                gridH = grid.y,
                itemType = "Weapon",
                weight = WeaponClassifier.GetDefaultWeight(category),
                renderVertical = WeaponClassifier.UsesVerticalIconRender(category) ? 1 : 0,
            });
        }

        localRows.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        var startId = pack == WeaponPack.Hero ? 1001 : 2001;
        for (var i = 0; i < localRows.Count; i++)
        {
            localRows[i].id = startId + i;
            rows.Add(localRows[i]);
        }
    }

    static void WriteItemsCsv(List<WeaponItemRow> rows)
    {
        EnsureFolder(Path.GetDirectoryName(ItemsCsvPath)?.Replace('\\', '/'));
        var builder = new StringBuilder();
        builder.AppendLine("id,name,pack,category,syntyPrefab,icon,worldPrefab,gridW,gridH,itemType,weight,renderVertical");

        foreach (var row in rows)
        {
            builder.Append(row.id).Append(',')
                .Append(EscapeCsv(row.name)).Append(',')
                .Append(EscapeCsv(row.pack)).Append(',')
                .Append(EscapeCsv(row.category)).Append(',')
                .Append(EscapeCsv(row.syntyPrefab)).Append(',')
                .Append(EscapeCsv(row.icon)).Append(',')
                .Append(EscapeCsv(row.worldPrefab)).Append(',')
                .Append(row.gridW).Append(',')
                .Append(row.gridH).Append(',')
                .Append(EscapeCsv(row.itemType)).Append(',')
                .Append(row.weight.ToString("0.##")).Append(',')
                .Append(row.renderVertical)
                .AppendLine();
        }

        File.WriteAllText(ItemsCsvPath, builder.ToString(), Encoding.UTF8);
    }

    static List<WeaponItemRow> LoadItemRowsFromCsv()
    {
        var table = new ConfigTable<WeaponItemRow>();
        table.LoadText(File.ReadAllText(ItemsCsvPath));
        return table.GetListInfo();
    }

    static SyntyWeaponItemData CreateOrUpdateItemAsset(WeaponItemRow row)
    {
        var stem = Path.GetFileNameWithoutExtension(row.GetWorldPrefabAssetPath()).Replace("_World", string.Empty);
        var assetPath = $"{ItemsFolder}/{stem}.asset";
        var item = AssetDatabase.LoadAssetAtPath<SyntyWeaponItemData>(assetPath);
        if (!item)
        {
            item = ScriptableObject.CreateInstance<SyntyWeaponItemData>();
            AssetDatabase.CreateAsset(item, assetPath);
        }

        item.ApplyFromRow(row);
        item.syntySourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(row.GetSyntyPrefabAssetPath());

        var iconPath = row.GetIconAssetPath();
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        if (sprite)
            item.icon = sprite;

        var worldPath = row.GetWorldPrefabAssetPath();
        var worldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(worldPath);
        if (worldPrefab)
            item.worldPickupPrefab = worldPrefab;

        EditorUtility.SetDirty(item);
        return item;
    }

    static bool CreateOrUpdateWorldPrefab(WeaponItemRow row, SyntyWeaponItemData itemAsset)
    {
        var prefabPath = row.GetWorldPrefabAssetPath();
        if (string.IsNullOrEmpty(prefabPath))
            return false;

        var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(row.GetSyntyPrefabAssetPath());
        if (!sourcePrefab)
            return false;

        EnsureFolder(Path.GetDirectoryName(prefabPath)?.Replace('\\', '/'));

        var pickupRoot = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
        try
        {
            var visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(pickupRoot.transform, false);

            var weaponInstance = Object.Instantiate(sourcePrefab, visualRoot.transform);
            weaponInstance.name = sourcePrefab.name;
            WeaponWorldLayout.Apply(weaponInstance, row.GetCategory());

            var pickup = pickupRoot.AddComponent<WeaponWorldPickup>();
            if (itemAsset)
                pickup.BindItemData(itemAsset);

            EquipmentVisualBounds.CenterVisualAtParentOrigin(visualRoot.transform);
            EquipmentVisualBounds.FitBoxColliderToVisual(pickupRoot, visualRoot);
            EquipmentVisualBounds.EnsurePickupTriggerCollider(pickupRoot, 2f);
            EquipmentVisualBounds.EnsurePickupRigidbody(pickupRoot);

            var saved = SavePrefab(pickupRoot, prefabPath);
            if (itemAsset && saved)
            {
                itemAsset.worldPickupPrefab = saved;
                EditorUtility.SetDirty(itemAsset);
            }

            return saved != null;
        }
        finally
        {
            Object.DestroyImmediate(pickupRoot);
        }
    }

    static GameObject SavePrefab(GameObject root, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing)
            PrefabUtility.SaveAsPrefabAsset(root, path);
        else
            existing = PrefabUtility.SaveAsPrefabAsset(root, path);

        return existing;
    }

    static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            return "\"" + value.Replace("\"", "\"\"") + "\"";

        return value;
    }

    static void EnsureFolder(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return;

        assetPath = assetPath.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        var parts = assetPath.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);

            current = next;
        }
    }
}
#endif
