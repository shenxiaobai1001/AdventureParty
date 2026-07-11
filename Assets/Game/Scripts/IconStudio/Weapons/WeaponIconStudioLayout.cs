using UnityEngine;

public static class WeaponIconStudioLayout
{
    public static void Apply(GameObject weaponRoot, WeaponCategory category)
    {
        if (!weaponRoot)
            return;

        weaponRoot.transform.localPosition = Vector3.zero;

        if (category == WeaponCategory.Shield)
        {
            weaponRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return;
        }

        // X CCW 90°, then Y 90° for icon framing.
        weaponRoot.transform.localRotation = Quaternion.Euler(90f, 90f, 0f);
    }

    public static void CenterAtOrigin(GameObject weaponRoot)
    {
        if (!weaponRoot)
            return;

        var bounds = CalculateBounds(weaponRoot);
        if (bounds.size.sqrMagnitude <= 0f)
            return;

        weaponRoot.transform.position += -bounds.center;
    }

    static Bounds CalculateBounds(GameObject root)
    {
        var bounds = new Bounds(root.transform.position, Vector3.zero);
        var hasBounds = false;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer || !renderer.enabled)
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
            bounds = new Bounds(root.transform.position, Vector3.one * 0.1f);

        return bounds;
    }
}
