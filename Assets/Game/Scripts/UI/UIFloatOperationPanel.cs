using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// World right-click context menu on characters.
/// Teammate → TeamButton (placeholder); NPC/Enemy → OtherButton with btn_Atk.
/// </summary>
public class UIFloatOperationPanel : MonoBehaviour
{
    const string PrefabAssetPath = "Assets/Game/Resources_moved/Prefabs/UI/UIFloatOperation.prefab";

    [SerializeField] RectTransform panelRoot;
    [SerializeField] GameObject teamButtonRoot;
    [SerializeField] GameObject otherButtonRoot;
    [SerializeField] Button attackButton;

    Transform _target;
    int _openedFrame = -1;

    static UIFloatOperationPanel _instance;

    public static bool BlocksWorldInput
    {
        get
        {
            if (!_instance || !_instance.gameObject.activeInHierarchy)
                return false;
            return _instance.IsPointerOverPanel();
        }
    }

    public static bool IsOpen =>
        _instance && _instance.gameObject.activeInHierarchy;

    public static UIFloatOperationPanel FindOrCreate()
    {
        if (_instance)
            return EnsureReady(_instance);

        _instance = Object.FindFirstObjectByType<UIFloatOperationPanel>(FindObjectsInactive.Include);
        if (_instance)
            return EnsureReady(_instance);

        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name != "UIFloatOperation" || !t.gameObject.scene.IsValid())
                continue;

            // Prefab was briefly mis-bound to UIEquipPanel — strip it if present.
            var wrong = t.GetComponent<UIEquipPanel>();
            if (wrong)
            {
                if (Application.isPlaying)
                    Object.Destroy(wrong);
                else
                    Object.DestroyImmediate(wrong);
            }

            _instance = t.GetComponent<UIFloatOperationPanel>() ?? t.gameObject.AddComponent<UIFloatOperationPanel>();
            return EnsureReady(_instance);
        }

        var canvas = FindOverlayCanvas();
        if (!canvas)
        {
            Debug.LogError("[UIFloatOperation] 找不到 Canvas。");
            return null;
        }

        var prefab = LoadPrefab();
        if (!prefab)
        {
            Debug.LogError("[UIFloatOperation] 找不到预制体。");
            return null;
        }

        var go = Object.Instantiate(prefab, canvas.transform, false);
        go.name = "UIFloatOperation";

        var wrongOnPrefab = go.GetComponent<UIEquipPanel>();
        if (wrongOnPrefab)
        {
            if (Application.isPlaying)
                Object.Destroy(wrongOnPrefab);
            else
                Object.DestroyImmediate(wrongOnPrefab);
        }

        _instance = go.GetComponent<UIFloatOperationPanel>() ?? go.AddComponent<UIFloatOperationPanel>();
        return EnsureReady(_instance);
    }

    static GameObject LoadPrefab()
    {
        var fromResources = Resources.Load<GameObject>("Prefabs/UI/UIFloatOperation");
        if (fromResources)
            return fromResources;

#if UNITY_EDITOR
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabAssetPath);
#else
        return null;
