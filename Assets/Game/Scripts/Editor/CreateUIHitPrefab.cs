#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CreateUIHitPrefab
{
    const string PrefabPath = "Assets/Game/Resources_moved/Prefabs/UIHit.prefab";

    [MenuItem("Game/Create UIHit Prefab")]
    public static void CreatePrefab()
    {
        var root = new GameObject("UIHit", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(UIHitController));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var centerGo = new GameObject("center", typeof(RectTransform));
        centerGo.transform.SetParent(root.transform, false);
        var centerRt = centerGo.GetComponent<RectTransform>();
        centerRt.anchorMin = Vector2.zero;
        centerRt.anchorMax = Vector2.one;
        centerRt.offsetMin = Vector2.zero;
        centerRt.offsetMax = Vector2.zero;

        var textGo = new GameObject("text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(UIHitFloatText));
        textGo.transform.SetParent(centerGo.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(400, 80);
        textRt.anchoredPosition = Vector2.zero;

        var text = textGo.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 36;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.92f, 0.2f, 1f);
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = "+100";

        var controller = root.GetComponent<UIHitController>();
        var so = new SerializedObject(controller);
        so.FindProperty("center").objectReferenceValue = centerRt;
        so.FindProperty("textTemplate").objectReferenceValue = text;
        so.ApplyModifiedPropertiesWithoutUndo();

        textGo.SetActive(false);

        EnsureFolder("Assets/Game/Resources_moved/Prefabs");
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"UIHit 预制体已生成: {PrefabPath}");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        var name = System.IO.Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(name))
            AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
