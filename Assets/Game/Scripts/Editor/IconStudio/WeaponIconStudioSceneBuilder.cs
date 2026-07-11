#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class WeaponIconStudioSceneBuilder
{
    const string ScenePath = "Assets/Game/Scenes/WeaponIconStudio.unity";

    [MenuItem("Game/Icon Studio/Create Or Update Weapon Icon Studio Scene")]
    public static void CreateOrUpdateScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        ConfigureCamera();
        ConfigureLighting();
        CreatePreviewStage();
        CreateUi();
        EnsureEventSystem();

        var controller = new GameObject("WeaponIconStudio").AddComponent<WeaponIconStudioController>();
        WireControllerReferences(controller);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Weapon Icon Studio",
            $"场景已保存到:\n{ScenePath}\n\n先运行 Game/Weapon/1 生成 CSV，Play 后 F5/F6 批量渲染武器图标。",
            "OK");
    }

    static void ConfigureCamera()
    {
        var camera = Camera.main;
        if (!camera)
            return;

        camera.gameObject.name = "WeaponIconPreviewCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 1.2f;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 50f;
        camera.transform.position = new Vector3(0f, 0f, -8f);
        camera.transform.rotation = Quaternion.identity;
    }

    static void ConfigureLighting()
    {
        var keyLightGo = new GameObject("Key Light");
        var keyLight = keyLightGo.AddComponent<Light>();
        keyLight.type = LightType.Directional;
        keyLight.intensity = 1.1f;
        keyLight.transform.rotation = Quaternion.Euler(38f, -35f, 0f);

        var fillLightGo = new GameObject("Fill Light");
        var fillLight = fillLightGo.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.intensity = 0.45f;
        fillLight.transform.rotation = Quaternion.Euler(10f, 140f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.28f, 0.3f, 0.34f);
    }

    static Transform CreatePreviewStage()
    {
        var stage = new GameObject("WeaponIconPreviewStage");
        stage.transform.position = Vector3.zero;
        return stage.transform;
    }

    static void CreateUi()
    {
        var canvasGo = new GameObject("WeaponIconStudioUI");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1600, 900);
        canvasGo.AddComponent<GraphicRaycaster>();

        var panel = CreateUiObject("Panel", canvasGo.transform);
        var panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.88f);
        AnchorTopBar(panel.GetComponent<RectTransform>(), 380f);

        CreateLabel(panel.transform, "TitleLabel", new Vector2(0f, -18f), new Vector2(760f, 36f), 24).text =
            "Weapon Icon Studio — 武器 UI 渲染";
        CreateDropdown(panel.transform, "EntryDropdown", new Vector2(0f, -72f), new Vector2(760f, 42f));
        CreateButton(panel.transform, "PreviousButton", "上一件 (A)", new Vector2(-250f, -132f), new Vector2(180f, 40f));
        CreateButton(panel.transform, "NextButton", "下一件 (D)", new Vector2(-50f, -132f), new Vector2(180f, 40f));
        CreateButton(panel.transform, "RenderCurrentButton", "渲染当前 (F5)", new Vector2(170f, -132f), new Vector2(180f, 40f));
        CreateButton(panel.transform, "RenderAllButton", "批量渲染 (F6)", new Vector2(370f, -132f), new Vector2(180f, 40f));
        CreateLabel(panel.transform, "InfoLabel", new Vector2(0f, -210f), new Vector2(760f, 120f), 18, TextAlignmentOptions.TopLeft);
        CreateLabel(panel.transform, "HintLabel", new Vector2(0f, -340f), new Vector2(760f, 48f), 16, TextAlignmentOptions.Center);
    }

    static void WireControllerReferences(WeaponIconStudioController controller)
    {
        var so = new SerializedObject(controller);
        so.FindProperty("previewCamera").objectReferenceValue = Camera.main;
        so.FindProperty("previewStage").objectReferenceValue = GameObject.Find("WeaponIconPreviewStage")?.transform;
        so.FindProperty("partMaterial").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Synty/PolygonFantasyHeroCharacters/Materials/FantasyHero.mat");

        var panel = GameObject.Find("Panel");
        if (panel)
        {
            so.FindProperty("entryDropdown").objectReferenceValue = panel.transform.Find("EntryDropdown")?.GetComponent<TMP_Dropdown>();
            so.FindProperty("previousButton").objectReferenceValue = panel.transform.Find("PreviousButton")?.GetComponent<Button>();
            so.FindProperty("nextButton").objectReferenceValue = panel.transform.Find("NextButton")?.GetComponent<Button>();
            so.FindProperty("renderCurrentButton").objectReferenceValue = panel.transform.Find("RenderCurrentButton")?.GetComponent<Button>();
            so.FindProperty("renderAllButton").objectReferenceValue = panel.transform.Find("RenderAllButton")?.GetComponent<Button>();
            so.FindProperty("infoLabel").objectReferenceValue = panel.transform.Find("InfoLabel")?.GetComponent<TextMeshProUGUI>();
            so.FindProperty("hintLabel").objectReferenceValue = panel.transform.Find("HintLabel")?.GetComponent<TextMeshProUGUI>();
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>())
            return;

        var eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    static GameObject CreateUiObject(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(RectTransform rectTransform, float left, float right, float top, float bottom)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = new Vector2(left, bottom);
        rectTransform.offsetMax = new Vector2(-right, -top);
    }

    static void AnchorTopBar(RectTransform rectTransform, float height)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(1f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.sizeDelta = new Vector2(0f, height);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    static TextMeshProUGUI CreateLabel(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
    {
        var go = CreateUiObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        var text = go.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.text = name == "InfoLabel" ? string.Empty : name;
        return text;
    }

    static TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
    {
        var templateRoot = CreateUiObject(name + "_TemplateRoot", parent);
        templateRoot.SetActive(false);
        var templateRootRect = templateRoot.GetComponent<RectTransform>();
        templateRootRect.anchorMin = new Vector2(0f, 0f);
        templateRootRect.anchorMax = new Vector2(1f, 0f);
        templateRootRect.pivot = new Vector2(0.5f, 1f);
        templateRootRect.sizeDelta = new Vector2(0f, 180f);
        templateRootRect.anchoredPosition = new Vector2(0f, 2f);

        var viewport = CreateUiObject("Viewport", templateRoot.transform);
        Stretch(viewport.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        viewport.AddComponent<RectMask2D>();

        var content = CreateUiObject("Content", viewport.transform);
        var contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0f, 28f);

        var item = CreateUiObject("Item", content.transform);
        var itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = new Vector2(0f, 0.5f);
        itemRect.anchorMax = new Vector2(1f, 0.5f);
        itemRect.sizeDelta = new Vector2(0f, 28f);

        var itemBackground = item.AddComponent<Image>();
        itemBackground.color = new Color(0.18f, 0.2f, 0.24f, 1f);
        var itemToggle = item.AddComponent<Toggle>();

        var itemLabelGo = CreateUiObject("Item Label", item.transform);
        Stretch(itemLabelGo.GetComponent<RectTransform>(), 8f, 8f, 0f, 0f);
        var itemLabel = itemLabelGo.AddComponent<TextMeshProUGUI>();
        itemLabel.fontSize = 18;
        itemLabel.alignment = TextAlignmentOptions.MidlineLeft;
        itemToggle.targetGraphic = itemBackground;

        var go = CreateUiObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        var background = go.AddComponent<Image>();
        background.color = new Color(0.16f, 0.18f, 0.22f, 1f);

        var captionGo = CreateUiObject("Label", go.transform);
        Stretch(captionGo.GetComponent<RectTransform>(), 12f, 36f, 0f, 0f);
        var caption = captionGo.AddComponent<TextMeshProUGUI>();
        caption.fontSize = 18;
        caption.alignment = TextAlignmentOptions.MidlineLeft;

        var scrollRect = templateRoot.AddComponent<ScrollRect>();
        scrollRect.content = contentRect;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        var dropdown = go.AddComponent<TMP_Dropdown>();
        dropdown.targetGraphic = background;
        dropdown.captionText = caption;
        dropdown.itemText = itemLabel;
        dropdown.template = templateRootRect;
        return dropdown;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
    {
        var go = CreateUiObject(name, parent);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        var image = go.AddComponent<Image>();
        image.color = new Color(0.22f, 0.28f, 0.34f, 1f);
        var button = go.AddComponent<Button>();
        button.targetGraphic = image;

        var textGo = CreateUiObject("Text", go.transform);
        Stretch(textGo.GetComponent<RectTransform>(), 0f, 0f, 0f, 0f);
        var text = textGo.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 18;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        return button;
    }
}
#endif
