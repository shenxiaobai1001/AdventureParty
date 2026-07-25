using UnityEngine;

/// <summary>
/// Animator parameters for ExplosiveLLC Warrior Mecanim controllers
/// (TriggerNumber + Trigger pattern, shared across packs).
/// </summary>
public static class WarriorAnimParams
{
    public static readonly int TriggerNumber = Animator.StringToHash("TriggerNumber");
    public static readonly int Trigger = Animator.StringToHash("Trigger");
    public static readonly int Action = Animator.StringToHash("Action");
    public static readonly int Weapons = Animator.StringToHash("Weapons");
    public static readonly int Blocking = Animator.StringToHash("Blocking");
    public static readonly int Moving = Animator.StringToHash("Moving");

    // Matches WarriorAnims.AnimatorTrigger
    public const int JumpTrigger = 1;
    public const int DashTrigger = 3;
    public const int AttackTrigger = 4;
    public const int LightHitTrigger = 8;
    public const int RollTrigger = 9;
    public const int AttackSpecialTrigger = 10;
    public const int AttackMoveTrigger = 11;
    public const int AttackRanged = 12;
    public const int BlockBreakTrigger = 13;
    public const int ReloadTrigger = 14;
    public const int WeaponSwitchTrigger = 15;
    public const int BlockTrigger = 16;

    /// <summary>Dash Action: 1 Forward, 2 Right, 3 Backward, 4 Left.</summary>
    public const int DashForward = 1;
    public const int DashRight = 2;
    public const int DashBack = 3;
    public const int DashLeft = 4;

    public static void FireTrigger(Animator animator, int triggerNumber, int actionNumber = 0)
    {
        if (!animator)
            return;

        if (actionNumber != 0)
            animator.SetInteger(Action, actionNumber);

        animator.SetInteger(TriggerNumber, triggerNumber);
        animator.SetTrigger(Trigger);
    }

    public static void ApplySheathed(Animator animator)
    {
        if (!animator)
            return;

        animator.SetBool(Weapons, false);
        animator.SetBool(Blocking, false);
        animator.SetBool(Moving, false);
    }

    public static void ApplyDrawn(Animator animator)
    {
        if (!animator)
            return;

        animator.SetBool(Weapons, true);
        animator.SetBool(Blocking, false);
    }

    /// <summary>Casual → combat: play WeaponUnsheath (Action=2).</summary>
    public static void BeginUnsheath(Animator animator)
    {
        if (!animator)
            return;

        animator.SetBool(Weapons, true);
        FireTrigger(animator, WeaponSwitchTrigger, 2);
    }

    /// <summary>Combat → casual: play WeaponSheath (Action=1).</summary>
    public static void BeginSheath(Animator animator)
    {
        if (!animator)
            return;

        animator.SetBool(Weapons, false);
        FireTrigger(animator, WeaponSwitchTrigger, 1);
    }

    public static void StartBlock(Animator animator)
    {
        if (!animator)
            return;

        animator.SetBool(Blocking, true);
        FireTrigger(animator, BlockTrigger);
    }

    public static void EndBlock(Animator animator)
    {
        if (!animator)
            return;

        animator.SetBool(Blocking, false);
    }
}
