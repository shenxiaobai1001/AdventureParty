#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WeaponTestSceneBuilder
{
    const string ScenePath = "Assets/Game/Scenes/WeaponTest.unity";
    const string PlayerPrefabPath = "Assets/Game/Resources_moved/Prefabs/Characters/PlayerHero_Base.prefab";
    const string MainControlPanelPath = "Assets/Game/Resources_moved/Prefabs/UI/UIMainControlPanel.prefab";

    static readonly string[] SampleWorldPrefabPaths =
    {
        "Assets/Game/Prefabs/Weapons/World/hero_sm_wep_sword_01_World.prefab",
        "Assets/Game/Prefabs/Weapons/World/hero_sm_wep_shield_buckler_01_World.prefab",
        "Assets/Game/Prefabs/Weapons/World/kingdom_sm_wep_spear_01_World.prefab",
        "Assets/Game/Prefabs/Weapons/World/kingdom_sm_prop_bow_01_World.prefab",
        "Assets/Game/Prefabs/Weapons/World/kingdom_sm_wep_hammer_01_World.prefab",
    };

    [MenuItem("Game/Weapon/Create Weapon Test Scene")]
    public static void CreateOrUpdateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        CreateGround();
        ConfigureLighting();
        CreatePlayer();
        CreateUiRoot();
        PlaceSamplePickups();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon Test Scene",
            $"场景已保存到:\n{ScenePath}\n\n请先运行 Game/Weapon/Generate All，再打开 WeaponTest 场景测试拾取→背包→武器栏。",
            "OK");
    }

    static void CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
    }

    static void ConfigureLighting()
    {
        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void CreatePlayer()
    {
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (!playerPrefab)
        {
            Debug.LogWarning($"[WeaponTestScene] Missing player prefab: {PlayerPrefabPath}");
            return;
        }

        var player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        if (!player)
            return;

        player.name = "PlayerHero";
        player.transform.position = new Vector3(0f, 0f, -3f);

        if (!player.GetComponent<EquipmentPickupInteractor>())
            player.AddComponent<EquipmentPickupInteractor>();
    }

    static void CreateUiRoot()
    {
        var uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainControlPanelPath);
        if (!uiPrefab)
            return;

        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (!canvas)
        {
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }

        PrefabUtility.InstantiatePrefab(uiPrefab, canvas.transform);
    }

    static void PlaceSamplePickups()
    {
        var root = new GameObject("WeaponPickups");
        var offsetX = -4f;

        foreach (var path in SampleWorldPrefabPaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (!prefab)
                continue;

            var instance = PrefabUtility.InstantiatePrefab(prefab, root.transform) as GameObject;
            if (!instance)
                continue;

            instance.transform.position = new Vector3(offsetX, 0.2f, 2f);
            offsetX += 2f;
        }
    }
}
#endif
