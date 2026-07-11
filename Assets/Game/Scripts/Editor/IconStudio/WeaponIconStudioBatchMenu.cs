#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class WeaponIconStudioBatchMenu
{
    [MenuItem("Game/Icon Studio/Batch Render Weapon Icons (Play Mode)")]
    public static void BatchRenderWeapons()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Weapon Icon Studio",
                "请先打开 WeaponIconStudio 场景并进入 Play 模式，再执行此菜单。",
                "OK");
            return;
        }

        var controller = Object.FindFirstObjectByType<WeaponIconStudioController>();
        if (!controller)
        {
            EditorUtility.DisplayDialog(
                "Weapon Icon Studio",
                "未找到 WeaponIconStudioController。请打开 Assets/Game/Scenes/WeaponIconStudio.unity。",
                "OK");
            return;
        }

        controller.StartBatchRenderAll();
    }
}
#endif
