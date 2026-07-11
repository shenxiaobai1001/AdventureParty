using UInventoryGrid;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sizes inventory item visuals to match the hosting grid's visual cell layout.
/// </summary>
public static class ItemVisualLayout
{
    const float IconVisualScale = 2f;

    const float HeadIconScaleMultiplier = 0.7f;
    const float HipsIconScaleMultiplier = 0.7f;
    const float LegIconScaleMultiplier = 0.8f;

    public static void Apply(Item item)
    {
        if (!item || !item.data || !item.rectTransform)
            return;

        ApplySize(item);
        ApplyLayerOrder(item);
        ApplyIcon(item);
        item.transform.SetAsLastSibling();
    }

    static void ApplySize(Item item)
    {
        var corrected = item.correctedSize;
        var cell = GetCellSize(item);

        item.rectTransform.localScale = Vector3.one;
        item.rectTransform.localRotation = Quaternion.identity;
        item.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        item.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        item.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        item.rectTransform.sizeDelta = new Vector2(
            corrected.width * cell.x,
            corrected.height * cell.y);
    }

    static void ApplyIcon(Item item)
    {
        if (!item.icon)
            return;

        var iconRect = item.icon.rectTransform;
        item.icon.sprite = item.data.icon;
        item.icon.color = item.revealed ? item.data.normalIconColor : item.data.hiddenIconColor;
        item.icon.preserveAspect = true;
        iconRect.localScale = Vector3.one * GetIconScale(item);
        iconRect.localRotation = Quaternion.Euler(0f, 0f, item.isRotated ? -90f : 0f);
    }

    static float GetIconScale(Item item)
    {
        var scale = IconVisualScale * GetSlotIconScaleMultiplier(item);

        if (!item.isRotated || item.data == null)
            return scale;

        var width = Mathf.Max(1, item.data.size.width);
        var height = Mathf.Max(1, item.data.size.height);
        return scale * ((float)height / width);
    }

    static float GetSlotIconScaleMultiplier(Item item)
    {
        if (item.data is SyntyEquipmentItemData equipment)
            return GetSlotIconScaleMultiplier(equipment.equipmentSlot);

        switch (item.data.itemType)
        {
            case ItemType.Head: return HeadIconScaleMultiplier;
            case ItemType.Hips: return HipsIconScaleMultiplier;
            case ItemType.Leg: return LegIconScaleMultiplier;
            default: return 1f;
        }
    }

    static float GetSlotIconScaleMultiplier(SyntyEquipmentSlot slot)
    {
        switch (slot)
        {
            case SyntyEquipmentSlot.Head: return HeadIconScaleMultiplier;
            case SyntyEquipmentSlot.Hips: return HipsIconScaleMultiplier;
            case SyntyEquipmentSlot.Leg: return LegIconScaleMultiplier;
            default: return 1f;
        }
    }

    static void ApplyLayerOrder(Item item)
    {
        var index = 0;

        if (item.background)
            item.background.transform.SetSiblingIndex(index++);

        if (item.slotGrid)
        {
            item.slotGrid.transform.SetSiblingIndex(index++);
            item.slotGrid.raycastTarget = false;
        }
        else
        {
            var grid = item.transform.Find("Grid");
            if (grid)
            {
                grid.SetSiblingIndex(index++);
                if (grid.TryGetComponent<Image>(out var gridImage))
                    gridImage.raycastTarget = false;
            }
        }

        if (item.icon)
            item.icon.transform.SetSiblingIndex(index++);

        if (item.stackCountText)
            item.stackCountText.transform.SetSiblingIndex(index++);

        if (item.searchIcon)
            item.searchIcon.transform.SetSiblingIndex(index++);
    }

    public static Vector2 GetSlotPixelSize(Item item) => GetCellSize(item);

    public static Vector2 GetSlotPixelSize(InventoryGrid grid)
    {
        if (grid)
            return grid.GetCellSize();

        return Vector2.one * 64f;
    }

    public static Vector2 GetCellSize(Item item)
    {
        if (item.inventoryGrid)
            return item.inventoryGrid.GetCellSize();

        if (item.inventory && item.inventory.inventorySettings)
        {
            var settings = item.inventory.inventorySettings;
            return new Vector2(
                settings.slotSize.x * settings.slotScale,
                settings.slotSize.y * settings.slotScale);
        }

        return Vector2.one * 64f;
    }
}
