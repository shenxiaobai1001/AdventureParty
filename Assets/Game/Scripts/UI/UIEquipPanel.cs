using UInventoryGrid;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Right-click weapon bar → Equip / Unequip panel (UIEquip).
/// Hierarchy: UIEquip / button / {btn_equip, btn_unEquip}
/// </summary>
public class UIEquipPanel : MonoBehaviour
{
    const string PrefabAssetPath = "Assets/Game/Resources_moved/Prefabs/UI/UIEquip.prefab";

    [SerializeField] Button equipButton;
    [SerializeField] Button unequipButton;
    [SerializeField] RectTransform panelRoot;

    Item _item;
    SyntyWeaponItemData _weapon;
    HeroWeaponVisual _visual;
    int _openedFrame = -1;

    static UIEquipPanel _instance;

    /// <summary>True while the equip popup should swallow inventory mouse picks.</summary>
    public static bool BlocksInventoryInput
    {
        get
        {
            if (!_instance || !_instance.gameObject.activeInHierarchy)
                return false;

            return _instance.IsPointerOverPanel();
        }
    }

    public static UIEquipPanel FindOrCreate()
    {
        if (_instance)
            return EnsureReady(_instance);

        _instance = Object.FindFirstObjectByType<UIEquipPanel>(FindObjectsInactive.Include);
        if (_instance)
            return EnsureReady(_instance);

        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != "UIEquip" || !t.gameObject.scene.IsValid())
                continue;

