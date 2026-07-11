using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class IconStudioController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] Camera previewCamera;
    [SerializeField] Transform previewStage;
    [SerializeField] Light keyLight;
    [SerializeField] Material partMaterial;

    [Header("UI")]
    [SerializeField] TMP_Dropdown entryDropdown;
    [SerializeField] Button previousButton;
    [SerializeField] Button nextButton;
    [SerializeField] Button renderCurrentButton;
    [SerializeField] Button renderAllButton;
    [SerializeField] TextMeshProUGUI infoLabel;
    [SerializeField] TextMeshProUGUI hintLabel;

    [Header("Presentation")]
    [SerializeField] Vector3 defaultFrontEuler = new Vector3(0f, 180f, 0f);
    [SerializeField] Vector3 backSlotEuler = new Vector3(0f, 0f, 0f);

    [Header("Input")]
    [SerializeField] float rotationSpeed = 0.35f;

    readonly List<IconRenderEntry> entries = new List<IconRenderEntry>();

    GameObject currentMannequin;
    int currentIndex;
    bool isDragging;
    Vector2 lastPointerPosition;
    Coroutine batchRoutine;
    Coroutine refreshRoutine;

    void Awake()
    {
        EnsureSceneReferences();
        BindUi();
    }

    void Start()
    {
        EquipmentData.Instance.EnsureLoaded();
        ReloadCatalog();

        if (hintLabel)
        {
            hintLabel.text =
                "左键拖拽旋转 | A/D 切换 | F5 渲染当前 | F6 全部批量 | F7 仅臂甲批量\n" +
                "装备先穿在骨骼模特上，再自动隐藏模特只显示该部位";
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
            var existing = GameObject.Find("IconPreviewStage");
            previewStage = existing ? existing.transform : new GameObject("IconPreviewStage").transform;
        }

        if (!keyLight)
            keyLight = FindFirstObjectByType<Light>();

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
        entries.AddRange(IconRenderCatalog.BuildFromEquipmentSets());
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
        ClearMannequin();

        if (entries.Count == 0)
        {
            UpdateInfoLabel(null);
            return;
        }

        var entry = entries[currentIndex];
        previewStage.localRotation = entry.slot == SyntyEquipmentSlot.Back
            ? Quaternion.Euler(backSlotEuler)
            : Quaternion.Euler(defaultFrontEuler);

        currentMannequin = IconStudioMannequinPresenter.Present(entry, previewStage, partMaterial);

        UpdateInfoLabel(entry);
        RequestPreviewRefresh();
    }

    void RequestPreviewRefresh()
    {
        if (refreshRoutine != null)
            StopCoroutine(refreshRoutine);

        refreshRoutine = StartCoroutine(RefreshPreviewAfterFrame());
    }

    IEnumerator RefreshPreviewAfterFrame()
    {
        yield return null;

        if (currentMannequin)
            IconStudioSkinnedBounds.PrepareSkinnedMeshes(currentMannequin.transform);

        UpdatePreviewCamera();
        refreshRoutine = null;
    }

    void ClearMannequin()
    {
        if (!currentMannequin)
            return;

        if (Application.isPlaying)
            Destroy(currentMannequin);
        else
            DestroyImmediate(currentMannequin);

        currentMannequin = null;
    }

    void UpdateInfoLabel(IconRenderEntry entry)
    {
        if (!infoLabel)
            return;

        if (entry == null)
        {
            infoLabel.text = "未找到可渲染装备，请确认 EquipmentSets.csv 已加载。";
            return;
        }

        var pixelSize = IconStudioSettings.GetOutputPixelSize(entry.slot);
        var gridSize = IconStudioSettings.GetGridSize(entry.slot);

        infoLabel.text =
            $"{entry.DisplayLabel}\n" +
            $"部件: {string.Join(", ", entry.parts)}\n" +
            $"输出: {entry.FileName}  ({gridSize.x}x{gridSize.y} 格, {pixelSize.x}x{pixelSize.y}px)\n" +
            "布局: 模特穿戴 + 骨骼摆姿";
    }

    void UpdatePreviewCamera()
    {
        if (!previewCamera || entries.Count == 0 || !previewStage)
            return;

        IconStudioCapture.FitCameraForPreview(previewCamera, previewStage, entries[currentIndex].slot);
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

        if (Input.GetKeyDown(KeyCode.F7))
            StartBatchRenderForSlot(SyntyEquipmentSlot.Forearm);
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

        var yaw = -delta.x * rotationSpeed;
        var pitch = delta.y * rotationSpeed;
        previewStage.Rotate(Vector3.up, yaw, Space.World);
        previewStage.Rotate(Vector3.right, pitch, Space.World);
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
        if (IconStudioCapture.CaptureToPng(previewCamera, previewStage, entry.slot, entry.FileName, out var path))
            Debug.Log($"[IconStudio] Rendered current icon: {path}");
    }

    void StartBatchRender()
    {
        if (batchRoutine != null)
            StopCoroutine(batchRoutine);

        batchRoutine = StartCoroutine(BatchRenderRoutine(null));
    }

    public void StartBatchRenderForSlot(SyntyEquipmentSlot slot)
    {
        if (batchRoutine != null)
            StopCoroutine(batchRoutine);

        batchRoutine = StartCoroutine(BatchRenderRoutine(slot));
    }

    IEnumerator BatchRenderRoutine(SyntyEquipmentSlot? slotFilter)
    {
        if (entries.Count == 0)
            yield break;

        var originalIndex = currentIndex;
        var successCount = 0;
        var processedCount = 0;

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (slotFilter.HasValue && entry.slot != slotFilter.Value)
                continue;

            processedCount++;
            currentIndex = i;
            SyncDropdown();
            ShowCurrentEntry();
            yield return null;

            if (IconStudioCapture.CaptureToPng(previewCamera, previewStage, entry.slot, entry.FileName, out _))
                successCount++;
        }

        currentIndex = originalIndex;
        SyncDropdown();
        ShowCurrentEntry();

        var scopeLabel = slotFilter.HasValue ? slotFilter.Value.ToString() : "All";
        Debug.Log($"[IconStudio] Batch render finished ({scopeLabel}): {successCount}/{processedCount}");
        batchRoutine = null;
    }
}
