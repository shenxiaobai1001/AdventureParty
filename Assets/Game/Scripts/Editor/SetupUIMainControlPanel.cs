using UnityEditor;
using UnityEngine;

public static class SetupUIMainControlPanel
{
    const string PanelPrefabPath = "Assets/Game/Resources_moved/Prefabs/UI/UIMainControlPanel.prefab";
    const string HeroPrefabPath = "Assets/Game/Resources_moved/Prefabs/Characters/PlayerHero_Base.prefab";

    [MenuItem("Game/UI/Setup UIMainControlPanel")]
    public static void Setup()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPrefabPath);
        if (!prefab)
        {
            Debug.LogError("[SetupUIMainControlPanel] Prefab not found: " + PanelPrefabPath);
            return;
        }

        var heroPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(HeroPrefabPath);
        var root = PrefabUtility.LoadPrefabContents(PanelPrefabPath);

        var panel = root.GetComponent<UIMainControlPanel>();
        if (!panel)
            panel = root.AddComponent<UIMainControlPanel>();

        var so = new SerializedObject(panel);
        so.FindProperty("playerHeroPrefab").objectReferenceValue = heroPrefab;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PanelPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();

        Debug.Log("[SetupUIMainControlPanel] UIMainControlPanel attached to prefab.");
    }
}
