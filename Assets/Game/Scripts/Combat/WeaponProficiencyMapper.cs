using UnityEngine;

/// <summary>
/// Weapon type ↔ proficiency identity mapping and grip / dual-wield rules.
/// </summary>
public static class WeaponProficiencyMapper
{
    public static WeaponProficiencyType GetProficiencyType(WeaponCategory category)
    {
        if (category == WeaponCategory.ObsoleteDagger)
            return WeaponProficiencyType.Sword;

        // Inventory categories share integer identity with proficiency lines 0–13.
        return (WeaponProficiencyType)(int)category;
    }

    public static WeaponProficiencyType GetProficiencyType(SyntyWeaponItemData weapon)
    {
        if (!weapon)
            return WeaponProficiencyType.MartialArts;

        if (weapon.proficiencyOverride)
            return weapon.proficiencyType;

        return GetProficiencyType(weapon.category);
    }

    public static WeaponHandRule GetHandRule(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Shield:
                return WeaponHandRule.LeftOnly;
            case WeaponCategory.Bow:
                // Archer anim: left holds the bow, right pulls the string.
                return WeaponHandRule.LeftTwoHand;
            case WeaponCategory.Staff:
                return WeaponHandRule.RightLocksLeft;
            case WeaponCategory.GreatSword:
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe:
            case WeaponCategory.Spear:
            case WeaponCategory.Crossbow:
            case WeaponCategory.LongGun:
                return WeaponHandRule.TwoHand;
            default:
                return WeaponHandRule.OneHand;
        }
    }

    public static bool OccupiesBothHands(WeaponCategory category)
    {
        var rule = GetHandRule(category);
        return rule == WeaponHandRule.TwoHand
            || rule == WeaponHandRule.RightLocksLeft
            || rule == WeaponHandRule.LeftTwoHand;
    }

    /// <summary>Primary grip is the left hand (bow).</summary>
    public static bool IsLeftHandPrimary(WeaponCategory category)
        => GetHandRule(category) == WeaponHandRule.LeftTwoHand;

    public static bool IsEdged(WeaponCategory category)
    {
        return category == WeaponCategory.Sword || category == WeaponCategory.ObsoleteDagger;
    }

    public static bool IsOneHandMelee(WeaponCategory category)
    {
        return category == WeaponCategory.Sword
            || category == WeaponCategory.Hammer
            || category == WeaponCategory.Axe
            || category == WeaponCategory.ObsoleteDagger;
    }

    public static bool CanEquipToHand(WeaponCategory category, WeaponHand hand)
    {
        switch (GetHandRule(category))
        {
            case WeaponHandRule.LeftOnly:
            case WeaponHandRule.LeftTwoHand:
                return hand == WeaponHand.Left;
            case WeaponHandRule.TwoHand:
            case WeaponHandRule.RightLocksLeft:
                return hand == WeaponHand.Right;
            default:
                return true;
        }
    }

    /// <summary>
    /// Returns false with a reason if equipping would conflict with current hands.
    /// Caller must unequip first — never auto-strip.
    /// </summary>
    public static bool CanEquip(
        SyntyWeaponItemData weapon,
        WeaponHand hand,
        SyntyWeaponItemData currentRight,
        SyntyWeaponItemData currentLeft,
        out string reason)
    {
        reason = null;
        if (!weapon)
        {
            reason = "无效武器";
            return false;
        }

        if (!CanEquipToHand(weapon.category, hand))
        {
            if (weapon.category == WeaponCategory.Shield)
                reason = "盾牌只能装备到左手";
            else if (weapon.category == WeaponCategory.Bow)
                reason = "弓只能装备到左手（右手拉弦）";
            else
                reason = "该武器只能从右手装备（占双手或锁左手）";
            return false;
        }

        var rule = GetHandRule(weapon.category);

        if (hand == WeaponHand.Right)
        {
            if (currentRight != null && currentRight != weapon)
            {
                reason = "请先卸下右手武器";
                return false;
            }

            if (rule == WeaponHandRule.TwoHand || rule == WeaponHandRule.RightLocksLeft)
            {
                if (currentLeft != null && currentLeft != weapon)
                {
                    reason = "请先卸下左手武器";
                    return false;
                }
            }
        }
        else
        {
            if (currentLeft != null && currentLeft != weapon)
            {
                reason = "请先卸下左手武器";
                return false;
            }

            if (rule == WeaponHandRule.LeftTwoHand)
            {
                if (currentRight != null && currentRight != weapon)
                {
                    reason = "请先卸下右手武器";
                    return false;
                }
            }
            else if (currentRight != null && OccupiesBothHands(currentRight.category))
            {
                reason = "请先卸下右手双手武器";
                return false;
            }
        }

        return true;
    }
}