#endif
    }

    static UIFloatOperationPanel EnsureReady(UIFloatOperationPanel panel)
    {
        if (!panel)
            return null;

        _instance = panel;
        panel.ResolveRefs();
        panel.WireButtons();
        if (panel.gameObject.activeSelf && panel._target == null)
            panel.gameObject.SetActive(false);
        return panel;
    }

    static Canvas FindOverlayCanvas()
    {
        Canvas best = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!c || !c.isActiveAndEnabled)
                continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
            best ??= c;
        }

        return best;
    }

    void Awake()
    {
        _instance = this;
        ResolveRefs();
        WireButtons();
    }

    void OnEnable()
    {
        _instance = this;
    }

    void Update()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (Time.frameCount <= _openedFrame)
            return;

        // Only dismiss on press outside — never while pointer is over any child of this panel
        // (Center was offset past panelRoot, so root-rect checks closed the menu before Button.onClick).
        if (Input.GetMouseButtonDown(0) && !IsPointerOverPanel())
            Close();
        else if (Input.GetMouseButtonDown(1) && !IsPointerOverPanel())
            Close();
        else if (Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    void ResolveRefs()
    {
        if (!panelRoot)
            panelRoot = transform as RectTransform;

        if (!teamButtonRoot)
        {
            var t = transform.Find("Center/TeamButton") ?? transform.Find("TeamButton");
            if (t)
                teamButtonRoot = t.gameObject;
        }

        if (!otherButtonRoot)
        {
            var t = transform.Find("Center/OtherButton") ?? transform.Find("OtherButton");
            if (t)
                otherButtonRoot = t.gameObject;
        }

        // Prefer OtherButton/btn_Atk only — TeamButton also has a placeholder btn_Atk.
        attackButton = null;
        if (otherButtonRoot)
        {
            var atk = otherButtonRoot.transform.Find("btn_Atk")
                      ?? FindDeepChild(otherButtonRoot.transform, "btn_Atk");
            if (atk)
                attackButton = atk.GetComponent<Button>();
        }

        if (!attackButton)
        {
            var atk = transform.Find("Center/OtherButton/btn_Atk");
            if (atk)
                attackButton = atk.GetComponent<Button>();
        }

        // Buttons are 180 wide; keep root large enough and center content on the cursor.
        if (panelRoot)
        {
            panelRoot.sizeDelta = new Vector2(
                Mathf.Max(panelRoot.sizeDelta.x, 200f),
                Mathf.Max(panelRoot.sizeDelta.y, 160f));
        }

        var center = transform.Find("Center") as RectTransform;
        if (center)
            center.anchoredPosition = Vector2.zero;
    }

    void WireButtons()
    {
        if (!attackButton)
        {
            Debug.LogWarning("[UIFloatOperation] 未找到 OtherButton/btn_Atk。");
            return;
        }

        attackButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(OnAttackClicked);
    }

    public void Open(Transform target)
    {
        if (!target)
            return;

        ResolveRefs();
        WireButtons();

        _target = target;
        _openedFrame = Time.frameCount;

        var isTeammate = GameTags.IsTeammate(target.gameObject);
        if (teamButtonRoot)
            teamButtonRoot.SetActive(isTeammate);
        if (otherButtonRoot)
            otherButtonRoot.SetActive(!isTeammate);

        EnsureAttackButtonClickable();

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        PositionNearMouse();

        Debug.Log($"[UIFloatOperation] Open on '{target.name}' teammate={isTeammate} atkBtn={(attackButton ? attackButton.name : "null")}");
    }

    void EnsureAttackButtonClickable()
    {
        if (!attackButton)
            return;

        attackButton.gameObject.SetActive(true);
        attackButton.interactable = true;

        // Prefab had SPR_Background.raycastTarget=false — Button never received presses.
        foreach (var g in attackButton.GetComponentsInChildren<Graphic>(true))
        {
            if (!g)
                continue;
            // Keep text raycast on so clicks on label still hit hierarchy under the button.
            g.raycastTarget = true;
        }

        if (!attackButton.targetGraphic)
        {
            var img = attackButton.GetComponent<Image>()
                      ?? attackButton.GetComponentInChildren<Image>(true);
            if (img)
                attackButton.targetGraphic = img;
        }
    }

    public void Close()
    {
        _target = null;
        gameObject.SetActive(false);
    }

    void OnAttackClicked()
    {
        var attacker = PlayerController.Selected;
        if (!attacker)
        {
            // Fallback: first Teammate in scene (common when dual heroes skipped auto-select).
            attacker = FindPreferredTeammateAttacker();
            if (attacker)
                PlayerController.Select(attacker);
        }

        if (!attacker)
        {
            Debug.LogWarning("[UIFloatOperation] 没有选中的队友，请先左键点选己方角色。");
            return;
        }

        if (!_target)
        {
            Debug.LogWarning("[UIFloatOperation] 没有攻击目标。");
            return;
        }

        if (_target == attacker.transform || _target.IsChildOf(attacker.transform))
        {
            Debug.LogWarning("[UIFloatOperation] 不能攻击自己。");
            return;
        }

        var target = _target;
        Debug.Log($"[UIFloatOperation] Attack '{target.name}' by '{attacker.name}'");
        Close();
        CombatEngageService.BeginAttack(attacker, target);
    }

    static PlayerController FindPreferredTeammateAttacker()
    {
        var players = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        PlayerController best = null;
        for (var i = 0; i < players.Length; i++)
        {
            var p = players[i];
            if (!p || !GameTags.IsTeammate(p.gameObject))
                continue;
            best = p;
            break;
        }

        return best;
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

    bool IsPointerOverPanel()
    {
        // Prefer EventSystem hits on any descendant (buttons may sit outside panelRoot rect).
        if (EventSystem.current)
        {
            var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
            var results = new List<RaycastResult>(16);
            EventSystem.current.RaycastAll(ped, results);
            for (var i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (!go)
                    continue;
                if (go.transform == transform || go.transform.IsChildOf(transform))
                    return true;
            }
        }

        if (!panelRoot)
            return false;

        var canvas = GetComponentInParent<Canvas>();
        var cam = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        if (RectTransformUtility.RectangleContainsScreenPoint(panelRoot, Input.mousePosition, cam))
            return true;

        // Also accept Center / OtherButton rects (legacy layout offsets).
        if (otherButtonRoot)
        {
            var rt = otherButtonRoot.transform as RectTransform;
            if (rt && RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam))
                return true;
        }

        if (teamButtonRoot)
        {
            var rt = teamButtonRoot.transform as RectTransform;
            if (rt && RectTransformUtility.RectangleContainsScreenPoint(rt, Input.mousePosition, cam))
                return true;
        }

        return false;
    }

    static Transform FindDeepChild(Transform root, string name)
    {
        if (!root)
            return null;
        for (var i = 0; i < root.childCount; i++)
        {
            var c = root.GetChild(i);
            if (c.name == name)
                return c;
            var nested = FindDeepChild(c, name);
            if (nested)
                return nested;
        }

        return null;
    }
}
