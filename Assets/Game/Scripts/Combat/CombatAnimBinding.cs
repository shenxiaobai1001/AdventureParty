using UnityEngine;

/// <summary>
/// Applies resolved combat loadouts to animator parameters (RPG or Warrior family).
/// </summary>
public static class CombatAnimBinding
{
    public static CombatMoveStance ResolveStance(ResolvedCombatLoadout loadout)
        => CombatMoveStanceResolver.ResolvePrimaryStance(loadout);

    /// <summary>
    /// Ensures correct controller for loadout and applies drawn/relax pose params.
    /// Returns true if RPG controller family is active.
    /// </summary>
    public static bool EnsureForLoadout(Animator animator, ResolvedCombatLoadout loadout, bool drawn)
    {
        var stance = ResolveStance(loadout);
        var useRpg = CombatAnimControllerCatalog.EnsureController(animator, stance);

        if (useRpg)
        {
            if (drawn && loadout != null && loadout.HasDrawableWeapon)
                ApplyCombatLoadout(animator, loadout, instant: true);
            else
                RpgAnimParams.ApplyRelaxMode(animator, true);
        }
        else
        {
            if (drawn)
                WarriorAnimParams.ApplyDrawn(animator);
            else
                WarriorAnimParams.ApplySheathed(animator);
        }

        return useRpg;
    }

    public static void ApplyCombatLoadout(Animator animator, ResolvedCombatLoadout loadout, bool instant = true)
    {
        if (!animator || loadout == null || !loadout.HasDrawableWeapon)
        {
            RpgAnimParams.ApplyRelaxMode(animator, instant);
            return;
        }

        var primary = loadout.primaryHand;
        if (loadout.gripMode == CombatGripMode.TwoHanded)
        {
            ApplyTwoHanded(animator, primary.category, instant);
            return;
        }

        if (loadout.gripMode == CombatGripMode.DualWield)
        {
            ApplyDualWield(animator, primary.category, loadout.offHand ? loadout.offHand.category : primary.category, instant);
            return;
        }

        if (loadout.gripMode == CombatGripMode.OneHandPlusOffHand)
        {
            ApplyOneHandPlusOffHand(animator, primary.category, loadout.offHand.category, instant);
            return;
        }

        ApplyOneHanded(animator, primary.category, instant);
    }

    public static void BeginUnsheathFromRelax(Animator animator, ResolvedCombatLoadout loadout)
    {
        if (!animator || loadout == null || !loadout.HasDrawableWeapon)
            return;

        var stance = ResolveStance(loadout);
        var useRpg = CombatAnimControllerCatalog.EnsureController(animator, stance);

        if (!useRpg)
        {
            WarriorAnimParams.BeginUnsheath(animator);
            return;
        }

        var primary = loadout.primaryHand;
        if (primary && primary.category == WeaponCategory.LongGun)
        {
            RpgAnimParams.BeginUnsheathRifleFromRelax(animator);
            return;
        }

        if (primary && primary.category == WeaponCategory.ShortGun && loadout.offHand == null)
        {
            RpgAnimParams.BeginUnsheathRightPistolFromRelax(animator);
            return;
        }

        if (CanUseSheathAnimation(loadout))
        {
            RpgAnimParams.BeginUnsheathRightSwordFromRelax(animator);
            return;
        }

        RpgAnimParams.SetSheathLocationBack(animator);
        ApplyCombatLoadout(animator, loadout, instant: true);
    }

    public static void BeginSheathToRelax(Animator animator, ResolvedCombatLoadout loadout)
    {
        if (!animator)
            return;

        var stance = loadout != null ? ResolveStance(loadout) : CombatMoveStance.MartialArts;
        var useRpg = CombatAnimControllerCatalog.EnsureController(animator, stance);

        if (!useRpg)
        {
            WarriorAnimParams.BeginSheath(animator);
            return;
        }

        var primary = loadout != null ? loadout.primaryHand : null;
        if (primary && primary.category == WeaponCategory.LongGun)
        {
            RpgAnimParams.BeginSheathRifleToRelax(animator);
            return;
        }

        if (primary && primary.category == WeaponCategory.ShortGun
            && (loadout == null || loadout.offHand == null))
        {
            RpgAnimParams.BeginSheathRightPistolToRelax(animator);
            return;
        }

        if (loadout != null && CanUseSheathAnimation(loadout))
        {
            RpgAnimParams.BeginSheathRightSwordToRelax(animator);
            return;
        }

        RpgAnimParams.ApplyRelaxMode(animator, true);
    }

    public static void FinalizeCombat(Animator animator, ResolvedCombatLoadout loadout)
    {
        EnsureForLoadout(animator, loadout, drawn: true);
    }

    public static void FinalizeRelax(Animator animator, ResolvedCombatLoadout loadout = null)
    {
        if (loadout != null && loadout.HasDrawableWeapon)
        {
            EnsureForLoadout(animator, loadout, drawn: false);
            return;
        }

        CombatAnimControllerCatalog.EnsureController(animator, CombatMoveStance.OneHandSingle);
        RpgAnimParams.FinalizeRelaxAfterSheath(animator);
    }

    static bool CanUseSheathAnimation(ResolvedCombatLoadout loadout)
    {
        // OneHandSingle template: sword / hammer / axe share RPG RightSword sheath + Sword-Attack-R*.
        return loadout.gripMode == CombatGripMode.OneHanded
            && loadout.primaryHand
            && WeaponProficiencyMapper.IsOneHandMelee(loadout.primaryHand.category)
            && loadout.offHand == null;
    }

