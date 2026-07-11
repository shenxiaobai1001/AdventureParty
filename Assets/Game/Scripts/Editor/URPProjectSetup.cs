using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class URPProjectSetup
{
    private const string SettingsFolder = "Assets/Settings";
    private const string RendererPath = SettingsFolder + "/URP-ForwardRenderer.asset";
    private const string PipelinePath = SettingsFolder + "/URP-Asset.asset";
    private const string SkyboxMatPath = SettingsFolder + "/URP-ProceduralSkybox.mat";

    private static readonly string[] ExplosiveScenePaths =
    {
        "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack/Scenes/RPG-Character.unity",
        "Assets/ExplosiveLLC/SuperCharacterController/Scenes/TestZone-RPGCharacter.unity",
        "Assets/ExplosiveLLC/SuperCharacterController/Scenes/TestZone.unity",
        "Assets/ExplosiveLLC/SuperCharacterController/Scenes/SpaceZone.unity",
        "Assets/ExplosiveLLC/SuperCharacterController/Scenes/TerrainTest.unity",
        "Assets/ExplosiveLLC/PerfectLookAt/Scenes/PerfectLookAt_Scene1_UnityModel.unity",
        "Assets/ExplosiveLLC/PerfectLookAt/Scenes/PerfectLookAt_Scene2_Human.unity",
        "Assets/ExplosiveLLC/PerfectLookAt/Scenes/PerfectLookAt_Scene3_RobotKyle.unity",
        "Assets/ExplosiveLLC/PerfectLookAt/Scenes/PerfectLookAt_Scene4_PerformanceTest.unity",
    };

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

    public static void UpgradeExplosiveLLCFromCommandLine()
    {
        UpgradeExplosiveLLCMaterials();
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

    [MenuItem("Tools/Rendering/Upgrade ExplosiveLLC Materials to URP")]
    public static void UpgradeExplosiveLLCMaterials()
    {
        EnsureFolder(SettingsFolder);
        var skybox = GetOrCreateProceduralSkybox();

        var upgradedCount = UpgradeMaterialsInFolder("Assets/ExplosiveLLC");
        var sceneCount = FixExplosiveLLCSceneSkyboxes(skybox);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"ExplosiveLLC URP upgrade done. Materials: {upgradedCount}, Scenes skybox fixed: {sceneCount}.");
    }

    private static void UpgradeProjectMaterials()
    {
        UpgradeMaterialsInFolder("Assets/Game");
    }

    private static int UpgradeMaterialsInFolder(string rootFolder)
    {
        var upgradedCount = 0;
        var materialGuids = AssetDatabase.FindAssets("t:Material", new[] { rootFolder });
        foreach (var guid in materialGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
                continue;

            var shaderName = material.shader.name;
            if (IsAlreadyURP(shaderName))
                continue;

            if (UpgradeMaterialToURP(material))
            {
                EditorUtility.SetDirty(material);
                upgradedCount++;
            }
        }

        return upgradedCount;
    }

    private static bool IsAlreadyURP(string shaderName)
    {
        return shaderName.StartsWith("Universal Render Pipeline/") ||
               shaderName.StartsWith("Shader Graphs/") ||
               shaderName.StartsWith("Sprites/") ||
               shaderName.StartsWith("UI/") ||
               shaderName.StartsWith("Skybox/");
    }

    private static bool UpgradeMaterialToURP(Material material)
    {
        var shaderName = material.shader.name;
        var mainTex = material.HasProperty("_MainTex") ? material.GetTexture("_MainTex") : null;
        var color = material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
        var emissionColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
        var glossiness = material.HasProperty("_Glossiness") ? material.GetFloat("_Glossiness") : 0.5f;
        var metallic = material.HasProperty("_Metallic") ? material.GetFloat("_Metallic") : 0f;
        var mode = material.HasProperty("_Mode") ? material.GetFloat("_Mode") : 0f;
        var cutoff = material.HasProperty("_Cutoff") ? material.GetFloat("_Cutoff") : 0.5f;

        string urpShaderName = null;
        if (shaderName == "Standard" ||
            shaderName == "Standard (Specular setup)" ||
            shaderName == "Legacy Shaders/Diffuse" ||
            shaderName == "Legacy Shaders/Specular" ||
            shaderName == "Legacy Shaders/Transparent/Diffuse" ||
            shaderName == "Mobile/Diffuse")
        {
            urpShaderName = mode >= 2f ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
        }
        else if (shaderName == "Unlit/Color" || shaderName == "Unlit/Texture")
        {
            urpShaderName = "Universal Render Pipeline/Unlit";
        }
        else if (shaderName == "Sprites/Default")
        {
            urpShaderName = "Universal Render Pipeline/2D/Sprite-Lit-Default";
        }

        if (urpShaderName == null)
            return false;

        var urpShader = Shader.Find(urpShaderName);
        if (urpShader == null)
            return false;

        material.shader = urpShader;

        if (material.HasProperty("_BaseMap") && mainTex != null)
            material.SetTexture("_BaseMap", mainTex);

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", glossiness);

        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", metallic);

        if (material.HasProperty("_Cutoff"))
            material.SetFloat("_Cutoff", cutoff);

        if (emissionColor.maxColorComponent > 0f)
        {
            if (material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", emissionColor);
            material.EnableKeyword("_EMISSION");
        }

        if (mode >= 1f)
            ApplyTransparentSurface(material, mode >= 3f);

        return true;
    }

    private static void ApplyTransparentSurface(Material material, bool fade)
    {
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", fade ? 0f : 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = (int)RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword(fade ? "_ALPHAPREMULTIPLY_ON" : "_ALPHAMODULATE_ON");
        material.SetShaderPassEnabled("DepthOnly", false);
        material.SetShaderPassEnabled("SHADOWCASTER", false);
    }

    private static Material GetOrCreateProceduralSkybox()
    {
        var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMatPath);
        if (skybox != null)
            return skybox;

        var shader = Shader.Find("Skybox/Procedural");
        if (shader == null)
        {
            Debug.LogWarning("Skybox/Procedural shader not found; skybox scenes may stay pink.");
            return null;
        }

        skybox = new Material(shader)
        {
            name = "URP-ProceduralSkybox"
        };
        skybox.SetColor("_SkyTint", new Color(0.5f, 0.5f, 0.5f, 1f));
        skybox.SetColor("_GroundColor", new Color(0.37f, 0.35f, 0.34f, 1f));
        skybox.SetFloat("_SunSize", 0.04f);
        skybox.SetFloat("_AtmosphereThickness", 1f);
        skybox.SetFloat("_Exposure", 1.3f);

        AssetDatabase.CreateAsset(skybox, SkyboxMatPath);
        return skybox;
    }

    private static int FixExplosiveLLCSceneSkyboxes(Material skybox)
    {
        if (skybox == null)
            return 0;

        var fixedCount = 0;
        var activeScenePath = SceneManager.GetActiveScene().path;

        foreach (var scenePath in ExplosiveScenePaths)
        {
            if (!File.Exists(scenePath))
                continue;

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (RenderSettings.skybox == null || IsBuiltInBrokenSkybox(RenderSettings.skybox))
            {
                RenderSettings.skybox = skybox;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                fixedCount++;
            }
        }

        if (!string.IsNullOrEmpty(activeScenePath) && File.Exists(activeScenePath))
            EditorSceneManager.OpenScene(activeScenePath, OpenSceneMode.Single);

        return fixedCount;
    }

    private static bool IsBuiltInBrokenSkybox(Material skyboxMaterial)
    {
        return skyboxMaterial != null &&
               skyboxMaterial.shader != null &&
               (skyboxMaterial.shader.name == "Skybox/Default" ||
                skyboxMaterial.shader.name == "Hidden/InternalErrorShader");
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
