using UnityEngine;

/// <summary>
/// Applies resolved combat loadouts to RPG Character animator parameters.
/// </summary>
public static class CombatAnimBinding
{
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
            ApplyDualWield(animator, primary.category, instant);
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

        if (loadout != null && CanUseSheathAnimation(loadout))
        {
            RpgAnimParams.BeginSheathRightSwordToRelax(animator);
            return;
        }

        RpgAnimParams.ApplyRelaxMode(animator, true);
    }

    public static void FinalizeCombat(Animator animator, ResolvedCombatLoadout loadout)
    {
        ApplyCombatLoadout(animator, loadout, instant: true);
    }

    public static void FinalizeRelax(Animator animator)
    {
        RpgAnimParams.FinalizeRelaxAfterSheath(animator);
    }

    static bool CanUseSheathAnimation(ResolvedCombatLoadout loadout)
    {
        return loadout.gripMode == CombatGripMode.OneHanded
            && loadout.primaryHand
            && loadout.primaryHand.category == WeaponCategory.Sword1H
            && loadout.offHand == null;
    }

    static void ApplyTwoHanded(Animator animator, WeaponCategory category, bool instant)
    {
        switch (category)
        {
            case WeaponCategory.HeavyWeapon2H:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandAxe, RpgAnimParams.HandWeaponTwoHandAxe, instant);
                break;
            case WeaponCategory.Polearm2H:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandSpear, RpgAnimParams.HandWeaponTwoHandSpear, instant);
                break;
            case WeaponCategory.Bow:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandBow, RpgAnimParams.HandWeaponTwoHandBow, instant);
                break;
            case WeaponCategory.FirearmRifle:
                SetTwoHand(animator, RpgAnimParams.WeaponRifle, RpgAnimParams.HandWeaponRifle, instant);
                break;
            default:
                SetTwoHand(animator, RpgAnimParams.WeaponTwoHandSword, RpgAnimParams.HandWeaponTwoHandSword, instant);
                break;
        }
    }

    static void ApplyDualWield(Animator animator, WeaponCategory category, bool instant)
    {
        switch (category)
        {
            case WeaponCategory.Hammer1H:
                SetArmedPair(animator, RpgAnimParams.HandWeaponLeftMace, RpgAnimParams.HandWeaponRightMace, RpgAnimParams.SideDual, instant);
                break;
            case WeaponCategory.Dagger1H:
                SetArmedPair(animator, RpgAnimParams.HandWeaponLeftDagger, RpgAnimParams.HandWeaponRightDagger, RpgAnimParams.SideDual, instant);
                break;
            default:
                SetArmedPair(animator, RpgAnimParams.HandWeaponLeftSword, RpgAnimParams.HandWeaponRightSword, RpgAnimParams.SideDual, instant);
                break;
        }
    }

    static void ApplyOneHandPlusOffHand(Animator animator, WeaponCategory primary, WeaponCategory offHand, bool instant)
    {
        var right = MapRightHandWeapon(primary);
        var left = MapLeftHandWeapon(offHand);
        SetArmedPair(animator, left, right, RpgAnimParams.SideRight, instant);
    }

    static void ApplyOneHanded(Animator animator, WeaponCategory category, bool instant)
    {
        SetArmedPair(
            animator,
            RpgAnimParams.HandWeaponUnarmed,
            MapRightHandWeapon(category),
            RpgAnimParams.SideRight,
            instant);
    }

    static int MapRightHandWeapon(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Hammer1H:
                return RpgAnimParams.HandWeaponRightMace;
            case WeaponCategory.Dagger1H:
                return RpgAnimParams.HandWeaponRightDagger;
            case WeaponCategory.FirearmPistol:
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
            case WeaponCategory.FirearmPistol:
                return RpgAnimParams.HandWeaponLeftPistol;
            case WeaponCategory.Hammer1H:
                return RpgAnimParams.HandWeaponLeftMace;
            case WeaponCategory.Dagger1H:
                return RpgAnimParams.HandWeaponLeftDagger;
            default:
                return RpgAnimParams.HandWeaponLeftSword;
        }
    }

    static void SetTwoHand(Animator animator, int animatorWeapon, int handWeapon, bool instant)
    {
        animator.SetInteger(RpgAnimParams.Weapon, animatorWeapon);
        animator.SetInteger(RpgAnimParams.WeaponSwitch, animatorWeapon);
        animator.SetInteger(RpgAnimParams.LeftWeapon, RpgAnimParams.HandWeaponUnarmed);
        animator.SetInteger(RpgAnimParams.RightWeapon, handWeapon);
        animator.SetInteger(RpgAnimParams.Side, RpgAnimParams.SideNone);

        if (instant)
            RpgAnimParams.FireInstantSwitch(animator);
    }

    static void SetArmedPair(Animator animator, int leftWeapon, int rightWeapon, int side, bool instant)
    {
        animator.SetInteger(RpgAnimParams.Weapon, RpgAnimParams.WeaponArmed);
        animator.SetInteger(RpgAnimParams.WeaponSwitch, RpgAnimParams.WeaponArmed);
        animator.SetInteger(RpgAnimParams.LeftWeapon, leftWeapon);
        animator.SetInteger(RpgAnimParams.RightWeapon, rightWeapon);
        animator.SetInteger(RpgAnimParams.Side, side);

        if (instant)
            RpgAnimParams.FireInstantSwitch(animator);
    }
}
