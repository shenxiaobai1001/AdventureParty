using UnityEngine;

public static class WorldPickupProbe
{
    public static bool ContainsProbePoint(Collider trigger, Vector3 fallbackPosition, Vector3 worldPoint, float pickupRadius)
    {
        if (pickupRadius <= 0f)
            pickupRadius = 0.1f;

        if (trigger)
        {
            var closest = trigger.ClosestPoint(worldPoint);
            return (closest - worldPoint).sqrMagnitude <= pickupRadius * pickupRadius;
        }

        return Vector3.Distance(worldPoint, fallbackPosition) <= pickupRadius;
    }
}
