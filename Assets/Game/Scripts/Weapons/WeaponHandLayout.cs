using UnityEngine;

/// <summary>
/// Local pose for a weapon parented to hand grip sockets.
/// </summary>
public static class WeaponHandLayout
{
    public static void Apply(Transform weaponRoot, WeaponCategory category, bool isOffHand = false)
    {
        if (!weaponRoot)
            return;

        weaponRoot.localPosition = Vector3.zero;
        weaponRoot.localRotation = Quaternion.identity;

        if (category == WeaponCategory.Shield && isOffHand)
            weaponRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);

        if (category == WeaponCategory.FirearmPistol)
        {
            var yaw = isOffHand ? -90f : 90f;
            weaponRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
