using TMPro;
using UInventoryGrid;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Role equipment / carry panel backed by UInventoryGrid.
/// </summary>
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(InventoryController))]
[RequireComponent(typeof(EquipmentInventoryWatcher))]
[RequireComponent(typeof(WeaponInventoryWatcher))]
public class UIRolePanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI roleNameLabel;
    [SerializeField] TextMeshProUGUI weightLabel;
    [SerializeField] TextMeshProUGUI weightStateLabel;

    Inventory inventory;
    InventoryController inventoryController;
    EquipmentInventoryWatcher equipmentWatcher;
    WeaponInventoryWatcher weaponWatcher;
    CharacterEntry boundEntry;
    bool isOpen;

    void Awake()
    {
        inventory = GetComponent<Inventory>();
        inventoryController = GetComponent<InventoryController>();
        equipmentWatcher = GetComponent<EquipmentInventoryWatcher>();
        if (!equipmentWatcher)
            equipmentWatcher = gameObject.AddComponent<EquipmentInventoryWatcher>();

        weaponWatcher = GetComponent<WeaponInventoryWatcher>();
        if (!weaponWatcher)
            weaponWatcher = gameObject.AddComponent<WeaponInventoryWatcher>();

        BindUiReferences();
        EnsureRoleNameLabel();
        ConfigureInventoryGrids();
        BindButtons();

        gameObject.SetActive(false);
    }

    void BindUiReferences()
    {
        if (!roleNameLabel)
            roleNameLabel = FindText("tx_roleName");

        if (!weightLabel)
            weightLabel = FindText("tx_weiget");

        if (!weightStateLabel)
            weightStateLabel = FindText("tx_weigetState");
    }

    TextMeshProUGUI FindText(string objectName)
    {
        foreach (var text in GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == objectName)
                return text;
        }

        return null;
    }

    void EnsureRoleNameLabel()
    {
        if (roleNameLabel)
            return;

        var center = transform.Find("Center");
        if (!center)
            return;

        var weight = FindText("tx_weiget");
        var labelObject = new GameObject("tx_roleName", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(center, false);

        var rectTransform = labelObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 450f);
        rectTransform.sizeDelta = new Vector2(520f, 56f);

        roleNameLabel = labelObject.GetComponent<TextMeshProUGUI>();
        roleNameLabel.alignment = TextAlignmentOptions.Center;
        roleNameLabel.fontSize = 32f;

        if (weight)
        {
            roleNameLabel.font = weight.font;
            roleNameLabel.fontSharedMaterial = weight.fontSharedMaterial;
            roleNameLabel.color = weight.color;
        }
    }

    void ConfigureInventoryGrids()
    {
        if (!inventory)
            return;

        if (inventory.grids == null || inventory.grids.Length == 0)
            inventory.grids = inventory.GetComponentsInChildren<InventoryGrid>(true);

        foreach (var grid in inventory.grids)
        {
            if (!grid)
                continue;

            if (grid.items == null)
                grid.InitializeGrid();
        }

        RoleInventoryTypes.ConfigureAllGrids(inventory.grids);

        var normalBack = inventory.FindGridByName("NormalBack");
        if (normalBack)
            inventory.addItemGrids = new[] { normalBack };
    }

    public void EnsureInventoryReady()
    {
        ConfigureInventoryGrids();
    }

    void BindButtons()
    {
        BindButton("Center/btn_close", Close);
        BindButton("Center/Func/btn_backPack", OnBackpackButtonClicked);
    }

    void BindButton(string path, UnityEngine.Events.UnityAction callback)
    {
        var buttonTransform = transform.Find(path);
        if (!buttonTransform)
            return;

        var button = buttonTransform.GetComponent<Button>();
        if (!button)
            button = buttonTransform.gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(callback);
    }

    public void Open(CharacterEntry entry)
    {
        if (entry == null)
            return;

        if (isOpen && boundEntry == entry)
            return;

        if (isOpen)
            Close();

        boundEntry = entry;
        isOpen = true;
        gameObject.SetActive(true);

        RefreshHeader();
        RoleInventoryPersistence.Import(inventory, entry.inventory);
        equipmentWatcher?.RefreshBoundHero();
        weaponWatcher?.RefreshBoundHero();
        RefreshWeightDisplay();
    }

    public void Close()
    {
        if (!isOpen)
            return;

        if (boundEntry != null && inventory)
        {
            var exported = RoleInventoryPersistence.Export(inventory);
            exported.maxCarryWeight = boundEntry.inventory.maxCarryWeight;
            boundEntry.inventory = exported;
        }

        boundEntry = null;
        isOpen = false;
        inventory?.DeselectItem();
        gameObject.SetActive(false);
    }

    void RefreshHeader()
    {
        if (roleNameLabel && boundEntry != null)
            roleNameLabel.text = boundEntry.displayName;
    }

    void RefreshWeightDisplay()
    {
        if (!inventory || boundEntry == null)
            return;

        var totalWeight = RoleInventoryPersistence.GetLiveTotalWeight(inventory);
        var maxWeight = boundEntry.inventory.maxCarryWeight;

        if (weightLabel)
            weightLabel.text = $"总重量: {Mathf.CeilToInt(totalWeight)}/{Mathf.CeilToInt(maxWeight)}";

        if (weightStateLabel)
            weightStateLabel.text = boundEntry.inventory.GetWeightStateLabel(totalWeight);
    }

    void OnBackpackButtonClicked()
    {
        Debug.Log("[UIRolePanel] External backpack panel is not implemented yet.");
    }

    void LateUpdate()
    {
        if (!isOpen || !inventory)
            return;

        RefreshWeightDisplay();
    }

    public PlayerHeroEntity ResolveBoundHero()
    {
        if (boundEntry == null || !boundEntry.heroObject)
            return null;

        return boundEntry.heroObject.GetComponent<PlayerHeroEntity>();
    }
}
