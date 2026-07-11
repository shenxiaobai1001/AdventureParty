using UInventoryGrid;
using UnityEngine;

[CreateAssetMenu(fileName = "SyntyEquipmentItem", menuName = "AdventureParty/Synty Equipment Item")]
public class SyntyEquipmentItemData : ItemData
{
    [Header("Synty Equipment")]
    public int equipmentItemId;
    public int setId;
    public SyntyEquipmentSlot equipmentSlot;
    [TextArea(1, 4)]
    public string parts;
    public GameObject worldPickupPrefab;

    public string[] GetPartNames()
    {
        return EquipmentPartParser.Split(parts);
    }

    public void ApplyFromRow(EquipmentItemRow row)
    {
        if (row == null)
            return;

        equipmentItemId = row.id;
        setId = row.setId;
        EquipmentSlotUtility.TryParseSlot(row.slot, out equipmentSlot);
        parts = row.parts ?? string.Empty;
        itemName = row.name;
        description = row.name;
        weight = row.weight > 0f ? row.weight : 1f;
        itemType = EquipmentSlotUtility.ToItemType(equipmentSlot);
        size = new SizeInt(Mathf.Max(1, row.gridW), Mathf.Max(1, row.gridH));
        stackable = false;
        maxStack = 1;
    }
}
