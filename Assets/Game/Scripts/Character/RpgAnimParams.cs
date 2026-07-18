using UnityEngine;

/// <summary>
/// Hashed animator parameter names for RPG-Character-Animation-Controller.
/// Mirrors RPGCharacterAnims.Lookups.AnimationParameters for game-side use without namespace coupling.
/// </summary>
public static class RpgAnimParams
{
    public static readonly int TriggerNumber = Animator.StringToHash("TriggerNumber");
    public static readonly int Trigger = Animator.StringToHash("Trigger");
    public static readonly int Moving = Animator.StringToHash("Moving");
    public static readonly int Aiming = Animator.StringToHash("Aiming");
    public static readonly int Stunned = Animator.StringToHash("Stunned");
    public static readonly int Swimming = Animator.StringToHash("Swimming");
    public static readonly int Blocking = Animator.StringToHash("Blocking");
    public static readonly int Injured = Animator.StringToHash("Injured");
    public static readonly int Weapon = Animator.StringToHash("Weapon");
    public static readonly int WeaponSwitch = Animator.StringToHash("WeaponSwitch");
    public static readonly int Side = Animator.StringToHash("Side");
    public static readonly int LeftWeapon = Animator.StringToHash("LeftWeapon");
    public static readonly int RightWeapon = Animator.StringToHash("RightWeapon");
    public static readonly int Jumping = Animator.StringToHash("Jumping");
    public static readonly int Action = Animator.StringToHash("Action");
    public static readonly int SheathLocation = Animator.StringToHash("SheathLocation");
    public static readonly int Talking = Animator.StringToHash("Talking");
    public static readonly int VelocityX = Animator.StringToHash("Velocity X");
    public static readonly int VelocityZ = Animator.StringToHash("Velocity Z");
    public static readonly int AimHorizontal = Animator.StringToHash("AimHorizontal");
    public static readonly int AimVertical = Animator.StringToHash("AimVertical");
    public static readonly int BowPull = Animator.StringToHash("BowPull");
    public static readonly int Charge = Animator.StringToHash("Charge");
    public static readonly int AnimationSpeed = Animator.StringToHash("AnimationSpeed");
    public static readonly int Sprint = Animator.StringToHash("Sprint");
    public static readonly int Crouch = Animator.StringToHash("Crouch");
    public static readonly int Idle = Animator.StringToHash("Idle");

    /// <summary>AnimatorWeapon.RELAX — casual / no weapon equipped (Relax sub-state machine).</summary>
    public const int WeaponRelax = -1;

    /// <summary>AnimatorWeapon.UNARMED — combat-ready fists.</summary>
    public const int WeaponUnarmed = 0;

    /// <summary>AnimatorWeapon.ARMED — all one-handed weapons.</summary>
    public const int WeaponArmed = 7;

    /// <summary>AnimatorWeapon.TWOHANDSWORD</summary>
    public const int WeaponTwoHandSword = 1;

    /// <summary>AnimatorWeapon.TWOHANDSPEAR</summary>
    public const int WeaponTwoHandSpear = 2;

    /// <summary>AnimatorWeapon.TWOHANDAXE</summary>
    public const int WeaponTwoHandAxe = 3;

    /// <summary>AnimatorWeapon.TWOHANDBOW</summary>
    public const int WeaponTwoHandBow = 4;

    /// <summary>AnimatorWeapon.RIFLE</summary>
    public const int WeaponRifle = 18;

    /// <summary>Weapon.Relax</summary>
    public const int HandWeaponRelax = -1;

    /// <summary>Weapon.Unarmed</summary>
    public const int HandWeaponUnarmed = 0;

    /// <summary>Weapon.Shield</summary>
    public const int HandWeaponShield = 7;

    /// <summary>Weapon.LeftSword</summary>
    public const int HandWeaponLeftSword = 8;

    /// <summary>Weapon.RightSword</summary>
    public const int HandWeaponRightSword = 9;

    /// <summary>Weapon.LeftMace</summary>
    public const int HandWeaponLeftMace = 10;

    /// <summary>Weapon.RightMace</summary>
    public const int HandWeaponRightMace = 11;

    /// <summary>Weapon.LeftDagger</summary>
    public const int HandWeaponLeftDagger = 12;

    /// <summary>Weapon.RightDagger</summary>
    public const int HandWeaponRightDagger = 13;

    /// <summary>Weapon.LeftPistol</summary>
    public const int HandWeaponLeftPistol = 16;

    /// <summary>Weapon.RightPistol</summary>
    public const int HandWeaponRightPistol = 17;

    /// <summary>Weapon.TwoHandSword</summary>
    public const int HandWeaponTwoHandSword = 1;

    /// <summary>Weapon.TwoHandSpear</summary>
    public const int HandWeaponTwoHandSpear = 2;

    /// <summary>Weapon.TwoHandAxe</summary>
    public const int HandWeaponTwoHandAxe = 3;

    /// <summary>Weapon.TwoHandBow</summary>
    public const int HandWeaponTwoHandBow = 4;

    /// <summary>Weapon.Rifle</summary>
    public const int HandWeaponRifle = 18;

    /// <summary>AnimatorTrigger.WeaponSheathTrigger</summary>
    public const int TriggerWeaponSheath = 15;

