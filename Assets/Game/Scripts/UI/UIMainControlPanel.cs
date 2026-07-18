using System.Collections.Generic;
using TMPro;
using UInventoryGrid;
using UnityEngine;
using UnityEngine.UI;

public class UIMainControlPanel : MonoBehaviour
{
    const string StatValueTextName = "tx_value";
    const string RoleHpTextName = "tx_hp";

    [Header("Role List")]
    [SerializeField] Transform roleListContent;
    [SerializeField] GameObject roleItemTemplate;

    [Header("State Bars")]
    [SerializeField] Slider hpSlider;
    [SerializeField] Slider armorSlider;
    [SerializeField] Slider energySlider;
    [SerializeField] Slider hungerSlider;

    [Header("Default Hero")]
    [SerializeField] GameObject playerHeroPrefab;

    [Header("Role Panel")]
    [SerializeField] UIRolePanelController rolePanel;
    [SerializeField] GameObject rolePanelPrefab;

    [Header("State Panel")]
    [SerializeField] UIStatePanel statePanel;
    [SerializeField] GameObject statePanelPrefab;

    readonly List<CharacterEntry> roleList = new List<CharacterEntry>();

    KenshiCameraController cameraController;
    TextMeshProUGUI hpValueLabel;
    TextMeshProUGUI armorValueLabel;
    TextMeshProUGUI energyValueLabel;
    TextMeshProUGUI hungerValueLabel;
    Image moveModeDialFill;
    Image sneakDialFill;
    Image restDialFill;
    int selectedRoleIndex;
    bool partyStealth;
    bool partyResting;
    PlayerController.CasualLocomotionMode partyLocomotion = PlayerController.CasualLocomotionMode.Walk;

    public IReadOnlyList<CharacterEntry> RoleList => roleList;

    public CharacterEntry GetSelectedCharacterEntry()
    {
        if (selectedRoleIndex < 0 || selectedRoleIndex >= roleList.Count)
            return null;

        return roleList[selectedRoleIndex];
    }

    public Inventory EnsureRoleInventory()
    {
        var panel = ResolveRolePanel();
        if (!panel)
            return null;

        panel.EnsureInventoryReady();
        return panel.GetComponent<Inventory>();
    }

    void Awake()
    {
        cameraController = FindFirstObjectByType<KenshiCameraController>();
        BindReferences();
        BindButtons();
        InitDefaultRoleList();
        ApplyPartyLocomotion();
        RefreshRoleList();
        RefreshStateBars();
    }

    void BindReferences()
    {
        if (!roleListContent)
        {
            var content = transform.Find("Center/Teammate/Scroll View/Viewport/Content");
            if (content)
                roleListContent = content;
        }

        if (!roleItemTemplate && roleListContent && roleListContent.childCount > 0)
            roleItemTemplate = roleListContent.GetChild(0).gameObject;

        var stateGroup = transform.Find("Center/State/StateGrounp");
        if (!stateGroup)
            stateGroup = transform.Find("Center/State/StateGroup");

        if (!stateGroup)
            return;

        DisableSampleScrollEffects(stateGroup);

        if (!hpSlider)
            hpSlider = FindStatSlider(stateGroup, "HP");
        if (!armorSlider)
            armorSlider = FindStatSlider(stateGroup, "Armor");
        if (!energySlider)
            energySlider = FindStatSlider(stateGroup, "Energy");
        if (!hungerSlider)
            hungerSlider = FindStatSlider(stateGroup, "Hunger");

        hpValueLabel = FindStatValueText(stateGroup, "HP");
        armorValueLabel = FindStatValueText(stateGroup, "Armor");
        energyValueLabel = FindStatValueText(stateGroup, "Energy");
        hungerValueLabel = FindStatValueText(stateGroup, "Hunger");
    }

