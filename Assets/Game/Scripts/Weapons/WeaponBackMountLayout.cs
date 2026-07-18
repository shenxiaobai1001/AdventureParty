using UnityEngine;

/// <summary>
/// Local pose for weapons parented to the hero back mount socket.
/// </summary>
public static class WeaponBackMountLayout
{
    static readonly Vector3 FirstWeaponLocalPosition = new Vector3(0.26f, 0.3f, 0.04f);
    static readonly Vector3 FirstWeaponLocalEuler = new Vector3(-155f, 90f, 4f);

    static readonly Vector3 SecondWeaponLocalPosition = new Vector3(-0.26f, 0.3f, 0.04f);
    static readonly Vector3 SecondWeaponLocalEuler = new Vector3(155f, 90f, 4f);

    public static void Apply(Transform weaponRoot, WeaponCategory category, int crossedWeaponIndex)
    {
        if (!weaponRoot)
            return;

        if (category == WeaponCategory.Shield)
        {
            weaponRoot.localPosition = Vector3.zero;
            weaponRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);
            return;
        }

        switch (crossedWeaponIndex)
        {
            case 0:
                weaponRoot.localPosition = FirstWeaponLocalPosition;
                weaponRoot.localRotation = Quaternion.Euler(FirstWeaponLocalEuler);
                break;
            case 1:
                weaponRoot.localPosition = SecondWeaponLocalPosition;
                weaponRoot.localRotation = Quaternion.Euler(SecondWeaponLocalEuler);
                break;
            default:
                weaponRoot.localPosition = Vector3.zero;
                weaponRoot.localRotation = Quaternion.Euler(-90f, 0f, 90f);
                break;
        }
    }
}
