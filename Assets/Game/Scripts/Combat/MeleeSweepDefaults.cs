using UnityEngine;

/// <summary>
/// Default melee sweep capsule sizes per weapon category.
/// </summary>
public static class MeleeSweepDefaults
{
    public static bool IsRangedOrNonMelee(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Bow:
            case WeaponCategory.Crossbow:
            case WeaponCategory.LongGun:
            case WeaponCategory.ShortGun:
            case WeaponCategory.Shield:
                return true;
            default:
                return false;
        }
    }

    public static void GetDefaults(WeaponCategory category, out Vector3 localRoot, out Vector3 localTip, out float radius)
    {
        // Synty props vary; these are safe starting capsules along local +Z.
        // Prefer AutoFitFromRenderers at runtime; these are fallbacks.
        switch (category)
        {
            case WeaponCategory.Sword:
                localRoot = new Vector3(0f, 0f, 0.05f);
                localTip = new Vector3(0f, 0f, 0.75f);
                radius = 0.06f;
                break;
            case WeaponCategory.GreatSword:
                localRoot = new Vector3(0f, 0f, 0.08f);
                localTip = new Vector3(0f, 0f, 1.15f);
                radius = 0.08f;
                break;
            case WeaponCategory.Hammer:
            case WeaponCategory.Axe:
                localRoot = new Vector3(0f, 0f, 0.05f);
                localTip = new Vector3(0f, 0f, 0.7f);
                radius = 0.1f;
                break;
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe:
                localRoot = new Vector3(0f, 0f, 0.08f);
                localTip = new Vector3(0f, 0f, 1.0f);
                radius = 0.12f;
                break;
            case WeaponCategory.Spear:
                localRoot = new Vector3(0f, 0f, 0.1f);
                localTip = new Vector3(0f, 0f, 1.6f);
                radius = 0.06f;
                break;
            case WeaponCategory.Staff:
                localRoot = new Vector3(0f, 0f, -0.4f);
                localTip = new Vector3(0f, 0f, 0.9f);
                radius = 0.07f;
                break;
            default:
                localRoot = Vector3.zero;
                localTip = new Vector3(0f, 0f, 0.6f);
                radius = 0.08f;
                break;
        }
    }
}
