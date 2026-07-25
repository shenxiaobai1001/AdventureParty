#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CombatSimMenus
{
    [MenuItem("Game/Combat/Add CombatSimOpponent To Selected")]
    public static void AddSimToSelected()
    {
        var go = Selection.activeGameObject;
        if (!go)
        {
            EditorUtility.DisplayDialog("Combat Sim", "先选中场景中的 NPC。", "OK");
            return;
        }

        var sim = go.GetComponent<CombatSimOpponent>();
        if (!sim)
            sim = Undo.AddComponent<CombatSimOpponent>(go);

        if (!go.GetComponent<CombatHealth>())
            Undo.AddComponent<CombatHealth>(go);

        EditorUtility.SetDirty(go);
        Debug.Log($"[CombatSim] Added CombatSimOpponent to '{go.name}'. Set Strength in Inspector.");
    }
}
#endif
