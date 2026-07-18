using UInventoryGrid;
using UnityEngine;

/// <summary>
/// World pickup for a single Synty weapon prefab.
/// </summary>
public class WeaponWorldPickup : MonoBehaviour, IWorldItemPickup
{
    [SerializeField] SyntyWeaponItemData itemData;

    public ItemData ItemData => itemData;

    void Awake()
    {
        EnsurePickupSetup();
    }

    void Start()
    {
        EnsurePickupSetup();
    }

    void EnsurePickupSetup()
    {
        var visualRoot = transform.Find("VisualRoot");
        if (visualRoot)
        {
            AlignVisualToRoot();
            return;
        }

        EquipmentVisualBounds.FitBoxColliderToVisual(gameObject, gameObject);
        EquipmentVisualBounds.EnsurePickupTriggerCollider(gameObject, 2f);
        EquipmentVisualBounds.EnsurePickupRigidbody(gameObject);
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
        return WorldPickupProbe.ContainsProbePoint(
            GetPickupTriggerCollider(),
            transform.position,
            worldPoint,
            fallbackRadius);
    }

    public void BindItemData(SyntyWeaponItemData data)
    {
        itemData = data;
    }

    public bool TryPickup(Inventory inventory)
    {
        var mainPanel = FindFirstObjectByType<UIMainControlPanel>();
        var entry = mainPanel ? mainPanel.GetSelectedCharacterEntry() : null;
        return WeaponInventoryBridge.TryPickup(this, inventory, entry);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (itemData)
            AlignVisualToRoot();
    }
#endif
}
