using UnityEngine;

public static class EquipmentVisualBounds
{
    public static Bounds CalculateRendererBounds(GameObject root)
    {
        var bounds = new Bounds(root.transform.position, Vector3.zero);
        var hasBounds = false;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
            bounds = new Bounds(root.transform.position, Vector3.one * 0.25f);

        return bounds;
    }

    public static void CenterVisualAtParentOrigin(Transform visualRoot)
    {
        if (!visualRoot)
            return;

        var bounds = CalculateRendererBounds(visualRoot.gameObject);
        var targetCenter = visualRoot.parent ? visualRoot.parent.position : Vector3.zero;
        visualRoot.position += targetCenter - bounds.center;
    }

    public static void FitBoxColliderToVisual(GameObject root, GameObject visualRoot)
    {
        if (!root || !visualRoot)
            return;

        CenterVisualAtParentOrigin(visualRoot.transform);

        var bounds = CalculateRendererBounds(visualRoot);
        var collider = root.GetComponent<BoxCollider>();
        if (!collider)
            collider = root.AddComponent<BoxCollider>();

        collider.isTrigger = false;
        collider.center = root.transform.InverseTransformPoint(bounds.center);
        collider.size = bounds.size;
    }

    public static BoxCollider EnsurePickupTriggerCollider(GameObject root, float scaleMultiplier = 2f)
    {
        if (!root || scaleMultiplier <= 0f)
            return null;

        var solid = root.GetComponent<BoxCollider>();
        if (!solid)
            return null;

        var triggerTransform = root.transform.Find("PickupTrigger");
        if (!triggerTransform)
        {
            var triggerObject = new GameObject("PickupTrigger");
            triggerObject.transform.SetParent(root.transform, false);
            triggerTransform = triggerObject.transform;
        }

        triggerTransform.localPosition = solid.center;
        triggerTransform.localRotation = Quaternion.identity;
        triggerTransform.localScale = Vector3.one;

        var trigger = triggerTransform.GetComponent<BoxCollider>();
        if (!trigger)
            trigger = triggerTransform.gameObject.AddComponent<BoxCollider>();

        trigger.isTrigger = true;
        trigger.center = Vector3.zero;
        trigger.size = solid.size * scaleMultiplier;
        return trigger;
    }

    public static void EnsurePickupRigidbody(GameObject root)
    {
        if (!root)
            return;

        var rigidbody = root.GetComponent<Rigidbody>();
        if (!rigidbody)
            rigidbody = root.AddComponent<Rigidbody>();

        rigidbody.isKinematic = true;
        rigidbody.useGravity = false;
    }
}