    /// <summary>AnimatorTrigger.WeaponUnsheathTrigger</summary>
    public const int TriggerWeaponUnsheath = 16;

    /// <summary>AnimatorTrigger.InstantSwitchTrigger — snap to Weapon mode without sheath clip.</summary>
    public const int TriggerInstantSwitch = 25;

    /// <summary>AnimatorTrigger.ActionTrigger — emotes such as sit / lay.</summary>
    public const int TriggerAction = 2;

    /// <summary>EmoteType.Sit</summary>
    public const int EmoteSit = 0;

    /// <summary>EmoteType.Laydown</summary>
    public const int EmoteLaydown = 1;

    /// <summary>EmoteType.Pickup</summary>
    public const int EmotePickup = 2;

    /// <summary>EmoteType.StandFromSitting</summary>
    public const int EmoteStandFromSitting = 9;

    /// <summary>EmoteType.StandFromLaying</summary>
    public const int EmoteStandFromLaying = 10;

    /// <summary>Back sheath (not hips).</summary>
    public const int SheathLocationBack = 0;

    public const int SideNone = 0;
    public const int SideUnchanged = -1;
    public const int SideLeft = 1;
    public const int SideRight = 2;
    public const int SideDual = 3;

    /// <summary>Normalized forward walk speed for Velocity Z blend trees.</summary>
    public const float WalkBlendSpeed = 0.45f;

    /// <summary>
    /// Enter Relax locomotion (no weapon). Matches RPG pack InstantWeaponSwitch(Weapon.Relax).
    /// </summary>
    public static void ApplyRelaxMode(Animator animator, bool instantSwitch = true)
    {
        if (!animator)
            return;

        animator.SetInteger(Weapon, WeaponRelax);
        animator.SetInteger(LeftWeapon, HandWeaponUnarmed);
        animator.SetInteger(RightWeapon, HandWeaponUnarmed);
        animator.SetInteger(Side, 0);

        if (instantSwitch)
        {
            animator.SetInteger(TriggerNumber, TriggerInstantSwitch);
            animator.SetTrigger(Trigger);
        }
    }

    /// <summary>Normalized forward run speed for Velocity Z blend trees.</summary>
    public const float RunBlendSpeed = 1f;

    public static void SetSheathLocationBack(Animator animator)
    {
        if (!animator)
            return;

        animator.SetInteger(SheathLocation, SheathLocationBack);
    }

    static void FireAnimatorTrigger(Animator animator, int triggerNumber)
    {
        animator.SetInteger(TriggerNumber, triggerNumber);
        animator.SetTrigger(Trigger);
    }

    public static void FireInstantSwitch(Animator animator)
    {
        if (!animator)
            return;

        FireAnimatorTrigger(animator, TriggerInstantSwitch);
    }

    /// <summary>Matches RPG pack unsheath RightSword from Relax with back sheath.</summary>
    public static void BeginUnsheathRightSwordFromRelax(Animator animator)
    {
        if (!animator)
            return;

        SetSheathLocationBack(animator);
        animator.SetInteger(WeaponSwitch, WeaponArmed);
        animator.SetInteger(Weapon, WeaponRelax);
        animator.SetInteger(Side, SideRight);
        FireAnimatorTrigger(animator, TriggerWeaponUnsheath);
    }

    /// <summary>Matches RPG pack sheath RightSword back to Relax.</summary>
    public static void BeginSheathRightSwordToRelax(Animator animator)
    {
        if (!animator)
            return;

        SetSheathLocationBack(animator);
        animator.SetInteger(WeaponSwitch, WeaponRelax);
        animator.SetInteger(Weapon, WeaponArmed);
        animator.SetInteger(RightWeapon, HandWeaponRightSword);
        animator.SetInteger(Side, SideRight);
        FireAnimatorTrigger(animator, TriggerWeaponSheath);
    }

    public static void FinalizeCombatRightSword(Animator animator)
    {
        if (!animator)
            return;

        animator.SetInteger(Weapon, WeaponArmed);
        animator.SetInteger(WeaponSwitch, WeaponArmed);
        animator.SetInteger(LeftWeapon, HandWeaponUnarmed);
        animator.SetInteger(RightWeapon, HandWeaponRightSword);
        animator.SetInteger(Side, SideRight);
    }

    public static void FinalizeRelaxAfterSheath(Animator animator)
    {
        if (!animator)
            return;

        animator.SetInteger(Weapon, WeaponRelax);
        animator.SetInteger(LeftWeapon, HandWeaponUnarmed);
        animator.SetInteger(RightWeapon, HandWeaponUnarmed);
        animator.SetInteger(Side, SideNone);
    }

    public static void SetCrouching(Animator animator, bool crouching)
    {
        if (!animator)
            return;

        animator.SetBool(Crouch, crouching);
    }

    public static void TriggerEmoteAction(Animator animator, int emoteType)
    {
        if (!animator)
            return;

        animator.SetInteger(Action, emoteType);
        FireAnimatorTrigger(animator, TriggerAction);
    }

    public static void TriggerPickup(Animator animator)
    {
        TriggerEmoteAction(animator, EmotePickup);
    }
}
