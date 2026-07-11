#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GenerateEquipmentAssets
{
    const string ItemsCsvPath = "Assets/Game/Resources_moved/Config/EquipmentItems.csv";
    const string ItemsFolder = "Assets/Game/Data/Equipment/Items";
    const string WorldFolder = "Assets/Game/Prefabs/Equipment/World";
    const string WorldMeshFolder = "Assets/Game/Art/Meshes/Equipment/World";

    static readonly SyntyEquipmentSlot[] SlotOrder =
    {
        SyntyEquipmentSlot.Head,
        SyntyEquipmentSlot.Body,
        SyntyEquipmentSlot.Forearm,
        SyntyEquipmentSlot.Hips,
        SyntyEquipmentSlot.Leg,
        SyntyEquipmentSlot.Back,
    };

    [MenuItem("Game/Equipment/1. Generate EquipmentItems.csv From Sets")]
    public static void GenerateItemsCsvFromSets()
    {
        if (!EquipmentData.Instance.EnsureLoaded())
        {
            EditorUtility.DisplayDialog("Equipment Items", "Failed to load EquipmentSets.csv", "OK");
            return;
        }

        var rows = BuildItemRows();
        WriteItemsCsv(rows);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Equipment Items",
            $"Generated {rows.Count} rows:\n{ItemsCsvPath}",
            "OK");
    }

    [MenuItem("Game/Equipment/2. Generate ItemData + World Prefabs")]
    public static void GenerateItemAssetsAndWorldPrefabs()
    {
        if (!File.Exists(ItemsCsvPath))
            GenerateItemsCsvFromSets();

        var rows = LoadItemRowsFromCsv();
        if (rows.Count == 0)
        {
            EditorUtility.DisplayDialog("Equipment Items", "No rows in EquipmentItems.csv", "OK");
            return;
        }

        EnsureFolder(ItemsFolder);
        EnsureFolder(WorldFolder);

        var createdItems = 0;
        var createdWorld = 0;
        var skippedWorld = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            EditorUtility.DisplayProgressBar(
                "Generate Equipment Assets",
                row.name,
                i / (float)rows.Count);

            var itemAsset = CreateOrUpdateItemAsset(row);
            if (itemAsset)
                createdItems++;

            if (CreateOrUpdateWorldPrefab(row, itemAsset, out _))
                createdWorld++;
            else
                skippedWorld++;
        }

        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Equipment Assets",
            $"ItemData assets: {createdItems}\nWorld prefabs: {createdWorld}\nSkipped world: {skippedWorld}",
            "OK");
    }

    [MenuItem("Game/Equipment/Generate All (CSV + Assets)")]
    public static void GenerateAll()
    {
        GenerateItemsCsvFromSets();
        GenerateItemAssetsAndWorldPrefabs();
    }

    static List<EquipmentItemRow> BuildItemRows()
    {
        var rows = new List<EquipmentItemRow>();
        foreach (var setRow in EquipmentData.Instance.sets.GetListInfo())
        {
            if (setRow == null)
                continue;

            foreach (var slot in SlotOrder)
            {
                var parts = setRow.GetSlotParts(slot);
                if (parts == null || parts.Length == 0)
                    continue;

                var hasAny = false;
                foreach (var part in parts)
                {
                    if (!string.IsNullOrWhiteSpace(part))
                    {
                        hasAny = true;
                        break;
                    }
                }

                if (!hasAny)
                    continue;

                var grid = IconStudioSettings.GetGridSize(slot);
                var stem = EquipmentSlotUtility.GetItemFileStem(setRow.id, slot);
                rows.Add(new EquipmentItemRow
                {
                    id = EquipmentSlotUtility.ComposeItemId(setRow.id, slot),
                    setId = setRow.id,
                    name = $"{setRow.name} {EquipmentSlotUtility.GetSlotLabel(slot)}",
                    slot = EquipmentSlotUtility.GetSlotLabel(slot),
                    parts = EquipmentPartParser.Join(parts),
                    icon = stem + ".png",
                    worldPrefab = $"{WorldFolder}/{stem}_World.prefab",
                    gridW = grid.x,
                    gridH = grid.y,
                    itemType = EquipmentSlotUtility.ToItemType(slot).ToString(),
                    weight = GetDefaultWeight(slot),
                });
            }
        }

        return rows;
    }

    static float GetDefaultWeight(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Body: return 7f;
            case SyntyEquipmentSlot.Leg: return 4f;
            case SyntyEquipmentSlot.Hips: return 3f;
            case SyntyEquipmentSlot.Forearm: return 2f;
            case SyntyEquipmentSlot.Back: return 2f;
            case SyntyEquipmentSlot.Head: return 1.5f;
            default: return 1f;
        }
    }

    static void WriteItemsCsv(List<EquipmentItemRow> rows)
    {
        EnsureFolder(Path.GetDirectoryName(ItemsCsvPath)?.Replace('\\', '/'));
        var builder = new StringBuilder();
        builder.AppendLine("id,setId,slot,name,parts,icon,worldPrefab,gridW,gridH,itemType,weight");

        foreach (var row in rows)
        {
            builder.Append(row.id).Append(',')
                .Append(row.setId).Append(',')
                .Append(EscapeCsv(row.slot)).Append(',')
                .Append(EscapeCsv(row.name)).Append(',')
                .Append(EscapeCsv(row.parts)).Append(',')
                .Append(EscapeCsv(row.icon)).Append(',')
                .Append(EscapeCsv(row.worldPrefab)).Append(',')
                .Append(row.gridW).Append(',')
                .Append(row.gridH).Append(',')
                .Append(EscapeCsv(row.itemType)).Append(',')
                .Append(row.weight.ToString("0.##"))
                .AppendLine();
        }

        File.WriteAllText(ItemsCsvPath, builder.ToString(), Encoding.UTF8);
    }

    static List<EquipmentItemRow> LoadItemRowsFromCsv()
    {
        var table = new ConfigTable<EquipmentItemRow>();
        table.LoadText(File.ReadAllText(ItemsCsvPath));
        return table.GetListInfo();
    }

    static SyntyEquipmentItemData CreateOrUpdateItemAsset(EquipmentItemRow row)
    {
        var assetPath = $"{ItemsFolder}/{EquipmentSlotUtility.GetItemFileStem(row.setId, row.GetEquipmentSlot())}.asset";
        var item = AssetDatabase.LoadAssetAtPath<SyntyEquipmentItemData>(assetPath);
        if (!item)
        {
            item = ScriptableObject.CreateInstance<SyntyEquipmentItemData>();
            AssetDatabase.CreateAsset(item, assetPath);
        }

        item.ApplyFromRow(row);

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

    static bool CreateOrUpdateWorldPrefab(EquipmentItemRow row, SyntyEquipmentItemData itemAsset, out GameObject prefab)
    {
        prefab = null;
        var prefabPath = row.GetWorldPrefabAssetPath();
        if (string.IsNullOrEmpty(prefabPath))
            return false;

        EnsureFolder(Path.GetDirectoryName(prefabPath)?.Replace('\\', '/'));

        var tempRoot = new GameObject("TempEquipmentWorldRoot");
        try
        {
            var request = new EquipmentDisplayAssembler.AssemblyRequest
            {
                setId = row.setId,
                setName = row.name,
                slot = row.GetEquipmentSlot(),
                parts = EquipmentPartParser.Split(row.parts),
            };

            var dressed = EquipmentDisplayAssembler.Assemble(request, tempRoot.transform);
            if (!dressed)
                return false;

            var visualRoot = EquipmentDisplayAssembler.BakeToStaticVisual(dressed, tempRoot.transform);
            if (!visualRoot)
                return false;

            var pickupRoot = new GameObject(Path.GetFileNameWithoutExtension(prefabPath));
            pickupRoot.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            visualRoot.transform.SetParent(pickupRoot.transform, false);

            var pickup = pickupRoot.AddComponent<EquipmentWorldPickup>();
            if (itemAsset)
                pickup.BindItemData(itemAsset);

            EquipmentVisualBounds.CenterVisualAtParentOrigin(visualRoot.transform);
            EquipmentVisualBounds.FitBoxColliderToVisual(pickupRoot, visualRoot);
            EquipmentVisualBounds.EnsurePickupTriggerCollider(pickupRoot, 2f);
            EquipmentVisualBounds.EnsurePickupRigidbody(pickupRoot);
            SaveBakedMeshes(visualRoot, row);
            prefab = SavePrefab(pickupRoot, prefabPath);

            if (itemAsset && prefab)
            {
                itemAsset.worldPickupPrefab = prefab;
                EditorUtility.SetDirty(itemAsset);
            }

            return prefab != null;
        }
        finally
        {
            Object.DestroyImmediate(tempRoot);
        }
    }

    static void SaveBakedMeshes(GameObject visualRoot, EquipmentItemRow row)
    {
        if (!visualRoot || row == null)
            return;

        var stem = EquipmentSlotUtility.GetItemFileStem(row.setId, row.GetEquipmentSlot());
        var folder = $"{WorldMeshFolder}/{stem}";
        EnsureFolder(WorldMeshFolder);
        EnsureFolder(folder);

        foreach (var meshFilter in visualRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!meshFilter || !meshFilter.sharedMesh)
                continue;

            var meshPath = $"{folder}/{meshFilter.gameObject.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if (existing)
                AssetDatabase.DeleteAsset(meshPath);

            var meshCopy = Object.Instantiate(meshFilter.sharedMesh);
            meshCopy.name = meshFilter.gameObject.name;
            AssetDatabase.CreateAsset(meshCopy, meshPath);
            meshFilter.sharedMesh = meshCopy;
        }
    }

    static void FitBoxCollider(GameObject root, GameObject visualRoot)
    {
        var bounds = CalculateRendererBounds(visualRoot);
        var collider = root.GetComponent<BoxCollider>();
        if (!collider)
            collider = root.AddComponent<BoxCollider>();

        collider.center = root.transform.InverseTransformPoint(bounds.center);
        collider.size = bounds.size;
    }

    static Bounds CalculateRendererBounds(GameObject root)
    {
        var bounds = new Bounds(root.transform.position, Vector3.zero);
        var hasBounds = false;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            bounds = new Bounds(root.transform.position, Vector3.one * 0.25f);

        return bounds;
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
