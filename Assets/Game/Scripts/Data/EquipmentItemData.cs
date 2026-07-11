using System.Collections.Generic;
using UInventoryGrid;
using UnityEngine;

public class EquipmentItemData : Singleton<EquipmentItemData>
{
    public ConfigTable<EquipmentItemRow> items = new ConfigTable<EquipmentItemRow>();

    readonly Dictionary<int, EquipmentItemRow> _bySetAndSlot = new Dictionary<int, EquipmentItemRow>();

    bool _loaded;

    public bool Init()
    {
        _loaded = items.Load("EquipmentItems.csv");
        RebuildSetSlotIndex();
        return _loaded;
    }

    public bool EnsureLoaded()
    {
        if (_loaded && items.GetListInfo().Count > 0)
            return true;

        return Init() && items.GetListInfo().Count > 0;
    }

    void RebuildSetSlotIndex()
    {
        _bySetAndSlot.Clear();
        foreach (var row in items.GetListInfo())
        {
            if (row == null || !EquipmentSlotUtility.TryParseSlot(row.slot, out var slot))
                continue;

            _bySetAndSlot[EquipmentSlotUtility.ComposeItemId(row.setId, slot)] = row;
        }
    }

    public EquipmentItemRow GetItem(int itemId)
    {
        return items.GetInfo(itemId);
    }

    public bool TryGetItem(int itemId, out EquipmentItemRow row)
    {
        return items.TryGetInfo(itemId, out row);
    }

    public EquipmentItemRow GetBySetAndSlot(int setId, SyntyEquipmentSlot slot)
    {
        var itemId = EquipmentSlotUtility.ComposeItemId(setId, slot);
        return GetItem(itemId);
    }

    public static bool IsSyntyEquipment(ItemData data, out SyntyEquipmentItemData equipmentItem)
    {
        equipmentItem = data as SyntyEquipmentItemData;
        return equipmentItem != null;
    }
}

public class EquipmentItemRow : NamedData
{
    public int setId;
    public string slot;
    public string parts;
    public string icon;
    public string worldPrefab;
    public int gridW;
    public int gridH;
    public string itemType;
    public float weight;

    public SyntyEquipmentSlot GetEquipmentSlot()
    {
        return EquipmentSlotUtility.TryParseSlot(slot, out var parsed) ? parsed : default;
    }

    public ItemType GetInventoryItemType()
    {
        return EquipmentSlotUtility.ToItemType(GetEquipmentSlot());
    }

    public string GetIconAssetPath()
    {
        if (string.IsNullOrWhiteSpace(icon))
            return string.Empty;

        var path = icon.Trim().Replace('\\', '/');
        if (!path.StartsWith("Assets/"))
            path = IconStudioSettings.OutputRoot + "/" + path;

        return path;
    }

    public string GetWorldPrefabAssetPath()
    {
        if (string.IsNullOrWhiteSpace(worldPrefab))
            return string.Empty;

        return worldPrefab.Trim().Replace('\\', '/');
    }
}
