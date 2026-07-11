using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class IconLayoutOverrideStore
{
    const string RelativePath = "Assets/Game/Data/IconStudio/LayoutOverrides.json";

    static IconLayoutOverrideFile cachedFile;
    static Dictionary<string, IconLayoutOverrideSet> lookup;

    public static string FilePath => Path.Combine(Application.dataPath, "Game/Data/IconStudio/LayoutOverrides.json");

    public static string GetKey(IconRenderEntry entry)
    {
        return $"set_{entry.setId:D3}_{entry.slot}";
    }

    public static void EnsureLoaded()
    {
        if (cachedFile != null)
            return;

        cachedFile = new IconLayoutOverrideFile();
        lookup = new Dictionary<string, IconLayoutOverrideSet>(StringComparer.Ordinal);

        if (!File.Exists(FilePath))
            return;

        try
        {
            var json = File.ReadAllText(FilePath);
            if (string.IsNullOrWhiteSpace(json))
                return;

            var loaded = JsonUtility.FromJson<IconLayoutOverrideFile>(json);
            if (loaded?.sets == null)
                return;

            cachedFile = loaded;
            RebuildLookup();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[IconStudio] Failed to load layout overrides: {ex.Message}");
        }
    }

    static void RebuildLookup()
    {
        lookup ??= new Dictionary<string, IconLayoutOverrideSet>(StringComparer.Ordinal);
        lookup.Clear();

        if (cachedFile?.sets == null)
            return;

        foreach (var set in cachedFile.sets)
        {
            if (set == null || string.IsNullOrWhiteSpace(set.key))
                continue;

            lookup[set.key] = set;
        }
    }

    public static bool HasOverride(IconRenderEntry entry)
    {
        EnsureLoaded();
        return lookup.ContainsKey(GetKey(entry));
    }

    public static bool TryGetOverride(IconRenderEntry entry, out IconLayoutOverrideSet overrideSet)
    {
        EnsureLoaded();
        return lookup.TryGetValue(GetKey(entry), out overrideSet);
    }

    public static void ApplyOverride(IconRenderEntry entry, Transform previewStage, IList<GameObject> parts)
    {
        if (!TryGetOverride(entry, out var overrideSet))
            return;

        if (previewStage)
            previewStage.localRotation = Quaternion.Euler(overrideSet.stageLocalEuler);

        if (overrideSet.parts == null)
            return;

        var partLookup = new Dictionary<string, IconLayoutPartOverride>(StringComparer.Ordinal);
        foreach (var partOverride in overrideSet.parts)
        {
            if (partOverride == null || string.IsNullOrWhiteSpace(partOverride.partName))
                continue;

            partLookup[partOverride.partName] = partOverride;
        }

        foreach (var part in parts)
        {
            if (!part || !partLookup.TryGetValue(part.name, out var partOverride))
                continue;

            part.transform.localPosition = partOverride.localPosition;
            part.transform.localEulerAngles = partOverride.localEulerAngles;
        }
    }

    public static bool RecordCurrent(IconRenderEntry entry, Transform previewStage, IList<GameObject> parts)
    {
        if (entry == null || !previewStage || parts == null || parts.Count == 0)
            return false;

        EnsureLoaded();

        var partOverrides = new List<IconLayoutPartOverride>(parts.Count);
        foreach (var part in parts)
        {
            if (!part)
                continue;

            partOverrides.Add(new IconLayoutPartOverride
            {
                partName = part.name,
                localPosition = part.transform.localPosition,
                localEulerAngles = part.transform.localEulerAngles
            });
        }

        var set = new IconLayoutOverrideSet
        {
            key = GetKey(entry),
            stageLocalEuler = previewStage.localEulerAngles,
            parts = partOverrides.ToArray()
        };

        lookup[set.key] = set;

        var sets = new List<IconLayoutOverrideSet>(lookup.Values);
        sets.Sort((a, b) => string.CompareOrdinal(a.key, b.key));
        cachedFile.sets = sets.ToArray();

        return SaveToDisk();
    }

    public static bool ClearOverride(IconRenderEntry entry)
    {
        if (entry == null)
            return false;

        EnsureLoaded();

        if (!lookup.Remove(GetKey(entry)))
            return false;

        var sets = new List<IconLayoutOverrideSet>(lookup.Values);
        sets.Sort((a, b) => string.CompareOrdinal(a.key, b.key));
        cachedFile.sets = sets.ToArray();

        return SaveToDisk();
    }

    static bool SaveToDisk()
    {
#if UNITY_EDITOR
        try
        {
            var directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var json = JsonUtility.ToJson(cachedFile, true);
            File.WriteAllText(FilePath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[IconStudio] Saved layout override: {RelativePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[IconStudio] Failed to save layout overrides: {ex.Message}");
            return false;
        }
#else
        Debug.LogWarning("[IconStudio] Layout overrides can only be saved in the Unity Editor.");
        return false;
#endif
    }
}
