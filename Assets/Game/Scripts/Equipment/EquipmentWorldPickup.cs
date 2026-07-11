using UInventoryGrid;
using UnityEngine;

/// <summary>
/// World pickup for a single Synty equipment item. Visual pose matches Icon Studio output.
/// </summary>
public class EquipmentWorldPickup : MonoBehaviour
{
    [SerializeField] SyntyEquipmentItemData itemData;
    [SerializeField] int equipmentItemId;

    public SyntyEquipmentItemData ItemData => itemData;

    void Awake()
    {
        AlignVisualToRoot();
    }

    void AlignVisualToRoot()
    {
        var visualRoot = transform.Find("VisualRoot");
        if (!visualRoot)
            return;

        EquipmentVisualBounds.CenterVisualAtParentOrigin(visualRoot);
        EquipmentVisualBounds.FitBoxColliderToVisual(gameObject, visualRoot.gameObject);
        EquipmentVisualBounds.EnsurePickupTriggerCollider(gameObject, 2f);
        EquipmentVisualBounds.EnsurePickupRigidbody(gameObject);
    }

    public Vector3 GetPickupAnchorPosition()
    {
        var trigger = GetPickupTriggerCollider();
        return trigger ? trigger.bounds.center : transform.position;
    }

    public Collider GetPickupTriggerCollider()
    {
        var triggerTransform = transform.Find("PickupTrigger");
        if (triggerTransform && triggerTransform.TryGetComponent<Collider>(out var trigger))
            return trigger;

        foreach (var collider in GetComponents<BoxCollider>())
        {
            if (collider && collider.isTrigger)
                return collider;
        }

        return GetComponent<Collider>();
    }

    public bool ContainsProbePoint(Vector3 worldPoint, float fallbackRadius)
    {
        var trigger = GetPickupTriggerCollider();
        if (trigger)
        {
            var closest = trigger.ClosestPoint(worldPoint);
            return (closest - worldPoint).sqrMagnitude < 0.05f;
        }

        return Vector3.Distance(worldPoint, transform.position) <= fallbackRadius;
    }

    public void BindItemData(SyntyEquipmentItemData data)
    {
        itemData = data;
        if (itemData)
            equipmentItemId = itemData.equipmentItemId;
    }

    public void BindItemId(int itemId)
    {
        equipmentItemId = itemId;
    }

    public bool TryPickup(Inventory inventory)
    {
        return EquipmentInventoryBridge.TryPickup(this, inventory);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (itemData)
            equipmentItemId = itemData.equipmentItemId;
    }
#endif
}
