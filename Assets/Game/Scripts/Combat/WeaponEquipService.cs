using UnityEngine;

/// <summary>
/// Smart weapon equip / unequip for the weapon bar (no L/R picker).
/// Holstered in grid ≠ equipped; Equip applies hand slots with conflict replace rules.
/// </summary>
public static class WeaponEquipService
{
    public static bool IsEquipped(HeroWeaponVisual visual, SyntyWeaponItemData weapon)
    {
        if (!visual || !weapon)
            return false;

        return visual.equippedRight == weapon || visual.equippedLeft == weapon;
    }

    public static bool IsOneHandCombatWeapon(WeaponCategory category)
    {
        return WeaponProficiencyMapper.IsOneHandMelee(category)
            || category == WeaponCategory.ShortGun;
    }

    public static bool IsShieldPairablePrimary(WeaponCategory category)
        => WeaponProficiencyMapper.IsOneHandMelee(category);

    /// <summary>
    /// Equip with automatic unequip/replace per design rules.
    /// Does not enter combat stance — only updates hand slots / loadout.
    /// </summary>
    public static bool TrySmartEquip(HeroWeaponVisual visual, SyntyWeaponItemData weapon, out string reason)
    {
        reason = null;
        if (!visual || !weapon)
        {
            reason = "无效武器";
            return false;
        }

        if (IsEquipped(visual, weapon))
        {
            reason = "已经装备";
            return false;
        }

        var cat = weapon.category;

        // Shield: only with a one-hand melee already on right.
        if (cat == WeaponCategory.Shield)
            return EquipShield(visual, weapon, out reason);

        // Bow: left hand holds the bow (right pulls string). Clears both hands.
        if (cat == WeaponCategory.Bow)
        {
            visual.ForceSetHands(null, weapon);
            reason = null;
            return true;
        }

        // Two-hand / staff / crossbow: clear both hands, occupy right (+ lock left).
        if (WeaponProficiencyMapper.OccupiesBothHands(cat))
        {
            visual.ForceSetHands(weapon, null);
            reason = null;
            return true;
        }

        // One-hand melee / short gun.
        if (IsOneHandCombatWeapon(cat))
            return EquipOneHand(visual, weapon, out reason);

        reason = "无法装备该武器类型";
        return false;
    }

    public static bool TrySmartUnequip(HeroWeaponVisual visual, SyntyWeaponItemData weapon, out string reason)
    {
        reason = null;
        if (!visual || !weapon)
        {
            reason = "无效武器";
            return false;
        }

        if (!IsEquipped(visual, weapon))
        {
            reason = "未装备";
            return false;
        }

        var right = visual.equippedRight;
        var left = visual.equippedLeft;

        if (right == weapon)
        {
            right = null;
            // Shield cannot stand alone.
            if (left && left.category == WeaponCategory.Shield)
                left = null;
        }
        else if (left == weapon)
        {
            left = null;
        }

        visual.ForceSetHands(right, left);
        return true;
    }

    static bool EquipShield(HeroWeaponVisual visual, SyntyWeaponItemData shield, out string reason)
    {
        var right = visual.equippedRight;
        if (!right || !IsShieldPairablePrimary(right.category))
        {
            reason = "必须先装备一把单手近战武器才能装备盾牌";
            return false;
        }

        // Dual wield → keep right, drop left, put shield.
        visual.ForceSetHands(right, shield);
        reason = null;
        return true;
    }

    static bool EquipOneHand(HeroWeaponVisual visual, SyntyWeaponItemData weapon, out string reason)
    {
        reason = null;
        var right = visual.equippedRight;
        var left = visual.equippedLeft;

        // Empty or only invalid left → right.
        if (!right)
        {
            // Left-only shield is illegal; clear it.
            visual.ForceSetHands(weapon, null);
            return true;
        }

        // Replacing two-hand / staff / bow.
        if ((right && WeaponProficiencyMapper.OccupiesBothHands(right.category))
            || (left && WeaponProficiencyMapper.IsLeftHandPrimary(left.category)))
        {
            visual.ForceSetHands(weapon, null);
            return true;
        }

        // Right is one-hand.
        if (!IsOneHandCombatWeapon(right.category))
        {
            visual.ForceSetHands(weapon, null);
            return true;
        }

        // Already have shield on left → second 1H becomes dual (drop shield).
        if (left && left.category == WeaponCategory.Shield)
        {
            if (CanDualWieldPair(right, weapon))
            {
                visual.ForceSetHands(right, weapon);
                return true;
            }

            // Cannot dual (e.g. short gun?) → replace right, keep? drop shield and replace.
            visual.ForceSetHands(weapon, null);
            return true;
        }

        // Left empty → dual if both one-hand melee (or compatible).
        if (!left)
        {
            if (CanDualWieldPair(right, weapon))
            {
                visual.ForceSetHands(right, weapon);
                return true;
            }

            // Replace right.
            visual.ForceSetHands(weapon, null);
            return true;
        }

        // Already dual → replace left with new weapon if dual-compatible with right, else replace both with new on right.
        if (CanDualWieldPair(right, weapon))
        {
            visual.ForceSetHands(right, weapon);
            return true;
        }

        visual.ForceSetHands(weapon, null);
        return true;
    }

    static bool CanDualWieldPair(SyntyWeaponItemData a, SyntyWeaponItemData b)
    {
        if (!a || !b)
            return false;

        // Dual only for one-hand melee (sword/hammer/axe). Short gun not dual with melee for now.
        return WeaponProficiencyMapper.IsOneHandMelee(a.category)
            && WeaponProficiencyMapper.IsOneHandMelee(b.category);
    }
}
