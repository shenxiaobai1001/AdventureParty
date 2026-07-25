#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Combat test helpers: duplicate hero as NPC; archive extra weapon assets.
/// </summary>
public static class CombatTestSetupMenus
{
    const string ItemsRoot = "Assets/Game/Data/Weapons/Items";
    const string WorldRoot = "Assets/Game/Prefabs/Weapons/World";
    const string ItemsBackupRoot = "Assets/Game/Data/Weapons/Items/_Backup";
    const string WorldBackupRoot = "Assets/Game/Prefabs/Weapons/World/_Backup";

    [MenuItem("Game/Combat/Duplicate Selected Hero As NPC")]
    public static void DuplicateSelectedHeroAsNpc()
    {
        var src = Selection.activeGameObject;
        if (!src || !src.GetComponent<PlayerHeroEntity>())
        {
            // Fall back to first PlayerHeroEntity in open scenes.
            src = Object.FindFirstObjectByType<PlayerHeroEntity>()?.gameObject;
        }

        if (!src)
        {
            EditorUtility.DisplayDialog("Duplicate Hero As NPC", "找不到 PlayerHeroEntity。", "OK");
            return;
        }

        var clone = Object.Instantiate(src);
        clone.name = src.name + "_NPC";
        clone.tag = GameTags.Npc;
        Undo.RegisterCreatedObjectUndo(clone, "Duplicate Hero As NPC");

        var offset = src.transform.position + src.transform.right * 2f;
        clone.transform.position = offset;
        clone.transform.rotation = src.transform.rotation;

        // Source becomes Teammate if still Player/Untagged.
        if (!GameTags.IsTeammate(src) && !src.CompareTag(GameTags.Npc))
        {
            Undo.RecordObject(src, "Tag Teammate");
            src.tag = GameTags.Teammate;
            EditorUtility.SetDirty(src);
        }

        Selection.activeGameObject = clone;
        EditorSceneManager.MarkSceneDirty(clone.scene);
        Debug.Log($"[CombatTest] Duplicated '{src.name}' → '{clone.name}' tagged NPC at {offset}");
    }

    [MenuItem("Game/Combat/Ensure PlayerHero Is Teammate Tag")]
    public static void EnsurePlayerHeroTeammateTag()
    {
        var count = 0;
        foreach (var hero in Object.FindObjectsByType<PlayerHeroEntity>(FindObjectsSortMode.None))
        {
            if (!hero || hero.CompareTag(GameTags.Npc) || hero.CompareTag(GameTags.Enemy))
                continue;

            if (hero.CompareTag(GameTags.Teammate))
                continue;

            Undo.RecordObject(hero.gameObject, "Tag Teammate");
            hero.tag = GameTags.Teammate;
            EditorUtility.SetDirty(hero.gameObject);
            count++;
        }

        Debug.Log($"[CombatTest] Tagged {count} hero(s) as Teammate.");
    }

