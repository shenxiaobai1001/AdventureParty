using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class URPProjectSetup
{
    private const string SettingsFolder = "Assets/Settings";
    private const string RendererPath = SettingsFolder + "/URP-ForwardRenderer.asset";
    private const string PipelinePath = SettingsFolder + "/URP-Asset.asset";

    static URPProjectSetup()
    {
        EditorApplication.delayCall += TryAutoAssignPipeline;
    }

    private static void TryAutoAssignPipeline()
    {
        if (GraphicsSettings.defaultRenderPipeline != null)
            return;

        var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipelineAsset == null)
            return;

        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;
        UpgradeProjectMaterials();
        Debug.Log("URP pipeline auto-assigned.");
    }

    public static void SetupFromCommandLine()
    {
        Setup();
        EditorApplication.Exit(0);
    }

    [MenuItem("Tools/Rendering/Setup URP Pipeline")]
    public static void Setup()
    {
        EnsureFolder(SettingsFolder);

        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, RendererPath);
        }

        var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipelineAsset == null)
        {
            pipelineAsset = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
            AssetDatabase.CreateAsset(pipelineAsset, PipelinePath);
        }

        var pipelineSerialized = new SerializedObject(pipelineAsset);
        var rendererList = pipelineSerialized.FindProperty("m_RendererDataList");
        rendererList.ClearArray();
        rendererList.InsertArrayElementAtIndex(0);
        rendererList.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
        pipelineSerialized.FindProperty("m_DefaultRendererIndex").intValue = 0;
        pipelineSerialized.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(pipelineAsset);
        EditorUtility.SetDirty(renderer);
        AssetDatabase.SaveAssets();

        GraphicsSettings.defaultRenderPipeline = pipelineAsset;
        QualitySettings.renderPipeline = pipelineAsset;

        UpgradeProjectMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("URP pipeline setup completed.");
    }

    [MenuItem("Tools/Rendering/Upgrade Game Materials to URP")]
    public static void UpgradeMaterialsMenu()
    {
        UpgradeProjectMaterials();
        AssetDatabase.SaveAssets();
        Debug.Log("Game materials upgraded to URP where possible.");
    }

    private static void UpgradeProjectMaterials()
    {
        var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Game" });
        foreach (var guid in materialGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
                continue;

            var shaderName = material.shader.name;
            if (shaderName.StartsWith("Universal Render Pipeline/") ||
                shaderName.StartsWith("Shader Graphs/") ||
                shaderName.StartsWith("Sprites/") ||
                shaderName.StartsWith("UI/"))
                continue;

            if (UpgradeMaterialToURP(material))
                EditorUtility.SetDirty(material);
        }
    }

    private static bool UpgradeMaterialToURP(Material material)
    {
        var shaderName = material.shader.name;
        string urpShaderName = null;

        if (shaderName == "Standard" || shaderName == "Legacy Shaders/Diffuse")
            urpShaderName = "Universal Render Pipeline/Lit";
        else if (shaderName == "Unlit/Color" || shaderName == "Unlit/Texture")
            urpShaderName = "Universal Render Pipeline/Unlit";
        else if (shaderName == "Sprites/Default")
            urpShaderName = "Universal Render Pipeline/2D/Sprite-Lit-Default";

        if (urpShaderName == null)
            return false;

        var urpShader = Shader.Find(urpShaderName);
        if (urpShader == null)
            return false;

        material.shader = urpShader;
        return true;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
                AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
