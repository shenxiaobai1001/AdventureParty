using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Role state panel: body attributes, fight attributes, weapon arts; XP progress on select.
/// PropertyPanel/Content/{Power,Tough,Flexible,Accurate}
/// CombatPanel/Content/{ATK,Defense,Perception}
/// WeaponPanel/Content/{Greatsword,Heavyweapons,...,MartialArts|Dagger,Gunpowder,Throw}
/// </summary>
public class UIStatePanel : MonoBehaviour
{
    static readonly Dictionary<string, BodyAttributeType> AttributeNodeMap = new Dictionary<string, BodyAttributeType>(StringComparer.OrdinalIgnoreCase)
    {
        { "Power", BodyAttributeType.Strength },
        { "Tough", BodyAttributeType.Toughness },
        { "Flexible", BodyAttributeType.Agility },
        { "Accurate", BodyAttributeType.Precision },
    };

    static readonly Dictionary<string, FightAttributeType> FightNodeMap = new Dictionary<string, FightAttributeType>(StringComparer.OrdinalIgnoreCase)
    {
        { "ATK", FightAttributeType.Offense },
        { "Defense", FightAttributeType.Defense },
        { "Perception", FightAttributeType.Awareness },
    };

    static readonly Dictionary<string, WeaponProficiencyType> WeaponNodeMap = new Dictionary<string, WeaponProficiencyType>(StringComparer.OrdinalIgnoreCase)
    {
        { "Greatsword", WeaponProficiencyType.GreatSword },
        { "Heavyweapons", WeaponProficiencyType.HeavyWeapon },
        { "LongHandled", WeaponProficiencyType.Polearm },
        { "Bow", WeaponProficiencyType.BowCrossbow },
        { "Shield", WeaponProficiencyType.Shield },
        { "Sword", WeaponProficiencyType.Longsword },
        { "HammerAxe", WeaponProficiencyType.HammerAxe },
        { "MartialArts", WeaponProficiencyType.MartialArts },
        { "Martial", WeaponProficiencyType.MartialArts },
        { "Wushu", WeaponProficiencyType.MartialArts },
        // Legacy panel node name — remap if the prefab row is still called Dagger.
        { "Dagger", WeaponProficiencyType.MartialArts },
        { "Gunpowder", WeaponProficiencyType.Firearm },
        { "Throw", WeaponProficiencyType.Throwing },
    };

    [Header("Optional Overrides")]
    [SerializeField] Transform propertyContent;
    [SerializeField] Transform combatContent;
    [SerializeField] Transform weaponContent;
    [SerializeField] Slider xpSlider;
    [SerializeField] TextMeshProUGUI xpLabel;
    [SerializeField] Button closeButton;

    readonly List<StateRowView> attributeRows = new List<StateRowView>();
    readonly List<StateRowView> fightRows = new List<StateRowView>();
    readonly List<StateRowView> weaponRows = new List<StateRowView>();

    CharacterEntry boundEntry;
    bool isOpen;
    SelectedKind selectedKind = SelectedKind.None;
    BodyAttributeType selectedAttribute;
    FightAttributeType selectedFightAttribute;
    WeaponProficiencyType selectedWeapon;

    enum SelectedKind
    {
        None,
        Attribute,
        FightAttribute,
        Weapon,
    }

    sealed class StateRowView
    {
        public Transform Root;
        public Button Button;
        public TextMeshProUGUI LevelLabel;
        public BodyAttributeType? Attribute;
        public FightAttributeType? FightAttribute;
        public WeaponProficiencyType? Weapon;
    }

    void Awake()
    {
        BindReferences();
        BindRows();
        BindButtons();
        gameObject.SetActive(false);
    }

    void BindReferences()
    {
        if (!propertyContent)
            propertyContent = FindChildByPath("Center/PropertyPanel/Content")
                ?? FindChildByName("PropertyPanel")?.Find("Content");

        if (!combatContent)
            combatContent = FindChildByPath("Center/CombatPanel/Content")
                ?? FindChildByName("CombatPanel")?.Find("Content");

        if (!weaponContent)
            weaponContent = FindChildByPath("Center/WeaponPanel/Content")
                ?? FindChildByName("WeaponPanel")?.Find("Content");

        if (!xpSlider)
        {
            var xpBar = FindChildByPath("Center/SchedulePanel/XPBar") ?? FindChildByName("XPBar");
            if (xpBar)
                xpSlider = xpBar.GetComponentInChildren<Slider>(true);
        }

        if (!xpLabel)
            xpLabel = FindNamedText("tx_xp");

        if (!closeButton)
        {
            var close = FindChildByPath("Center/btn_close") ?? FindChildByName("btn_close");
            if (close)
                closeButton = close.GetComponent<Button>();
        }
    }

    void BindRows()
    {
        attributeRows.Clear();
        fightRows.Clear();
        weaponRows.Clear();

        if (propertyContent)
        {
            foreach (Transform child in propertyContent)
            {
                if (!AttributeNodeMap.TryGetValue(child.name, out var attr))
                    continue;

                attributeRows.Add(CreateRow(child, attr, null, null));
            }
        }

        if (combatContent)
        {
            foreach (Transform child in combatContent)
            {
                if (!FightNodeMap.TryGetValue(child.name, out var fight))
                    continue;

                fightRows.Add(CreateRow(child, null, fight, null));
            }
        }

        if (weaponContent)
        {
            foreach (Transform child in weaponContent)
            {
                if (!WeaponNodeMap.TryGetValue(child.name, out var weapon))
                    continue;

                weaponRows.Add(CreateRow(child, null, null, weapon));
            }
        }
    }