    void BindButtons()
    {
        BindNavButtonByName("btn_role", OnRoleButtonClicked);
        BindNavButtonByName("btn_state", OnStateButtonClicked);
        BindNavButtonByName("btn_map", OnMapButtonClicked);
        BindNavButtonByName("btn_queue", OnQueueButtonClicked);
        BindNavButtonByName("btn_work", OnWorkButtonClicked);

        BindToggleButton("Center/btn_moveMode", TogglePartyLocomotion, out moveModeDialFill, RefreshMoveModeVisual);
        BindToggleButton("Center/btn_sneak", TogglePartyStealth, out sneakDialFill, RefreshStealthVisual);
        BindToggleButton("Center/btn_rest", TogglePartyRest, out restDialFill, RefreshRestVisual);
    }

    void BindToggleButton(string path, UnityEngine.Events.UnityAction callback, out Image dialFill, UnityEngine.Events.UnityAction refreshVisual)
    {
        dialFill = null;

        var buttonTransform = transform.Find(path);
        if (!buttonTransform)
            return;

        var dialFillTransform = buttonTransform.Find("Dial Fill");
        if (dialFillTransform)
            dialFill = dialFillTransform.GetComponent<Image>();

        var button = buttonTransform.GetComponent<Button>();
        if (!button)
            button = buttonTransform.gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
        refreshVisual?.Invoke();
    }

    static void BindNavButton(Transform root, string path, UnityEngine.Events.UnityAction callback)
    {
        var buttonTransform = root.Find(path);
        if (!buttonTransform)
            return;

        var button = buttonTransform.GetComponent<Button>();
        if (!button)
            button = buttonTransform.gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }

    void BindNavButton(string path, UnityEngine.Events.UnityAction callback)
    {
        BindNavButton(transform, path, callback);
    }

    void BindNavButtonByName(string buttonName, UnityEngine.Events.UnityAction callback)
    {
        var buttonTransform = FindChildByName(transform, buttonName);
        if (!buttonTransform)
            return;

        var button = buttonTransform.GetComponent<Button>();
        if (!button)
            button = buttonTransform.gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }

    static Transform FindChildByName(Transform root, string objectName)
    {
        if (!root)
            return null;

        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
                return child;
        }

