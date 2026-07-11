#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class IconStudioBatchMenu
{
    [MenuItem("Game/Icon Studio/Batch Render Forearm Icons (Play Mode)")]
    public static void BatchRenderForearm()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Icon Studio",
                "请先打开 IconStudio 场景并进入 Play 模式，再执行此菜单。",
                "OK");
            return;
        }

        var controller = Object.FindFirstObjectByType<IconStudioController>();
        if (!controller)
        {
            EditorUtility.DisplayDialog(
                "Icon Studio",
                "未找到 IconStudioController。请打开 Assets/Game/Scenes/IconStudio.unity。",
                "OK");
            return;
        }

        controller.StartBatchRenderForSlot(SyntyEquipmentSlot.Forearm);
    }
}
#endif
