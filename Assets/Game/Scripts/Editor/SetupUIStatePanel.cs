#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SetupUIStatePanel
{
    const string PanelPrefabPath = "Assets/Game/Resources_moved/Prefabs/UI/UIStatePanel.prefab";

    [MenuItem("Game/UI/Setup UIStatePanel")]
    public static void Setup()
    {
        var root = PrefabUtility.LoadPrefabContents(PanelPrefabPath);
        if (!root)
        {
            Debug.LogError("[SetupUIStatePanel] Prefab not found: " + PanelPrefabPath);
            return;
        }

        try
        {
            var panel = root.GetComponent<UIStatePanel>();
            if (!panel)
                panel = root.AddComponent<UIStatePanel>();

            PrefabUtility.SaveAsPrefabAsset(root, PanelPrefabPath);
            Debug.Log("[SetupUIStatePanel] UIStatePanel attached to prefab.");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }
}
#endif
