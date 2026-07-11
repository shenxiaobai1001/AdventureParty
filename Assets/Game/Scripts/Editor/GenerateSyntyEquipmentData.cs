#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class GenerateSyntyEquipmentData
{
    const string StaticPartsPath =
        "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Characters_ModularParts_Static";

    const string PresetsPath =
        "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Characters_Presets";

    const string EquipmentFolder = "Assets/Game/Data/Equipment";
    const string CatalogAssetPath = EquipmentFolder + "/SyntyPartCatalog.asset";
    const string DatabaseAssetPath = EquipmentFolder + "/EquipmentSetDatabase.asset";
    const string CatalogTextPath = EquipmentFolder + "/SyntyPartCatalog.txt";
    const string SetsTsvPath = EquipmentFolder + "/EquipmentSets.tsv";
    const string SetsCsvPath = "Assets/Game/Resources_moved/Config/EquipmentSets.csv";

    [MenuItem("Game/Character/Generate Synty Equipment Catalog + Sets")]
    public static void GenerateAll()
    {
        EnsureFolder(EquipmentFolder);

        var catalog = LoadOrCreateCatalog();
        var grouped = ScanStaticParts();
        foreach (var pair in grouped)
            catalog.SetParts(pair.Key, pair.Value.ToArray());

        EditorUtility.SetDirty(catalog);

        var sets = ScanPresetSets();
        var database = LoadOrCreateDatabase();
        database.sets = sets;
        EditorUtility.SetDirty(database);

        WriteCatalogText(grouped);
        WriteSetsTsv(sets);
        WriteSetsCsv(sets);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Synty Equipment Data",
            $"Generated:\n" +
            $"- {CatalogAssetPath}\n" +
            $"- {DatabaseAssetPath}\n" +
            $"- {CatalogTextPath}\n" +
            $"- {SetsTsvPath}\n" +
            $"- {SetsCsvPath}\n\n" +
            $"Catalog parts: {CountParts(grouped)}\n" +
            $"Equipment sets: {sets.Count}",
            "OK");
    }

    static SyntyEquipmentPartCatalog LoadOrCreateCatalog()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<SyntyEquipmentPartCatalog>(CatalogAssetPath);
        if (catalog)
            return catalog;

        catalog = ScriptableObject.CreateInstance<SyntyEquipmentPartCatalog>();
        AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
        return catalog;
    }

    static EquipmentSetDatabase LoadOrCreateDatabase()
    {
        var database = AssetDatabase.LoadAssetAtPath<EquipmentSetDatabase>(DatabaseAssetPath);
        if (database)
            return database;

        database = ScriptableObject.CreateInstance<EquipmentSetDatabase>();
        AssetDatabase.CreateAsset(database, DatabaseAssetPath);
        return database;
    }

    static Dictionary<SyntyEquipmentSlot, List<string>> ScanStaticParts()
    {
        var grouped = CreateEmptyGroups();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { StaticPartsPath });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var partName = Path.GetFileNameWithoutExtension(path).Replace("_Static", string.Empty);
            var slot = SyntyEquipmentPartClassifier.Classify(partName);
            if (!slot.HasValue)
                continue;

            grouped[slot.Value].Add(partName);
        }

        foreach (var slot in grouped.Keys)
            grouped[slot].Sort();

        return grouped;
    }

    static List<EquipmentSetEntry> ScanPresetSets()
    {
        var result = new List<EquipmentSetEntry>();
        var guids = AssetDatabase.FindAssets("Chr_FantasyHero_Preset_", new[] { PresetsPath });

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (!TryParsePresetIndex(fileName, out var setIndex))
                continue;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab)
                continue;

            var partNames = CollectActiveEquipmentParts(prefab);
            var groups = EquipmentPartParser.GroupBySlot(partNames);
            var entry = new EquipmentSetEntry
            {
                setIndex = setIndex,
                setName = "Preset_" + setIndex,
            };

            entry.head = JoinSlot(groups, SyntyEquipmentSlot.Head);
            entry.body = JoinSlot(groups, SyntyEquipmentSlot.Body);
            entry.shoulder = string.Empty;
            entry.forearm = JoinSlot(groups, SyntyEquipmentSlot.Forearm);
            entry.hips = JoinSlot(groups, SyntyEquipmentSlot.Hips);
            entry.leg = JoinSlot(groups, SyntyEquipmentSlot.Leg);
            entry.back = JoinSlot(groups, SyntyEquipmentSlot.Back);

            result.Add(entry);
        }

        result.Sort((a, b) => a.setIndex.CompareTo(b.setIndex));
        return result;
    }

    static List<string> CollectActiveEquipmentParts(GameObject prefabRoot)
    {
        var parts = new List<string>();
        var renderers = prefabRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);

        foreach (var renderer in renderers)
        {
            if (!renderer.gameObject.activeSelf)
                continue;

            var partName = renderer.gameObject.name;
            if (!partName.StartsWith("Chr_"))
                continue;

            if (!SyntyEquipmentPartClassifier.Classify(partName).HasValue)
                continue;

            if (!parts.Contains(partName))
                parts.Add(partName);
        }

        return parts;
    }

    static string JoinSlot(Dictionary<SyntyEquipmentSlot, List<string>> groups, SyntyEquipmentSlot slot)
    {
        if (!groups.TryGetValue(slot, out var parts) || parts.Count == 0)
            return string.Empty;

        SyntyEquipmentPartClassifier.SortPartsForSlot(slot, parts);
        return EquipmentPartParser.Join(parts);
    }

    static Dictionary<SyntyEquipmentSlot, List<string>> CreateEmptyGroups()
    {
        return new Dictionary<SyntyEquipmentSlot, List<string>>
        {
            { SyntyEquipmentSlot.Head, new List<string>() },
            { SyntyEquipmentSlot.Body, new List<string>() },
            { SyntyEquipmentSlot.Forearm, new List<string>() },
            { SyntyEquipmentSlot.Hips, new List<string>() },
            { SyntyEquipmentSlot.Leg, new List<string>() },
            { SyntyEquipmentSlot.Back, new List<string>() },
        };
    }

    static void WriteCatalogText(Dictionary<SyntyEquipmentSlot, List<string>> grouped)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Synty PolygonFantasyHeroCharacters - Equipment Part Catalog");
        sb.AppendLine("# Source: Characters_ModularParts_Static");
        sb.AppendLine("# Format: 6 equipment slots (body includes shoulders; weapons excluded). Parts in one set use ; separator.");
        sb.AppendLine();

        AppendSlotSection(sb, "1-Head (HelmetAttachment;HeadCovering)", grouped[SyntyEquipmentSlot.Head]);
        AppendSlotSection(sb, "2-Body (ShoulderAttach;Torso;ArmUpperRight;ArmUpperLeft)", grouped[SyntyEquipmentSlot.Body]);
        AppendSlotSection(sb, "3-Forearm (ArmLowerRight;ArmLowerLeft;HandRight;HandLeft; optional Hips)", grouped[SyntyEquipmentSlot.Forearm]);
        AppendSlotSection(sb, "4-Hips (Hips;HipsAttachment)", grouped[SyntyEquipmentSlot.Hips]);
        AppendSlotSection(sb, "5-Leg (LegRight;LegLeft; optional KneeAttach)", grouped[SyntyEquipmentSlot.Leg]);
        AppendSlotSection(sb, "6-Back (BackAttachment)", grouped[SyntyEquipmentSlot.Back]);

        File.WriteAllText(CatalogTextPath, sb.ToString(), new UTF8Encoding(false));
    }

    static void AppendSlotSection(StringBuilder sb, string title, List<string> parts)
    {
        sb.Append('[').Append(title).AppendLine("]");
        sb.Append("count=").Append(parts.Count).AppendLine();
        foreach (var part in parts)
            sb.AppendLine(part);
        sb.AppendLine();
    }

    static void WriteSetsTsv(List<EquipmentSetEntry> sets)
    {
        var sb = new StringBuilder();
        sb.AppendLine("setIndex\tsetName\tHead\tBody\tForearm\tHips\tLeg\tBack");

        foreach (var set in sets)
        {
            sb.Append(set.setIndex).Append('\t')
                .Append(set.setName).Append('\t')
                .Append(set.head).Append('\t')
                .Append(set.body).Append('\t')
                .Append(set.forearm).Append('\t')
                .Append(set.hips).Append('\t')
                .Append(set.leg).Append('\t')
                .Append(set.back).AppendLine();
        }

        File.WriteAllText(SetsTsvPath, sb.ToString(), new UTF8Encoding(false));
    }

    static void WriteSetsCsv(List<EquipmentSetEntry> sets)
    {
        EnsureFolder("Assets/Game/Resources_moved/Config");

        var sb = new StringBuilder();
        sb.AppendLine("id,name,head,body,forearm,hips,leg,back");

        foreach (var set in sets)
        {
            sb.Append(set.setIndex).Append(',')
                .Append(set.setName).Append(',')
                .Append(set.head).Append(',')
                .Append(set.body).Append(',')
                .Append(set.forearm).Append(',')
                .Append(set.hips).Append(',')
                .Append(set.leg).Append(',')
                .Append(set.back).AppendLine();
        }

        File.WriteAllText(SetsCsvPath, sb.ToString(), new UTF8Encoding(false));
    }

    static int CountParts(Dictionary<SyntyEquipmentSlot, List<string>> grouped)
    {
        var total = 0;
        foreach (var pair in grouped)
            total += pair.Value.Count;
        return total;
    }

    static bool TryParsePresetIndex(string fileName, out int setIndex)
    {
        setIndex = 0;
        const string prefix = "Chr_FantasyHero_Preset_";
        if (!fileName.StartsWith(prefix))
            return false;

        return int.TryParse(fileName.Substring(prefix.Length), out setIndex);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parts = path.Split('/');
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
