using UInventoryGrid;

public interface IWorldItemPickup
{
    ItemData ItemData { get; }
    UnityEngine.Vector3 GetPickupAnchorPosition();
    UnityEngine.Collider GetPickupTriggerCollider();
    bool ContainsProbePoint(UnityEngine.Vector3 worldPoint, float fallbackRadius);
    bool TryPickup(Inventory inventory);
}
