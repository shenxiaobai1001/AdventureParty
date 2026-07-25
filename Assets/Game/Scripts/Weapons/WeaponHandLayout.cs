using UnityEngine;

/// <summary>
/// Local pose offsets when a weapon is parented to a hand socket.
/// Shield / bow poses are owned by dedicated sockets (identity here).
/// Long/short guns use identity (no forced 90° tilt).
/// </summary>
public static class WeaponHandLayout
{
    public static void Apply(Transform weapon, WeaponCategory category, bool isOffHand)
    {
        if (!weapon)
            return;

        weapon.localPosition = Vector3.zero;
        weapon.localRotation = Quaternion.identity;
        weapon.localScale = Vector3.one;

        if (category == WeaponCategory.Shield
            || category == WeaponCategory.Bow
            || category == WeaponCategory.LongGun
            || category == WeaponCategory.ShortGun)
            return;
    }
}