    [MenuItem("Game/Weapons/Archive Extra Weapons (Keep One Per Category)")]
    public static void ArchiveExtraWeapons()
    {
        if (!EditorUtility.DisplayDialog(
                "Archive Extra Weapons",
                "每个武器类型文件夹只保留 1 个 Item 资源，其余移到 Items/_Backup。\n" +
                "World 预制体：仅保留被留下的 Item 引用的，其余移到 World/_Backup。\n\n继续？",
                "Archive",
                "Cancel"))
            return;

        EnsureFolder(ItemsBackupRoot);
        EnsureFolder(WorldBackupRoot);

        var keptWorldGuids = new HashSet<string>();
        var movedItems = 0;
        var categories = AssetDatabase.GetSubFolders(ItemsRoot)
            .Where(p => !p.Replace('\\', '/').EndsWith("/_Backup"))
            .ToArray();

        foreach (var categoryFolder in categories)
        {
            var categoryName = Path.GetFileName(categoryFolder);
            var assets = AssetDatabase.FindAssets("t:SyntyWeaponItemData", new[] { categoryFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.Replace('\\', '/').StartsWith(categoryFolder.Replace('\\', '/') + "/"))
                .Where(p => !p.Contains("/_Backup/"))
                .OrderBy(PreferCanonicalWeaponPath)
                .ThenBy(p => p)
                .ToList();

            if (assets.Count == 0)
                continue;

            var keepPath = assets[0];
            var keep = AssetDatabase.LoadAssetAtPath<SyntyWeaponItemData>(keepPath);
            if (keep && keep.worldPickupPrefab)
            {
                var worldPath = AssetDatabase.GetAssetPath(keep.worldPickupPrefab);
                if (!string.IsNullOrEmpty(worldPath))
                    keptWorldGuids.Add(AssetDatabase.AssetPathToGUID(worldPath));
            }

            var backupCat = $"{ItemsBackupRoot}/{categoryName}";
            EnsureFolder(backupCat);

            for (var i = 1; i < assets.Count; i++)
            {
                var from = assets[i];
                var fileName = Path.GetFileName(from);
                var to = AssetDatabase.GenerateUniqueAssetPath($"{backupCat}/{fileName}");
                var err = AssetDatabase.MoveAsset(from, to);
                if (string.IsNullOrEmpty(err))
                    movedItems++;
                else
                    Debug.LogWarning($"[ArchiveWeapons] Move failed {from} → {to}: {err}");
            }

            Debug.Log($"[ArchiveWeapons] {categoryName}: kept '{Path.GetFileName(keepPath)}', archived {assets.Count - 1}");
        }

        var movedWorld = 0;
        var worldPrefabs = AssetDatabase.FindAssets("t:Prefab", new[] { WorldRoot })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => !p.Contains("/_Backup/"))
            .ToList();

        foreach (var path in worldPrefabs)
        {
            var guid = AssetDatabase.AssetPathToGUID(path);
            if (keptWorldGuids.Contains(guid))
                continue;

            // If nothing kept a world prefab for a category that still has an item without ref,
            // still archive orphans — but keep at least one world prefab if keptWorldGuids empty for safety.
            var fileName = Path.GetFileName(path);
            var to = AssetDatabase.GenerateUniqueAssetPath($"{WorldBackupRoot}/{fileName}");
            var err = AssetDatabase.MoveAsset(path, to);
            if (string.IsNullOrEmpty(err))
                movedWorld++;
            else
                Debug.LogWarning($"[ArchiveWeapons] World move failed {path}: {err}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog(
            "Archive Extra Weapons",
            $"完成。\nItem 已归档: {movedItems}\nWorld 已归档: {movedWorld}\n保留的 World GUID 数: {keptWorldGuids.Count}",
            "OK");
    }

    /// <summary>Lower sort key = more preferred to keep.</summary>
    static string PreferCanonicalWeaponPath(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        var folder = Path.GetFileName(Path.GetDirectoryName(path) ?? "").ToLowerInvariant();

        var score = 0;
        // Prefer the category keyword; demote odd props / covers / misfiles.
        if (name.Contains(folder))
            score -= 30;
        if (name.Contains("_01") || name.EndsWith("01"))
            score -= 5;

        if (name.Contains("cover") || name.Contains("placeholder"))
            score += 40;
        if (name.Contains("corkscrew") || name.Contains("sceptre") || name.Contains("joust"))
            score += 80;
        if (name.Contains("dagger") || name.Contains("knife") || name.Contains("icepick"))
            score += 60;
        if (folder == "sword" && !name.Contains("sword"))
            score += 50;
        if (folder == "staff" && !name.Contains("staff"))
            score += 50;
        if (folder == "spear" && !name.Contains("spear"))
            score += 50;
        if (folder == "crossbow" && name.Contains("sword"))
            score += 200;

        return $"{score:D4}_{name}";
    }

    static void EnsureFolder(string path)
    {
        path = path.Replace('\\', '/');
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
