using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Maps combat move stances to animator controllers and applies swaps at runtime.
/// Warrior packs for most melee/bow; RPG for OneHandSingle / firearms / throwing.
/// </summary>
public static class CombatAnimControllerCatalog
{
    public const string RpgControllerPath =
        "Assets/Game/Animation/RPG Character Mecanim Animation Pack/Animation Controller/RPG-Character-Animation-Controller.controller";

    const string WarriorRoot = "Assets/Newanimaton/ExplosiveLLC/";

    static RuntimeAnimatorController _cachedRpg;
    static CombatMoveStance _lastStance = (CombatMoveStance)(-1);

    public static string GetControllerPath(CombatMoveStance stance)
    {
        switch (stance)
        {
            case CombatMoveStance.OneHandSingle:
            case CombatMoveStance.RangedRifle:
            case CombatMoveStance.RangedPistol:
            case CombatMoveStance.RangedThrowing:
            case CombatMoveStance.SharedArmed:
            case CombatMoveStance.SharedUnarmed:
                return RpgControllerPath;

            case CombatMoveStance.GreatSword:
                return WarriorRoot + "GreatSword_2H/Animation Controller/2Handed Warrior Animation Controller.controller";
            case CombatMoveStance.HeavyWeapon2H:
                return WarriorRoot + "Hammer/Animation Controller/Hammer Warrior Animation Controller.controller";
            case CombatMoveStance.Spear:
                return WarriorRoot + "Polearm/Animation Controller/Spearman Warrior Animation Controller.controller";
            case CombatMoveStance.Staff:
                return WarriorRoot + "Staff/Animation Controller/Mage Warrior Animation Controller.controller";
            case CombatMoveStance.SwordShield:
                return WarriorRoot + "SwordShield/Animation Controller/Knight Warrior Animation Controller.controller";
            case CombatMoveStance.DualBlades:
                return WarriorRoot + "DualBlades/Animation Controller/Ninja Warrior Animation Controller.controller";
            case CombatMoveStance.DualHeavy:
                return WarriorRoot + "DualHeavy/Animation Controller/Swordsman Warrior Animation Controller.controller";
            case CombatMoveStance.MartialArts:
                return WarriorRoot + "MartialArts/Animation Controller/Karate Warrior Animation Controller.controller";
            case CombatMoveStance.RangedBow:
                return WarriorRoot + "Bow/Animation Controller/Archer Warrior Animation Controller.controller";
            case CombatMoveStance.RangedCrossbow:
                return WarriorRoot + "Crossbow/Animation Controller/Crossbow Warrior Animation Controller.controller";
            default:
                return RpgControllerPath;
        }
    }

    public static bool UsesRpgController(CombatMoveStance stance)
        => GetControllerPath(stance) == RpgControllerPath;

    public static RuntimeAnimatorController LoadController(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
            return null;

#if UNITY_EDITOR
        var loaded = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(assetPath);
        if (loaded)
            return loaded;
#endif
        Debug.LogWarning($"[CombatAnimControllerCatalog] Failed to load controller: {assetPath}");
        return null;
    }

    public static RuntimeAnimatorController GetRpgController()
    {
        if (!_cachedRpg)
            _cachedRpg = LoadController(RpgControllerPath);
        return _cachedRpg;
    }

    /// <summary>
    /// Ensures the animator uses the controller for this move stance.
    /// Returns true if using RPG family after ensure.
    /// </summary>
    public static bool EnsureController(Animator animator, CombatMoveStance stance)
    {
        if (!animator)
            return true;

        var path = GetControllerPath(stance);
        var controller = LoadController(path);
        if (!controller)
            return UsesRpgController(stance);

        // CharacterController drives movement — attack clips must not sink/slide the root.
        animator.applyRootMotion = false;

        if (animator.runtimeAnimatorController != controller)
        {
            ClearTriggers(animator);
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.Play(0, 0, 0f);
            _lastStance = stance;
            Debug.Log($"[CombatAnim] Switched controller → {stance} ({System.IO.Path.GetFileName(path)})");
        }
        else
        {
            _lastStance = stance;
        }

        return UsesRpgController(stance);
    }

    public static bool TryApplyController(Animator animator, CombatMoveStance stance, RuntimeAnimatorController controller)
    {
        if (!animator || !controller)
            return false;

        if (animator.runtimeAnimatorController == controller)
            return false;

        ClearTriggers(animator);
        animator.runtimeAnimatorController = controller;
        animator.Play(0, 0, 0f);
        _lastStance = stance;
        return true;
    }

    static void ClearTriggers(Animator animator)
    {
        if (!animator || animator.runtimeAnimatorController == null)
            return;

        for (var i = 0; i < animator.parameterCount; i++)
        {
            var p = animator.GetParameter(i);
            if (p.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(p.nameHash);
        }
    }
}