        return null;
    }

    UIRolePanelController ResolveRolePanel()
    {
        if (rolePanel)
            return rolePanel;

        rolePanel = FindFirstObjectByType<UIRolePanelController>(FindObjectsInactive.Include);
        if (rolePanel)
            return rolePanel;

        if (!rolePanelPrefab)
            return null;

        var canvasTransform = GetComponentInParent<Canvas>()?.transform;
        var instance = Instantiate(rolePanelPrefab, canvasTransform ? canvasTransform : transform.root);
        rolePanel = instance.GetComponent<UIRolePanelController>();
        return rolePanel;
    }

    void TogglePartyLocomotion()
    {
        partyLocomotion = partyLocomotion == PlayerController.CasualLocomotionMode.Walk
            ? PlayerController.CasualLocomotionMode.Run
            : PlayerController.CasualLocomotionMode.Walk;

        ApplyPartyLocomotion();
        RefreshMoveModeVisual();
    }

    void ApplyPartyLocomotion()
    {
        foreach (var entry in roleList)
        {
            if (!entry.heroObject)
                continue;

            var player = entry.heroObject.GetComponent<PlayerController>();
            player?.SetCasualLocomotion(partyLocomotion);
        }
    }

    void RefreshMoveModeVisual()
    {
        if (!moveModeDialFill)
            return;

        moveModeDialFill.fillAmount = partyLocomotion == PlayerController.CasualLocomotionMode.Run ? 1f : 0f;
    }

    void TogglePartyStealth()
    {
        partyStealth = !partyStealth;

        if (partyStealth)
            partyResting = false;

        ApplyPartyStealth();
        ApplyPartyRest();
        RefreshStealthVisual();
        RefreshRestVisual();
    }

    void ApplyPartyStealth()
    {
        if (!partyStealth)
        {
            foreach (var entry in roleList)
            {
                if (!entry.heroObject)
                    continue;

                entry.heroObject.GetComponent<PlayerActivityController>()?.SetStealth(false);
            }

            return;
        }

        var enteredAny = false;
        foreach (var entry in roleList)
        {
            if (!entry.heroObject)
                continue;

            var activity = entry.heroObject.GetComponent<PlayerActivityController>();
            if (activity != null && activity.SetStealth(true))
                enteredAny = true;
        }

        if (!enteredAny)
            partyStealth = false;
    }

    void RefreshStealthVisual()
    {
        if (!sneakDialFill)
            return;

        sneakDialFill.fillAmount = partyStealth ? 1f : 0f;
    }

    void TogglePartyRest()
    {
        partyResting = !partyResting;

        if (partyResting)
            partyStealth = false;

        ApplyPartyStealth();
        ApplyPartyRest();
        RefreshStealthVisual();
        RefreshRestVisual();
    }

    void ApplyPartyRest()
    {
        if (!partyResting)
        {
            foreach (var entry in roleList)
            {
                if (!entry.heroObject)
                    continue;

                entry.heroObject.GetComponent<PlayerActivityController>()?.SetRest(false);
            }

            return;
        }

        var enteredAny = false;
        foreach (var entry in roleList)
        {
            if (!entry.heroObject)
                continue;

            var activity = entry.heroObject.GetComponent<PlayerActivityController>();
            if (activity != null && activity.SetRest(true))
                enteredAny = true;
        }

        if (!enteredAny)
            partyResting = false;
    }

    void RefreshRestVisual()
    {
        if (!restDialFill)
            return;

        restDialFill.fillAmount = partyResting ? 1f : 0f;
    }

    void OnRoleButtonClicked()
    {
        if (roleList.Count == 0)
            return;

        selectedRoleIndex = Mathf.Clamp(selectedRoleIndex, 0, roleList.Count - 1);

        var panel = ResolveRolePanel();
        if (!panel)
        {
            Debug.LogWarning("[UIMainControlPanel] UIRolePanel not found. Assign rolePanel or rolePanelPrefab.");
            return;
        }

        panel.Open(roleList[selectedRoleIndex]);
    }

    void OnStateButtonClicked()
    {
        if (roleList.Count == 0)
            return;

        selectedRoleIndex = Mathf.Clamp(selectedRoleIndex, 0, roleList.Count - 1);

        var panel = ResolveStatePanel();
        if (!panel)
        {
            Debug.LogWarning("[UIMainControlPanel] UIStatePanel not found. Assign statePanel or statePanelPrefab.");
            return;
        }

        panel.Open(roleList[selectedRoleIndex]);
    }

    UIStatePanel ResolveStatePanel()
    {
        if (statePanel)
            return statePanel;

        statePanel = FindFirstObjectByType<UIStatePanel>(FindObjectsInactive.Include);
        if (statePanel)
            return statePanel;

        var existingRoot = FindSceneObjectByName("UIStatePanel");
        if (existingRoot)
        {
            statePanel = existingRoot.GetComponent<UIStatePanel>();
            if (!statePanel)
                statePanel = existingRoot.AddComponent<UIStatePanel>();
            return statePanel;
        }

        if (!statePanelPrefab)
            return null;

        var canvasTransform = GetComponentInParent<Canvas>()?.transform;
        var instance = Instantiate(statePanelPrefab, canvasTransform ? canvasTransform : transform.root);
        statePanel = instance.GetComponent<UIStatePanel>();
        if (!statePanel)
            statePanel = instance.AddComponent<UIStatePanel>();

        return statePanel;
    }

    static GameObject FindSceneObjectByName(string objectName)
    {
        var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in transforms)
        {
            if (t && t.name == objectName)
                return t.gameObject;
        }

        return null;
    }

    void OnMapButtonClicked() { }

    void OnQueueButtonClicked() { }

    void OnWorkButtonClicked() { }

    static Slider FindStatSlider(Transform stateGroup, string statName)
    {
        var stat = stateGroup.Find(statName);
        return stat ? stat.GetComponentInChildren<Slider>(true) : null;
    }

    static TextMeshProUGUI FindStatValueText(Transform stateGroup, string statName)
    {
        var stat = stateGroup.Find(statName);
        return stat ? FindNamedText(stat, StatValueTextName) : null;
    }

    static TextMeshProUGUI FindNamedText(Transform root, string objectName)
    {
        if (!root)
            return null;

        var direct = root.Find(objectName);
        if (direct)
            return direct.GetComponent<TextMeshProUGUI>();

        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
                return child.GetComponent<TextMeshProUGUI>();
        }

        return null;
    }

    static void DisableSampleScrollEffects(Transform root)
    {
        foreach (var behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour && behaviour.GetType().Name == "SampleScrollUV")
                behaviour.enabled = false;
        }
    }

    void InitDefaultRoleList()
    {
        if (roleList.Count > 0)
            return;

        var heroObject = FindSceneHero();
        var entry = new CharacterEntry
        {
            displayName = MainUiConst.DefaultRoleName,
            hp = MainUiConst.DefaultStatValue,
            armorDurability = MainUiConst.DefaultStatValue,
            energy = MainUiConst.DefaultStatValue,
            hunger = MainUiConst.DefaultStatValue,
            heroObject = heroObject
        };
        entry.EnsureCombatDefaults();
        roleList.Add(entry);
    }

    GameObject FindSceneHero()
    {
        var hero = FindFirstObjectByType<PlayerHeroEntity>();
        if (hero)
            return hero.gameObject;

        return playerHeroPrefab;
    }

    void RefreshRoleList()
    {
        if (!roleListContent || !roleItemTemplate)
            return;

        for (var i = roleListContent.childCount - 1; i >= 0; i--)
        {
            var child = roleListContent.GetChild(i).gameObject;
            if (child == roleItemTemplate)
                continue;

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }

        roleItemTemplate.SetActive(false);

        for (var i = 0; i < roleList.Count; i++)
        {
            var entry = roleList[i];
            var item = Instantiate(roleItemTemplate, roleListContent);
            item.SetActive(true);
            BindRoleItem(item, entry, i);
        }
    }

    void BindRoleItem(GameObject item, CharacterEntry entry, int roleIndex)
    {
        var hpText = FindNamedText(item.transform, RoleHpTextName);
        SetStatText(hpText, entry.hp, MainUiConst.DefaultStatValue);

        var hitPoints = item.transform.Find("HitPoints");
        if (hitPoints)
        {
            DisableSampleScrollEffects(hitPoints);
            var hitPointsSlider = hitPoints.GetComponentInChildren<Slider>(true);
            SetStatSlider(hitPointsSlider, entry.hp, MainUiConst.DefaultStatValue);
        }

        var portrait = item.transform.Find("HUD_PortraitContents");
        if (!portrait)
            return;

        var button = portrait.GetComponent<Button>();
        if (!button)
            button = portrait.gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => SelectRole(roleIndex));
    }

    void SelectRole(int roleIndex)
    {
        if (roleIndex < 0 || roleIndex >= roleList.Count)
            return;

        selectedRoleIndex = roleIndex;
        RefreshStateBars();

        var entry = roleList[roleIndex];
        if (!entry.heroObject || !cameraController)
            return;

        cameraController.SetFollowTarget(entry.heroObject.transform);
    }

    void RefreshStateBars()
    {
        if (roleList.Count == 0)
            return;

        selectedRoleIndex = Mathf.Clamp(selectedRoleIndex, 0, roleList.Count - 1);
        var entry = roleList[selectedRoleIndex];
        var maxValue = MainUiConst.DefaultStatValue;

        SetStatSlider(hpSlider, entry.hp, maxValue);
        SetStatSlider(armorSlider, entry.armorDurability, maxValue);
        SetStatSlider(energySlider, entry.energy, maxValue);
        SetStatSlider(hungerSlider, entry.hunger, maxValue);

        SetStatText(hpValueLabel, entry.hp, maxValue);
        SetStatText(armorValueLabel, entry.armorDurability, maxValue);
        SetStatText(energyValueLabel, entry.energy, maxValue);
        SetStatText(hungerValueLabel, entry.hunger, maxValue);
    }

    static void SetStatSlider(Slider slider, float current, float max)
    {
        if (!slider)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        var normalized = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        slider.SetValueWithoutNotify(normalized);
    }

    static void SetStatText(TextMeshProUGUI valueLabel, float current, float max)
    {
        if (!valueLabel)
            return;

        valueLabel.text = $"{Mathf.RoundToInt(current)}/{Mathf.RoundToInt(max)}";
    }
}
