#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class SetupKenshiCamera
{
    [MenuItem("Game/Camera/Add Kenshi Camera Controller To Main Camera")]
    static void AddToMainCamera()
    {
        var cam = Camera.main;
        if (!cam)
        {
            cam = Object.FindFirstObjectByType<Camera>();
            if (!cam)
            {
                EditorUtility.DisplayDialog("Kenshi Camera", "No Camera found in the open scene.", "OK");
                return;
            }
        }

        if (!cam.GetComponent<KenshiCameraController>())
            cam.gameObject.AddComponent<KenshiCameraController>();

        EditorUtility.SetDirty(cam.gameObject);
        Debug.Log("[SetupKenshiCamera] KenshiCameraController added to " + cam.name);
    }
}
#endif
