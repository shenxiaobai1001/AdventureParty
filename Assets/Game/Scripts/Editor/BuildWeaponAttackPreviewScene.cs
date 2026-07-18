#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Builds WeaponAttackPreview: one inactive group per weapon family (all non-move/crouch clips).
/// Runtime switcher enables a single group at a time (lazy spawn).
/// Menu: Game → Animation → Build Weapon Attack Preview Scene
/// </summary>
public static class BuildWeaponAttackPreviewScene
{
    const string CharacterPrefabPath =
        "Assets/Game/Animation/RPG Character Mecanim Animation Pack/Prefabs/Character/RPG-Character.prefab";

    const string AnimRoot =
        "Assets/Game/Animation/RPG Character Mecanim Animation Pack/Animations";

    const string ScenePath = "Assets/Game/Scenes/WeaponAttackPreview.unity";

    static readonly Regex LocomotionExclude = new Regex(
        @"Crouch|Walk|Run|Strafe|Turn-",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    struct GroupDef
    {
        public string name;
        public WeaponAttackPreviewKit kit;
        public string folder;
    }

    [MenuItem("Game/Animation/Build Weapon Attack Preview Scene")]
    public static void Build()
    {
        var characterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CharacterPrefabPath);
        if (!characterPrefab)
        {
            EditorUtility.DisplayDialog(
                "Weapon Attack Preview",
                "Missing RPG-Character prefab:\n" + CharacterPrefabPath,
                "OK");
            return;
        }

        var defs = BuildGroupDefinitions();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var root = new GameObject("WeaponAttackPreviewRoot");
        var groupsRoot = new GameObject("Groups");
        groupsRoot.transform.SetParent(root.transform, false);

        var light = UnityEngine.Object.FindFirstObjectByType<Light>();
        if (light)
        {
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(root.transform, false);
        ground.transform.localScale = new Vector3(
            Mathf.Max(20f, WeaponAttackPreviewGroup.Columns * WeaponAttackPreviewGroup.ActorSpacing * 0.12f),
            1f,
            40f);

        var builtGroups = new List<WeaponAttackPreviewGroup>();
        var totalClips = 0;
        var maxClips = 0;

        try
        {
            for (var gi = 0; gi < defs.Count; gi++)
            {
                var def = defs[gi];
                EditorUtility.DisplayProgressBar(
                    "Weapon Attack Preview",
                    $"Collecting {def.name} ({gi + 1}/{defs.Count})",
                    (gi + 1f) / defs.Count);

                var (clips, labels) = CollectClips(def.folder);
                if (clips.Length == 0)
                {
                    Debug.LogWarning($"[AttackPreview] No clips for {def.name} in {def.folder}");
                    continue;
                }

                totalClips += clips.Length;
                maxClips = Mathf.Max(maxClips, clips.Length);

                var groupGo = new GameObject(def.name);
                groupGo.transform.SetParent(groupsRoot.transform, false);
                var group = groupGo.AddComponent<WeaponAttackPreviewGroup>();
                group.Configure(def.name, def.kit, characterPrefab, clips, labels);
                groupGo.SetActive(false);
                builtGroups.Add(group);
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        if (builtGroups.Count == 0)
        {
            EditorUtility.DisplayDialog("Weapon Attack Preview", "No animation groups found.", "OK");
            return;
        }

        SetupCamera(maxClips);
        var (title, prev, next) = CreateUi(root.transform);
        var switcher = root.AddComponent<WeaponAttackPreviewSwitcher>();
        switcher.Configure(builtGroups.ToArray(), title, prev, next);

        EnsureFolder("Assets/Game/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon Attack Preview",
            $"Created {ScenePath}\n" +
            $"Groups: {builtGroups.Count}\n" +
            $"Clips catalogued: {totalClips}\n" +
            $"Largest group: {maxClips}\n\n" +
            "Play, then use Prev/Next (or A/D) to switch weapons.\n" +
            "Only one group spawns actors at a time.",
            "OK");

        EditorSceneManager.OpenScene(ScenePath);
    }

    static List<GroupDef> BuildGroupDefinitions()
    {
        return new List<GroupDef>
        {
            G("01_2H_Sword", WeaponAttackPreviewKit.TwoHandSword, "2Hand-Sword"),
            G("02_2H_Spear", WeaponAttackPreviewKit.TwoHandSpear, "2Hand-Spear"),
            G("03_2H_Axe", WeaponAttackPreviewKit.TwoHandAxe, "2Hand-Axe"),
            G("04_2H_Staff", WeaponAttackPreviewKit.TwoHandStaff, "2Hand-Staff"),
            G("05_2H_Bow", WeaponAttackPreviewKit.TwoHandBow, "2Hand-Bow"),
            G("06_2H_Crossbow", WeaponAttackPreviewKit.TwoHandCrossbow, "2Hand-Crossbow"),
            G("07_2H_Rifle", WeaponAttackPreviewKit.TwoHandRifle, "2Hand-Shooting"),
            G("08_1H_Sword", WeaponAttackPreviewKit.SwordRight, "1Hand-Sword"),
            G("09_1H_Mace", WeaponAttackPreviewKit.MaceRight, "1Hand-Mace"),
            G("10_1H_Dagger", WeaponAttackPreviewKit.DaggerRight, "1Hand-Dagger"),
            G("11_1H_Spear", WeaponAttackPreviewKit.SpearRight, "1Hand-Spear"),
            G("12_1H_Item", WeaponAttackPreviewKit.ItemRight, "1Hand-Item"),
            G("13_1H_Pistol", WeaponAttackPreviewKit.PistolRight, "1Hand-Pistol"),
            G("14_Armed", WeaponAttackPreviewKit.SwordDual, "Armed"),
            G("15_Armed_Shield", WeaponAttackPreviewKit.ShieldSword, "Armed-Shield"),
            G("16_Unarmed", WeaponAttackPreviewKit.None, "Unarmed"),
        };
    }

    static GroupDef G(string name, WeaponAttackPreviewKit kit, string folder)
    {
        return new GroupDef
        {
            name = name,
            kit = kit,
            folder = $"{AnimRoot}/{folder}".Replace('\\', '/'),
        };
    }

    static (AnimationClip[] clips, string[] labels) CollectClips(string folder)
    {
        if (!AssetDatabase.IsValidFolder(folder))
            return (Array.Empty<AnimationClip>(), Array.Empty<string>());

        var guids = AssetDatabase.FindAssets("t:Model", new[] { folder });
        var entries = new List<(string path, string file, AnimationClip clip)>();

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
            if (path.IndexOf("/Old Pose/", StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            var file = Path.GetFileName(path);
            if (LocomotionExclude.IsMatch(file))
                continue;

            var clip = LoadPrimaryClip(path);
            if (!clip)
                continue;

            entries.Add((path, Path.GetFileNameWithoutExtension(path), clip));
        }

        entries.Sort((a, b) => string.Compare(a.file, b.file, StringComparison.OrdinalIgnoreCase));

        return (
            entries.Select(e => e.clip).ToArray(),
            entries.Select(e => e.file).ToArray());
    }

    static AnimationClip LoadPrimaryClip(string fbxPath)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        foreach (var asset in assets)
        {
            if (asset is not AnimationClip clip)
                continue;
            if (clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                continue;
            return clip;
        }

        return null;
    }

    static void SetupCamera(int maxClipsInGroup)
    {
        var cam = Camera.main;
        if (!cam)
        {
            var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            cam = camGo.GetComponent<Camera>();
        }

        var cols = Mathf.Min(WeaponAttackPreviewGroup.Columns, Mathf.Max(1, maxClipsInGroup));
        var rows = Mathf.CeilToInt(maxClipsInGroup / (float)WeaponAttackPreviewGroup.Columns);
        var width = cols * WeaponAttackPreviewGroup.ActorSpacing;
        var depth = Mathf.Max(1, rows) * WeaponAttackPreviewGroup.RowSpacing;

        cam.transform.position = new Vector3(width * 0.4f, Mathf.Max(16f, depth * 0.35f), -Mathf.Max(14f, depth * 0.25f));
        cam.orthographic = false;
        cam.fieldOfView = 55f;
        cam.transform.LookAt(new Vector3(width * 0.4f, 1f, depth * 0.35f));
    }

    static (TextMeshProUGUI title, Button prev, Button next) CreateUi(Transform parent)
    {
        var canvasGo = new GameObject("PreviewUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.transform.SetParent(parent, false);
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        if (!UnityEngine.Object.FindFirstObjectByType<EventSystem>())
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(parent, false);
        }

        var titleGo = new GameObject("Title", typeof(RectTransform));
        titleGo.transform.SetParent(canvasGo.transform, false);
        var titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -20f);
        titleRt.sizeDelta = new Vector2(1100f, 90f);
        var title = titleGo.AddComponent<TextMeshProUGUI>();
        title.alignment = TextAlignmentOptions.Center;
        title.fontSize = 36f;
        title.color = Color.white;
        title.text = "Weapon Preview";

        var prev = CreateButton(canvasGo.transform, "Prev", new Vector2(-220f, 40f), "◀ Prev");
        var next = CreateButton(canvasGo.transform, "Next", new Vector2(220f, 40f), "Next ▶");

        return (title, prev, next);
    }

    static Button CreateButton(Transform parent, string name, Vector2 anchoredPos, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(180f, 56f);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.18f, 0.92f);

        var textGo = new GameObject("Label", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize = 28f;
        tmp.color = Color.white;

        return go.GetComponent<Button>();
    }

    static void EnsureFolder(string assetPath)
    {
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
