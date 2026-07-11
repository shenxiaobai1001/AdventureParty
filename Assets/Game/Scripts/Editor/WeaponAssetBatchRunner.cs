#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Unity batch entry point, e.g.
/// Unity.exe -batchmode -quit -projectPath ... -executeMethod WeaponAssetBatchRunner.GenerateAllAndScenes
/// </summary>
public static class WeaponAssetBatchRunner
{
    public static void GenerateAllAndScenes()
    {
        GenerateWeaponAssets.GenerateAll();
        WeaponIconStudioSceneBuilder.CreateOrUpdateScene();
        WeaponTestSceneBuilder.CreateOrUpdateScene();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }
}
#endif
