#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class FixRpgDemoForURP
{
    const string CameraPrefabPath =
        "Assets/ExplosiveLLC/RPG Character Mecanim Animation Pack/Demo Elements/Prefabs/Camera.prefab";

    [MenuItem("Tools/Rendering/Fix RPG Demo Cameras for URP")]
    public static void FixCameras()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CameraPrefabPath);
        if (!prefab)
        {
            Debug.LogError($"Camera prefab not found: {CameraPrefabPath}");
            return;
        }

        var root = PrefabUtility.LoadPrefabContents(CameraPrefabPath);
        var cameras = root.GetComponentsInChildren<Camera>(true);
        UniversalAdditionalCameraData baseData = null;

        foreach (var cam in cameras)
        {
            cam.renderingPath = RenderingPath.UsePlayerSettings;
            var urp = cam.GetComponent<UniversalAdditionalCameraData>();
            if (!urp)
                urp = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

            urp.renderPostProcessing = false;

            var isGuiCamera = cam.gameObject.name == "GUI Camera";
            if (isGuiCamera)
            {
                cam.clearFlags = CameraClearFlags.Depth;
                urp.renderType = CameraRenderType.Overlay;
                urp.renderShadows = false;
            }
            else
            {
                cam.clearFlags = CameraClearFlags.Skybox;
                urp.renderType = CameraRenderType.Base;
                urp.renderShadows = true;
                baseData = urp;
            }
        }

        if (baseData != null)
        {
            baseData.cameraStack.Clear();
            foreach (var cam in cameras)
            {
                if (cam.gameObject.name != "GUI Camera")
                    continue;

                var overlay = cam.GetComponent<UniversalAdditionalCameraData>();
                if (overlay != null && overlay.renderType == CameraRenderType.Overlay)
                    baseData.cameraStack.Add(cam);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(root, CameraPrefabPath);
        PrefabUtility.UnloadPrefabContents(root);
        AssetDatabase.SaveAssets();

        Debug.Log("RPG demo Camera prefab updated for URP.");
    }
}
#endif