            _instance = t.GetComponent<UIEquipPanel>() ?? t.gameObject.AddComponent<UIEquipPanel>();
            return EnsureReady(_instance);
        }

        var canvas = FindOverlayCanvas();
        if (!canvas)
        {
            Debug.LogError("[UIEquipPanel] 找不到 Canvas，无法创建装备面板。");
            return null;
        }

        var prefab = LoadEquipPrefab();
        if (prefab)
        {
            var go = Object.Instantiate(prefab, canvas.transform, false);
            go.name = "UIEquip";
            _instance = go.GetComponent<UIEquipPanel>() ?? go.AddComponent<UIEquipPanel>();
            return EnsureReady(_instance);
        }

        // Runtime fallback when prefab cannot be loaded (non-Editor builds without Resources copy).
        var fallback = new GameObject("UIEquip", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fallback.transform.SetParent(canvas.transform, false);
        var rt = fallback.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(140f, 100f);
        fallback.GetComponent<Image>().color = new Color(0.15f, 0.12f, 0.1f, 0.92f);

        _instance = fallback.AddComponent<UIEquipPanel>();
        _instance.BuildFallbackButtons(fallback.transform);
        Debug.LogWarning("[UIEquipPanel] 使用临时装备面板（未加载到 UIEquip 预制体）。");
        return EnsureReady(_instance);
    }

    static GameObject LoadEquipPrefab()
    {
        var fromResources = Resources.Load<GameObject>("Prefabs/UI/UIEquip");
        if (fromResources)
            return fromResources;

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
#else
        return null;
#endif
    }

    static UIEquipPanel EnsureReady(UIEquipPanel panel)
    {
        if (!panel)
            return null;

        panel.BindButtons();
        return panel;
    }

    static Canvas FindOverlayCanvas()
    {
        Canvas best = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!c.isActiveAndEnabled)
                continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
            best ??= c;
        }

        return best ?? Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
    }

    void BuildFallbackButtons(Transform root)
    {
        equipButton = CreateFallbackButton(root, "btn_equip", "装备", new Vector2(0f, 22f));
        unequipButton = CreateFallbackButton(root, "btn_unEquip", "卸下", new Vector2(0f, -22f));
        panelRoot = root as RectTransform;
    }

    static Button CreateFallbackButton(Transform parent, string name, string label, Vector2 anchoredPos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(120f, 36f);
        rt.anchoredPosition = anchoredPos;
        go.GetComponent<Image>().color = new Color(0.45f, 0.35f, 0.25f, 1f);

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var text = textGo.GetComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.raycastTarget = false;

        return go.GetComponent<Button>();
    }

    void Awake()
    {
        _instance = this;
        if (!panelRoot)
            panelRoot = transform as RectTransform;

        BindButtons();
    }

    void Start()
    {
        // Start inactive; Open() will show and reparent to active canvas.
        if (_item == null)
            gameObject.SetActive(false);
    }

    void OnEnable()
    {
        HeroWeaponVisual.EquipChanged += RefreshButtonStates;
    }

    void OnDisable()
    {
        HeroWeaponVisual.EquipChanged -= RefreshButtonStates;
    }

    void BindButtons()
    {
        if (!equipButton)
        {
            var t = transform.Find("button/btn_equip")
                ?? transform.Find("btn_equip")
                ?? FindDeepChild(transform, "btn_equip");
            if (t)
                equipButton = t.GetComponent<Button>();
        }

        if (!unequipButton)
        {
            var t = transform.Find("button/btn_unEquip")
                ?? transform.Find("btn_unEquip")
                ?? FindDeepChild(transform, "btn_unEquip");
            if (t)
                unequipButton = t.GetComponent<Button>();
        }

        if (equipButton)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnEquipClicked);
        }

        if (unequipButton)
        {
            unequipButton.onClick.RemoveAllListeners();
            unequipButton.onClick.AddListener(OnUnequipClicked);
        }
    }

    static Transform FindDeepChild(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name)
                return t;
        }

        return null;
    }

    void Update()
    {
        if (!gameObject.activeSelf)
            return;

        // Skip the open frame + next so right-click / left-down don't instantly close.
        if (Time.frameCount <= _openedFrame + 1)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
            return;
        }

        if (Input.GetMouseButtonDown(0) && !IsPointerOverPanel())
            Close();
    }

    bool IsPointerOverPanel()
    {
        if (!panelRoot)
            return false;

        var canvas = GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        if (RectTransformUtility.RectangleContainsScreenPoint(panelRoot, Input.mousePosition, cam))
            return true;

        // Buttons can extend past the root rect.
        foreach (var rt in GetComponentsInChildren<RectTransform>(true))
        {
            if (rt == panelRoot)
                continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam))
                return true;
        }

        return false;
    }

    /// <summary>
    /// UIEquip may live under inactive UIStatePanel — reparent to an active canvas so it can show.
    /// </summary>
    void EnsureOnActiveCanvas()
    {
        var canvas = FindOverlayCanvas();
        if (!canvas)
            return;

        if (transform.parent != canvas.transform)
            transform.SetParent(canvas.transform, false);

        transform.SetAsLastSibling();
        transform.localScale = Vector3.one;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    /// <summary>
    /// Inventory picks items with raw Input + grid math — NOT EventSystem.
    /// Image.raycastTarget only affects GraphicRaycaster; it cannot stop inventory by itself.
    /// We block via <see cref="BlocksInventoryInput"/> geometry, and keep a full-rect Graphic
    /// so EventSystem also hits the panel (and does not fall through to other UI).
    /// </summary>
    void EnsurePanelRaycastBlocker()
    {
        if (!panelRoot)
            panelRoot = transform as RectTransform;

        // Prefab root is 100×220 but buttons are 180 wide — expand so hit tests cover buttons.
        if (panelRoot.sizeDelta.x < 180f || panelRoot.sizeDelta.y < 160f)
            panelRoot.sizeDelta = new Vector2(
                Mathf.Max(panelRoot.sizeDelta.x, 180f),
                Mathf.Max(panelRoot.sizeDelta.y, 160f));

        var blocker = transform.Find("ClickBlocker") as RectTransform;
        if (!blocker)
        {
            var go = new GameObject("ClickBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            blocker = go.GetComponent<RectTransform>();
            blocker.SetParent(transform, false);
            blocker.SetAsFirstSibling();
        }

        // Stretch to full panel — child "Image" is only 148×144 and cannot cover buttons.
        blocker.anchorMin = Vector2.zero;
        blocker.anchorMax = Vector2.one;
        blocker.offsetMin = Vector2.zero;
        blocker.offsetMax = Vector2.zero;
        blocker.localScale = Vector3.one;

        var image = blocker.GetComponent<Image>();
        if (!image)
            image = blocker.gameObject.AddComponent<Image>();

        // Fully transparent — raycast still works with raycastTarget; do not cull zero-alpha mesh.
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = true;
        var canvasRenderer = blocker.GetComponent<CanvasRenderer>();
        if (canvasRenderer)
            canvasRenderer.cullTransparentMesh = false;

        // Decorative child Image is smaller; keep it from stealing layout, but not required for block.
        var decor = transform.Find("Image");
        if (decor)
        {
            var decorImage = decor.GetComponent<Image>();
            if (decorImage)
                decorImage.raycastTarget = false;
        }

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            if (!btn)
                continue;
            var g = btn.targetGraphic;
            if (g)
                g.raycastTarget = true;
        }
    }

    public void Open(Item item, HeroWeaponVisual visual)
    {
        if (!item || item.data is not SyntyWeaponItemData weapon)
        {
            Debug.LogWarning($"[UIEquipPanel] Open 失败：需要 SyntyWeaponItemData，当前={item?.data?.GetType().Name ?? "null"}");
            return;
        }

        if (!visual)
        {
            Debug.LogWarning("[UIEquipPanel] Open 失败：没有 HeroWeaponVisual。");
            return;
        }

        BindButtons();
        _item = item;
        _weapon = weapon;
        _visual = visual;

        EnsureOnActiveCanvas();
        EnsurePanelRaycastBlocker();
        gameObject.SetActive(true);

        // Activate inactive button parents (e.g. under collapsed groups).
        if (equipButton && !equipButton.gameObject.activeInHierarchy)
            ActivateAncestorsUntil(equipButton.transform, transform);
        if (unequipButton && !unequipButton.gameObject.activeInHierarchy)
            ActivateAncestorsUntil(unequipButton.transform, transform);

        _openedFrame = Time.frameCount;
        PositionNearMouse();
        RefreshButtonStates();
        Debug.Log($"[UIEquipPanel] 已打开装备面板: {weapon.itemName}", this);
    }

    static void ActivateAncestorsUntil(Transform start, Transform stopInclusive)
    {
        var t = start;
        while (t)
        {
            if (!t.gameObject.activeSelf)
                t.gameObject.SetActive(true);
            if (t == stopInclusive)
                break;
            t = t.parent;
        }
    }

    public void Close()
    {
        _item = null;
        _weapon = null;
        _visual = null;
        gameObject.SetActive(false);
    }

    void PositionNearMouse()
    {
        if (!panelRoot)
            return;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out var local);
            panelRoot.localPosition = local + new Vector2(12f, -12f);
            return;
        }

        panelRoot.position = (Vector2)Input.mousePosition + new Vector2(12f, -12f);
    }

    void RefreshButtonStates()
    {
        if (!_weapon || !_visual)
            return;

        var equipped = WeaponEquipService.IsEquipped(_visual, _weapon);
        if (equipButton)
        {
            equipButton.gameObject.SetActive(true);
            equipButton.interactable = !equipped;
        }

        if (unequipButton)
        {
            // Always show; only disable click when not currently equipped.
            unequipButton.gameObject.SetActive(true);
            unequipButton.interactable = true;
        }

        ItemVisualLayout.RefreshEquippedMarkersInOpenInventories(_visual);
    }

    void OnEquipClicked()
    {
        if (!_weapon || !_visual)
            return;

        var oldRight = _visual.equippedRight;
        var oldLeft = _visual.equippedLeft;

        if (!WeaponEquipService.TrySmartEquip(_visual, _weapon, out var reason))
        {
            Debug.LogWarning($"[UIEquip] 装备失败: {reason}");
            return;
        }

        PersistEquip();
        ApplyEquipVisualsAfterChange(oldRight, oldLeft, _visual.equippedRight, _visual.equippedLeft);
        ItemVisualLayout.RefreshEquippedMarkersInOpenInventories(_visual);
        Close();
    }

    void OnUnequipClicked()
    {
        if (!_weapon || !_visual)
            return;

        var oldRight = _visual.equippedRight;
        var oldLeft = _visual.equippedLeft;

        if (!WeaponEquipService.TrySmartUnequip(_visual, _weapon, out var reason))
        {
            Debug.LogWarning($"[UIEquip] 卸下失败: {reason}");
            return;
        }

        PersistEquip();
        ApplyEquipVisualsAfterChange(oldRight, oldLeft, _visual.equippedRight, _visual.equippedLeft);
        ItemVisualLayout.RefreshEquippedMarkersInOpenInventories(_visual);
        Close();
    }

    void ApplyEquipVisualsAfterChange(
        SyntyWeaponItemData oldRight,
        SyntyWeaponItemData oldLeft,
        SyntyWeaponItemData newRight,
        SyntyWeaponItemData newLeft)
    {
        if (!_visual)
            return;

        var stance = _visual.GetComponent<PlayerStanceController>();
        if (stance && stance.CurrentStance == PlayerStanceController.StanceMode.Combat)
        {
            // Do not Sync mid-swap — coroutine owns weapon parenting + sheath/unsheath.
            stance.BeginDrawnWeaponSwap(oldRight, oldLeft, newRight, newLeft);
            return;
        }

        RefreshHeroWeaponVisuals();
        _visual.PlaceWeaponsForCasualStance();
    }

    void PersistEquip()
    {
        if (!_item || !_item.inventory)
            return;

        var role = _item.inventory.GetComponent<UIRolePanelController>();
        role?.PersistBoundEquip();
    }

    void RefreshHeroWeaponVisuals()
    {
        if (_item && _item.inventory)
        {
            var role = _item.inventory.GetComponent<UIRolePanelController>();
            var hero = role ? role.ResolveBoundHero() : null;
            if (hero)
                WeaponInventoryBridge.ApplyInventoryToHero(_item.inventory, hero);
        }
    }
}
