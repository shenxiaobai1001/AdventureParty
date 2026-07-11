using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponIconStudioController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] Camera previewCamera;
    [SerializeField] Transform previewStage;
    [SerializeField] Material partMaterial;

    [Header("UI")]
    [SerializeField] TMP_Dropdown entryDropdown;
    [SerializeField] Button previousButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button renderCurrentButton;
    [SerializeField] Button renderAllButton;
    [SerializeField] TextMeshProUGUI infoLabel;
    [SerializeField] TextMeshProUGUI hintLabel;

    [Header("Input")]
    [SerializeField] float rotationSpeed = 0.35f;

    readonly List<WeaponIconRenderEntry> entries = new List<WeaponIconRenderEntry>();

    GameObject currentWeapon;
    int currentIndex;
    bool isDragging;
    Vector2 lastPointerPosition;
    Coroutine batchRoutine;

    void Awake()
    {
        EnsureSceneReferences();
        BindUi();
    }

    void Start()
    {
        WeaponItemData.Instance.EnsureLoaded();
        ReloadCatalog();

        if (hintLabel)
        {
            hintLabel.text =
                "左键拖拽旋转 | A/D 切换 | F5 渲染当前 | F6 全部批量\n" +
                "弓/盾竖直渲染，其余武器横向渲染；输出尺寸与武器栏格子一致";
        }
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleRotationInput();
        UpdatePreviewCamera();
    }

    void EnsureSceneReferences()
    {
        if (!previewCamera)
            previewCamera = Camera.main;

        if (!previewStage)
        {
            var existing = GameObject.Find("WeaponIconPreviewStage");
            previewStage = existing ? existing.transform : new GameObject("WeaponIconPreviewStage").transform;
        }

#if UNITY_EDITOR
        if (!partMaterial)
        {
            partMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/Synty/PolygonFantasyHeroCharacters/Materials/FantasyHero.mat");
        }
#endif
    }

    void BindUi()
    {
        if (entryDropdown)
        {
            entryDropdown.onValueChanged.RemoveAllListeners();
            entryDropdown.onValueChanged.AddListener(OnDropdownChanged);
        }

        if (previousButton)
        {
            previousButton.onClick.RemoveAllListeners();
            previousButton.onClick.AddListener(ShowPrevious);
        }

        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(ShowNext);
        }

        if (renderCurrentButton)
        {
            renderCurrentButton.onClick.RemoveAllListeners();
            renderCurrentButton.onClick.AddListener(RenderCurrent);
        }

        if (renderAllButton)
        {
            renderAllButton.onClick.RemoveAllListeners();
            renderAllButton.onClick.AddListener(StartBatchRender);
        }
    }

    void ReloadCatalog()
    {
        entries.Clear();
        entries.AddRange(WeaponIconRenderCatalog.BuildFromWeaponItems());
        currentIndex = Mathf.Clamp(currentIndex, 0, Mathf.Max(entries.Count - 1, 0));
        RefreshDropdown();
        ShowCurrentEntry();
    }

    void RefreshDropdown()
    {
        if (!entryDropdown)
            return;

        entryDropdown.ClearOptions();
        var options = new List<TMP_Dropdown.OptionData>(entries.Count);
        foreach (var entry in entries)
            options.Add(new TMP_Dropdown.OptionData(entry.DisplayLabel));

        entryDropdown.AddOptions(options);
        entryDropdown.SetValueWithoutNotify(currentIndex);
        entryDropdown.RefreshShownValue();
    }

    void OnDropdownChanged(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, entries.Count - 1);
        ShowCurrentEntry();
    }

    void ShowPrevious()
    {
        if (entries.Count == 0)
            return;

        currentIndex = (currentIndex - 1 + entries.Count) % entries.Count;
        SyncDropdown();
        ShowCurrentEntry();
    }

    void ShowNext()
    {
        if (entries.Count == 0)
            return;

        currentIndex = (currentIndex + 1) % entries.Count;
        SyncDropdown();
        ShowCurrentEntry();
    }

    void SyncDropdown()
    {
        if (entryDropdown)
            entryDropdown.SetValueWithoutNotify(currentIndex);
    }

    void ShowCurrentEntry()
    {
        ClearWeapon();

        if (entries.Count == 0)
        {
            UpdateInfoLabel(null);
            return;
        }

        var entry = entries[currentIndex];
        previewStage.localRotation = Quaternion.identity;
        currentWeapon = WeaponIconStudioPresenter.Present(entry, previewStage, partMaterial);
        UpdateInfoLabel(entry);
    }

    void ClearWeapon()
    {
        if (!currentWeapon)
            return;

        if (Application.isPlaying)
            Destroy(currentWeapon);
        else
            DestroyImmediate(currentWeapon);

        currentWeapon = null;
    }

    void UpdateInfoLabel(WeaponIconRenderEntry entry)
    {
        if (!infoLabel)
            return;

        if (entry == null)
        {
            infoLabel.text = "未找到可渲染武器，请先运行 Game/Weapon/1 生成 WeaponItems.csv。";
            return;
        }

        var pixelSize = WeaponIconStudioSettings.GetOutputPixelSize(entry.gridSize);
        infoLabel.text =
            $"{entry.DisplayLabel}\n" +
            $"类型: {entry.category} | 渲染: {(entry.renderVertical ? "竖直" : "横向")}\n" +
            $"输出: {entry.FileName} ({entry.gridSize.x}x{entry.gridSize.y} 格, {pixelSize.x}x{pixelSize.y}px)";
    }

    void UpdatePreviewCamera()
    {
        if (!previewCamera || entries.Count == 0 || !previewStage)
            return;

        WeaponIconStudioCapture.FitCameraForPreview(previewCamera, previewStage, entries[currentIndex]);
    }

    void HandleKeyboardInput()
    {
        if (entries.Count == 0)
            return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            ShowPrevious();

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            ShowNext();

        if (Input.GetKeyDown(KeyCode.F5))
            RenderCurrent();

        if (Input.GetKeyDown(KeyCode.F6))
            StartBatchRender();
    }

    void HandleRotationInput()
    {
        if (!previewStage)
            return;

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUi())
        {
            isDragging = true;
            lastPointerPosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (!isDragging || !Input.GetMouseButton(0))
            return;

        var delta = (Vector2)Input.mousePosition - lastPointerPosition;
        lastPointerPosition = Input.mousePosition;

        previewStage.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
        previewStage.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
    }

    static bool IsPointerOverUi()
    {
        if (!EventSystem.current)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    void RenderCurrent()
    {
        if (entries.Count == 0)
            return;

        var entry = entries[currentIndex];
        if (WeaponIconStudioCapture.CaptureToPng(previewCamera, previewStage, entry, out var path))
            Debug.Log($"[WeaponIconStudio] Rendered current icon: {path}");
    }

    public void StartBatchRenderAll()
    {
        StartBatchRender();
    }

    void StartBatchRender()
    {
        if (batchRoutine != null)
            StopCoroutine(batchRoutine);

        batchRoutine = StartCoroutine(BatchRenderRoutine());
    }

    IEnumerator BatchRenderRoutine()
    {
        if (entries.Count == 0)
            yield break;

        var originalIndex = currentIndex;
        var successCount = 0;

        for (var i = 0; i < entries.Count; i++)
        {
            currentIndex = i;
            SyncDropdown();
            ShowCurrentEntry();
            yield return null;

            if (WeaponIconStudioCapture.CaptureToPng(previewCamera, previewStage, entries[i], out _))
                successCount++;
        }

        currentIndex = originalIndex;
        SyncDropdown();
        ShowCurrentEntry();

        Debug.Log($"[WeaponIconStudio] Batch render finished: {successCount}/{entries.Count}");
        batchRoutine = null;
    }
}
