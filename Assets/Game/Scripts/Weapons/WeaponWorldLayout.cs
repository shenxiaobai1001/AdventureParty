using UnityEngine;

/// <summary>
/// Pose for weapon world pickups (ground loot), separate from icon studio framing.
/// </summary>
public static class WeaponWorldLayout
{
    public static void Apply(GameObject weaponRoot, WeaponCategory category)
    {
        if (!weaponRoot)
            return;

        weaponRoot.transform.localPosition = Vector3.zero;
        weaponRoot.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        if (category == WeaponCategory.Shield || category == WeaponCategory.Bow)
            weaponRoot.transform.localRotation = Quaternion.Euler(75f, 35f, 0f);
    }
}