    static void ApplyTwoHanded(Animator animator, WeaponCategory category, bool instant)
    {
        switch (category)
        {
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandAxe, RpgAnimParams.HandWeaponTwoHandAxe, instant);
                break;
            case WeaponCategory.Spear:
            case WeaponCategory.Staff:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandSpear, RpgAnimParams.HandWeaponTwoHandSpear, instant);
                break;
            case WeaponCategory.Bow:
            case WeaponCategory.Crossbow:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandBow, RpgAnimParams.HandWeaponTwoHandBow, instant);
                break;
            case WeaponCategory.LongGun:
                SetTwoHand(animator, RpgAnimParams.WeaponRifle, RpgAnimParams.HandWeaponRifle, instant);
                break;
            default:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandSword, RpgAnimParams.HandWeaponTwoHandSword, instant);
                break;
        }
    }

    static void ApplyDualWield(Animator animator, WeaponCategory right, WeaponCategory left, bool instant)
    {
        SetArmedPair(animator, MapLeftHandWeapon(left), MapRightHandWeapon(right), RpgAnimParams.SideDual, instant);
    }

    static void ApplyOneHandPlusOffHand(Animator animator, WeaponCategory primary, WeaponCategory offHand, bool instant)
    {
        var right = MapRightHandWeapon(primary);
        var left = MapLeftHandWeapon(offHand);
        SetArmedPair(animator, left, right, RpgAnimParams.SideRight, instant);
    }

    static void ApplyOneHanded(Animator animator, WeaponCategory category, bool instant)
    {
        if (category == WeaponCategory.Shield)
        {
            SetArmedPair(
                animator,
                RpgAnimParams.HandWeaponShield,
                RpgAnimParams.HandWeaponUnarmed,
                RpgAnimParams.SideLeft,
                instant);
            return;
        }

        // OneHandSingle CSV uses Sword-Attack-R* for sword/hammer/axe — RightWeapon must be Sword
        // so Action 8/9/10 transitions match (Mace Action table is 4/5/6 and would freeze).
        var rightWeapon = WeaponProficiencyMapper.IsOneHandMelee(category)
            ? RpgAnimParams.HandWeaponRightSword
            : MapRightHandWeapon(category);

        SetArmedPair(
            animator,
            RpgAnimParams.HandWeaponUnarmed,
            rightWeapon,
            RpgAnimParams.SideRight,
            instant);
    }

    static int MapRightHandWeapon(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Hammer:
            case WeaponCategory.Axe:
                return RpgAnimParams.HandWeaponRightMace;
            case WeaponCategory.ShortGun:
                return RpgAnimParams.HandWeaponRightPistol;
            case WeaponCategory.Shield:
                return RpgAnimParams.HandWeaponUnarmed;
            default:
                return RpgAnimParams.HandWeaponRightSword;
        }
    }

    static int MapLeftHandWeapon(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Shield:
                return RpgAnimParams.HandWeaponShield;
            case WeaponCategory.ShortGun:
                return RpgAnimParams.HandWeaponLeftPistol;
            case WeaponCategory.Hammer:
            case WeaponCategory.Axe:
                return RpgAnimParams.HandWeaponLeftMace;
            default:
                return RpgAnimParams.HandWeaponLeftSword;
        }
    }

    static void SetTwoHand(Animator animator, int animatorWeapon, int handWeapon, bool instant)
    {
        var already =
            animator.GetInteger(RpgAnimParams.Weapon) == animatorWeapon
            && animator.GetInteger(RpgAnimParams.RightWeapon) == handWeapon;

        animator.SetInteger(RpgAnimParams.Weapon, animatorWeapon);
        animator.SetInteger(RpgAnimParams.WeaponSwitch, animatorWeapon);
        animator.SetInteger(RpgAnimParams.LeftWeapon, RpgAnimParams.HandWeaponUnarmed);
        animator.SetInteger(RpgAnimParams.RightWeapon, handWeapon);
        animator.SetInteger(RpgAnimParams.Side, RpgAnimParams.SideNone);

        // Re-firing InstantSwitch every attack interrupts the attack state → freeze / sink.
        if (instant && !already)
            RpgAnimParams.FireInstantSwitch(animator);
    }

    static void SetArmedPair(Animator animator, int leftWeapon, int rightWeapon, int side, bool instant)
    {
        var already =
            animator.GetInteger(RpgAnimParams.Weapon) == RpgAnimParams.WeaponArmed
            && animator.GetInteger(RpgAnimParams.LeftWeapon) == leftWeapon
            && animator.GetInteger(RpgAnimParams.RightWeapon) == rightWeapon
            && animator.GetInteger(RpgAnimParams.Side) == side;

        animator.SetInteger(RpgAnimParams.Weapon, RpgAnimParams.WeaponArmed);
        animator.SetInteger(RpgAnimParams.WeaponSwitch, RpgAnimParams.WeaponArmed);
        animator.SetInteger(RpgAnimParams.LeftWeapon, leftWeapon);
        animator.SetInteger(RpgAnimParams.RightWeapon, rightWeapon);
        animator.SetInteger(RpgAnimParams.Side, side);

        if (instant && !already)
            RpgAnimParams.FireInstantSwitch(animator);
    }
}
