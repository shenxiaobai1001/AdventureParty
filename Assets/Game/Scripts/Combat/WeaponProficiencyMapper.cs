using UnityEngine;

/// <summary>
/// Maps inventory <see cref="WeaponCategory"/> to combat proficiency lines and grip rules.
/// </summary>
public static class WeaponProficiencyMapper
{
    public static WeaponProficiencyType GetProficiencyType(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.GreatSword2H:
                return WeaponProficiencyType.GreatSword;
            case WeaponCategory.HeavyWeapon2H:
                return WeaponProficiencyType.HeavyWeapon;
            case WeaponCategory.Polearm2H:
                return WeaponProficiencyType.Polearm;
            case WeaponCategory.Bow:
                return WeaponProficiencyType.BowCrossbow;
            case WeaponCategory.Shield:
                return WeaponProficiencyType.Shield;
            case WeaponCategory.Sword1H:
                return WeaponProficiencyType.Longsword;
            case WeaponCategory.Hammer1H:
                return WeaponProficiencyType.HammerAxe;
            case WeaponCategory.Dagger1H:
                // Physical short blades use Longsword; MartialArts is for unarmed fighting.
                return WeaponProficiencyType.Longsword;
            case WeaponCategory.FirearmRifle:
            case WeaponCategory.FirearmPistol:
                return WeaponProficiencyType.Firearm;
            default:
                return WeaponProficiencyType.Longsword;
        }
    }

    public static WeaponProficiencyType GetProficiencyType(SyntyWeaponItemData weapon)
    {
        if (!weapon)
            return WeaponProficiencyType.Longsword;

        if (weapon.proficiencyOverride)
            return weapon.proficiencyType;

        return GetProficiencyType(weapon.category);
    }

    public static bool OccupiesBothHands(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.GreatSword2H:
            case WeaponCategory.HeavyWeapon2H:
            case WeaponCategory.Polearm2H:
            case WeaponCategory.Bow:
            case WeaponCategory.FirearmRifle:
                return true;
            default:
                return false;
        }
    }

    public static bool IsOffHandCandidate(WeaponCategory category)
    {
        return category == WeaponCategory.Shield
            || category == WeaponCategory.FirearmPistol
            || category == WeaponCategory.Sword1H
            || category == WeaponCategory.Hammer1H
            || category == WeaponCategory.Dagger1H;
    }

    public static bool CanDualWield(SyntyWeaponItemData left, SyntyWeaponItemData right)
    {
        if (!left || !right)
            return false;

        if (left.category != right.category)
            return false;

        return left.category == WeaponCategory.Sword1H
            || left.category == WeaponCategory.Hammer1H
            || left.category == WeaponCategory.Dagger1H;
    }

    public static bool CanPairOffHand(SyntyWeaponItemData primary, SyntyWeaponItemData offHand)
    {
        if (!primary || !offHand || OccupiesBothHands(primary.category))
            return false;

        if (offHand.category == WeaponCategory.Shield)
            return primary.category == WeaponCategory.Sword1H
                || primary.category == WeaponCategory.Hammer1H
                || primary.category == WeaponCategory.Dagger1H;

        if (offHand.category == WeaponCategory.FirearmPistol)
            return primary.category == WeaponCategory.Sword1H
                || primary.category == WeaponCategory.Hammer1H
                || primary.category == WeaponCategory.Dagger1H;

        return CanDualWield(primary, offHand);
    }
}
