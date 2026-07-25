#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Attaches UIEquipPanel to the UIEquip prefab / selection.</summary>
public static class SetupUIEquipPanel
{
    const string PrefabPath = "Assets/Game/Resources_moved/Prefabs/UI/UIEquip.prefab";

    [MenuItem("Game/UI/Setup UIEquip Panel Script")]
    public static void Setup()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (!prefab)
        {
            EditorUtility.DisplayDialog("UIEquip", "找不到 " + PrefabPath, "OK");
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        var panel = root.GetComponent<UIEquipPanel>();
        if (!panel)
            panel = root.AddComponent<UIEquipPanel>();

        var so = new SerializedObject(panel);
        var equip = root.transform.Find("button/btn_equip");
        var unequip = root.transform.Find("button/btn_unEquip");
        if (equip)
            so.FindProperty("equipButton").objectReferenceValue = equip.GetComponent<UnityEngine.UI.Button>();
        if (unequip)
            so.FindProperty("unequipButton").objectReferenceValue = unequip.GetComponent<UnityEngine.UI.Button>();
        so.FindProperty("panelRoot").objectReferenceValue = root.GetComponent<RectTransform>();
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        EditorUtility.DisplayDialog("UIEquip", "已挂载 UIEquipPanel 并绑定按钮。", "OK");
    }
}
#endif