    StateRowView CreateRow(
        Transform root,
        BodyAttributeType? attribute,
        FightAttributeType? fightAttribute,
        WeaponProficiencyType? weapon)
    {
        var row = new StateRowView
        {
            Root = root,
            Attribute = attribute,
            FightAttribute = fightAttribute,
            Weapon = weapon,
            Button = root.GetComponentInChildren<Button>(true),
            LevelLabel = FindDescLabel(root),
        };

        if (!row.Button)
        {
            row.Button = root.gameObject.AddComponent<Button>();
            var graphic = root.GetComponentInChildren<Graphic>(true);
            if (graphic)
                row.Button.targetGraphic = graphic;
        }

        row.Button.onClick.RemoveAllListeners();
        if (attribute.HasValue)
        {
            var captured = attribute.Value;
            row.Button.onClick.AddListener(() => SelectAttribute(captured));
        }
        else if (fightAttribute.HasValue)
        {
            var captured = fightAttribute.Value;
            row.Button.onClick.AddListener(() => SelectFightAttribute(captured));
        }
        else if (weapon.HasValue)
        {
            var captured = weapon.Value;
            row.Button.onClick.AddListener(() => SelectWeapon(captured));
        }

        return row;
    }

    static TextMeshProUGUI FindDescLabel(Transform root)
    {
        var desc = root.Find("desc");
        if (desc)
            return desc.GetComponent<TextMeshProUGUI>();

        foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "desc")
                return text;
        }

        return null;
    }

    void BindButtons()
    {
        if (!closeButton)
            return;

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close);
    }

    public void Open(CharacterEntry entry)
    {
        if (entry == null)
            return;

        if (isOpen && boundEntry == entry)
        {
            RefreshAll();
            return;
        }

        boundEntry = entry;
        boundEntry.EnsureCombatDefaults();
        isOpen = true;
        gameObject.SetActive(true);

        if (selectedKind == SelectedKind.None && attributeRows.Count > 0 && attributeRows[0].Attribute.HasValue)
            SelectAttribute(attributeRows[0].Attribute.Value);
        else
            RefreshAll();
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        boundEntry = null;
        gameObject.SetActive(false);
    }

    void SelectAttribute(BodyAttributeType type)
    {
        selectedKind = SelectedKind.Attribute;
        selectedAttribute = type;
        RefreshAll();
    }

    void SelectFightAttribute(FightAttributeType type)
    {
        selectedKind = SelectedKind.FightAttribute;
        selectedFightAttribute = type;
        RefreshAll();
    }

    void SelectWeapon(WeaponProficiencyType type)
    {
        selectedKind = SelectedKind.Weapon;
        selectedWeapon = type;
        RefreshAll();
    }

    void RefreshAll()
    {
        if (boundEntry == null)
            return;

        boundEntry.EnsureCombatDefaults();
        var profile = boundEntry.combatProficiency;

        foreach (var row in attributeRows)
        {
            if (!row.Attribute.HasValue)
                continue;

            SetLevelLabel(row.LevelLabel, profile.GetAttributeLevel(row.Attribute.Value));
        }

        foreach (var row in fightRows)
        {
            if (!row.FightAttribute.HasValue)
                continue;

            SetLevelLabel(row.LevelLabel, profile.GetFightAttributeLevel(row.FightAttribute.Value));
        }

        foreach (var row in weaponRows)
        {
            if (!row.Weapon.HasValue)
                continue;

            SetLevelLabel(row.LevelLabel, profile.GetWeaponLevel(row.Weapon.Value));
        }

        RefreshXpBar(profile);
    }

    void RefreshXpBar(CombatProficiencyProfile profile)
    {
        float xpPerLevel = ProficiencyProgression.DefaultXpPerLevel;
        ProficiencyValue value = ProficiencyValue.Default;

        switch (selectedKind)
        {
            case SelectedKind.Attribute:
            {
                var entry = profile.GetOrCreateAttribute(selectedAttribute);
                value = entry.value;
                xpPerLevel = CombatProficiencyRuntime.GetAttributeXpPerLevel(selectedAttribute);
                break;
            }
            case SelectedKind.FightAttribute:
            {
                var entry = profile.GetOrCreateFightAttribute(selectedFightAttribute);
                value = entry.value;
                xpPerLevel = CombatProficiencyRuntime.GetFightAttributeXpPerLevel(selectedFightAttribute);
                break;
            }
            case SelectedKind.Weapon:
            {
                var entry = profile.GetOrCreateWeapon(selectedWeapon);
                value = entry.value;
                xpPerLevel = CombatProficiencyRuntime.GetWeaponXpPerLevel(selectedWeapon);
                break;
            }
        }

        var progress = ProficiencyProgression.GetProgress01(value, xpPerLevel);

        if (xpSlider)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.SetValueWithoutNotify(progress);
        }

        if (xpLabel)
            xpLabel.text = $"{Mathf.RoundToInt(progress * 100f)}%";
    }

    static void SetLevelLabel(TextMeshProUGUI label, float level)
    {
        if (!label)
            return;

        label.text = $"{Mathf.FloorToInt(level)}级";
    }

    Transform FindChildByPath(string path)
    {
        return transform.Find(path);
    }

    Transform FindChildByName(string objectName)
    {
        foreach (var child in GetComponentsInChildren<Transform>(true))
        {
            if (child.name == objectName)
                return child;
        }

        return null;
    }

    TextMeshProUGUI FindNamedText(string objectName)
    {
        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == objectName)
                return text;
        }

        return null;
    }
}
