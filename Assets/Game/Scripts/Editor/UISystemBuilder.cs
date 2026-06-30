#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class UISystemBuilder
{
    [MenuItem("Game/UI/Create UISystem Panel In Scene")]
    public static void CreateUISystemPanel()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景中未找到 Canvas");
            return;
        }

        Transform center = canvas.transform.Find("Center");
        if (center == null)
        {
            GameObject centerGo = CreateUIObject("Center", canvas.transform);
            StretchFull(centerGo.GetComponent<RectTransform>());
            center = centerGo.transform;
        }

        Transform old = center.Find("UISystem");
        if (old != null)
            Object.DestroyImmediate(old.gameObject);

        GameObject panel = CreateUIObject("UISystem", center);
        RectTransform panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.sizeDelta = new Vector2(520, 420);
        panelRt.anchoredPosition = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.12f, 0.92f);

        UISystem ui = panel.AddComponent<UISystem>();
        ui.bgImage = bg;

        float y = 170f;
        const float rowH = 36f;
        const float gap = 8f;

        CreateLabel(panel.transform, "音乐音量", new Vector2(-180, y), new Vector2(100, rowH));
        ui.musicSlider = CreateSlider(panel.transform, "Slider_Music", new Vector2(-20, y), new Vector2(220, rowH));
        ui.musicValueText = CreateLabel(panel.transform, "50%", new Vector2(200, y), new Vector2(60, rowH));
        ui.musicLabelText = null;

        y -= rowH + gap;
        CreateLabel(panel.transform, "音效音量", new Vector2(-180, y), new Vector2(100, rowH));
        ui.soundSlider = CreateSlider(panel.transform, "Slider_Sound", new Vector2(-20, y), new Vector2(220, rowH));
        ui.soundValueText = CreateLabel(panel.transform, "100%", new Vector2(200, y), new Vector2(60, rowH));
        ui.soundLabelText = null;

        y -= rowH + gap;
        ui.toggleShowPlayer = CreateToggle(panel.transform, "Toggle_ShowPlayer", "显示人物名字", new Vector2(0, y), new Vector2(300, rowH));

        y -= rowH + gap;
        ui.toggleShowKid = CreateToggle(panel.transform, "Toggle_ShowKid", "显示小孩名字", new Vector2(0, y), new Vector2(300, rowH));

        y -= rowH + gap;
        ui.toggleShowRoad = CreateToggle(panel.transform, "Toggle_ShowRoad", "显示马路名字", new Vector2(0, y), new Vector2(300, rowH));

        y -= rowH + gap + 4f;
        ui.inputField = CreateInputField(panel.transform, "InputField", new Vector2(0, y), new Vector2(400, rowH + 8));

        y -= rowH + gap + 12f;
        CreateLabel(panel.transform, "小孩皮肤", new Vector2(-180, y), new Vector2(100, rowH));
        ui.dropdownKidSkin = CreateDropdown(panel.transform, "Dropdown_KidSkin", new Vector2(40, y), new Vector2(260, rowH));

        Selection.activeGameObject = panel;
        EditorGUIUtility.PingObject(panel);
        Debug.Log("UISystem 面板已创建：Canvas/Center/UISystem，可在 Inspector 中微调布局。");
    }

    static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static Text CreateLabel(Transform parent, string text, Vector2 pos, Vector2 size)
    {
        GameObject go = CreateUIObject("Text", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text t = go.AddComponent<Text>();
        t.text = text;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = 18;
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleLeft;
        go.AddComponent<CanvasRenderer>();
        return t;
    }

    static Slider CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject go = DefaultControls.CreateSlider(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Slider s = go.GetComponent<Slider>();
        s.minValue = 0f;
        s.maxValue = 1f;
        s.value = 0.5f;
        return s;
    }

    static Toggle CreateToggle(Transform parent, string name, string label, Vector2 pos, Vector2 size)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject go = DefaultControls.CreateToggle(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Text labelText = go.GetComponentInChildren<Text>();
        if (labelText != null)
            labelText.text = label;
        return go.GetComponent<Toggle>();
    }

    static InputField CreateInputField(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject go = DefaultControls.CreateInputField(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go.GetComponent<InputField>();
    }

    static Dropdown CreateDropdown(Transform parent, string name, Vector2 pos, Vector2 size)
    {
        DefaultControls.Resources res = new DefaultControls.Resources();
        GameObject go = DefaultControls.CreateDropdown(res);
        go.name = name;
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go.GetComponent<Dropdown>();
    }
}
#endif
