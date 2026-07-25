using System.Collections;
using UnityEngine;

/// <summary>
/// Enter combat stance, close to melee range, face target, then light attack.
/// </summary>
public static class CombatEngageService
{
    public const float MeleeStopDistance = 1.35f;
    public const float MeleeMaxChaseDistance = 12f;
    public const float MeleeMoveSpeed = 5f;

    public static Transform CurrentTarget { get; private set; }

    public static void BeginAttack(PlayerController attacker, Transform target)
    {
        if (!attacker || !target)
            return;

        CurrentTarget = target;
        EnsureCombatant(target.gameObject);
        EnsureCombatant(attacker.gameObject);

        attacker.CancelMovement();
        attacker.StartCoroutine(EngageRoutine(attacker, target));
    }

    public static void ClearTarget()
    {
        CurrentTarget = null;
    }

    public static CombatHealth GetTargetHealth()
    {
        if (!CurrentTarget)
            return null;
        return CurrentTarget.GetComponent<CombatHealth>()
               ?? CurrentTarget.GetComponentInParent<CombatHealth>();
    }

    public static void EnsureCombatant(GameObject go)
    {
        if (!go)
            return;

        if (!go.GetComponent<CombatHealth>())
            go.AddComponent<CombatHealth>();
        else
            go.GetComponent<CombatHurtbox>()?.EnsureCollider();
    }

    static IEnumerator EngageRoutine(PlayerController attacker, Transform target)
    {
        var stance = attacker.GetComponent<PlayerStanceController>();
        var combat = attacker.GetComponent<CombatMovePlayer>();
        if (!combat)
            combat = attacker.gameObject.AddComponent<CombatMovePlayer>();

        var cc = attacker.GetComponent<CharacterController>();

        FaceTarget(attacker.transform, target);

        if (stance
            && stance.CurrentStance != PlayerStanceController.StanceMode.Combat
            && !stance.IsSwitching)
        {
            if (!stance.TryToggleStance())
            {
                Debug.LogWarning("[CombatEngage] 无法进入战斗姿态（可能未装备可拔出武器）。");
                yield break;
            }
        }

        var timeout = Time.time + 5f;
        while (stance && (stance.IsSwitching || stance.CurrentStance != PlayerStanceController.StanceMode.Combat))
        {
            if (Time.time > timeout || !target)
            {
                Debug.LogWarning("[CombatEngage] 等待进入战斗姿态超时。");
                yield break;
            }

            FaceTarget(attacker.transform, target);
            yield return null;
        }

        // Close distance — UI attack used to swing in place and miss entirely.
        var chaseDeadline = Time.time + 3.5f;
        while (target && Time.time < chaseDeadline)
        {
            var flat = FlatDelta(attacker.transform.position, target.position);
            var dist = flat.magnitude;
            if (dist <= MeleeStopDistance)
                break;

            if (dist > MeleeMaxChaseDistance)
            {
                Debug.LogWarning($"[CombatEngage] 目标过远 ({dist:0.0}m)，取消追击。");
                yield break;
            }

            FaceTarget(attacker.transform, target);
            var step = flat.normalized * MeleeMoveSpeed;
            if (cc && cc.enabled)
                cc.SimpleMove(step);
            else
                attacker.transform.position += step * Time.deltaTime;

            yield return null;
        }

        if (!target)
            yield break;

        FaceTarget(attacker.transform, target);
        yield return null;

        // Start stationary combat-sim on NPC if present.
        var sim = target.GetComponent<CombatSimOpponent>()
                  ?? target.GetComponentInParent<CombatSimOpponent>();
        if (sim && sim.autoStartOnPlayerEngage)
            sim.BeginSession(attacker);

        if (!combat.PlayLightAttack())
            Debug.LogWarning("[CombatEngage] PlayLightAttack 失败（检查战斗姿态 / 武器招式配置）。");
        else
            Debug.Log($"[CombatEngage] Light attack toward '{target.name}' dist={FlatDelta(attacker.transform.position, target.position).magnitude:0.00}m");
    }

    static Vector3 FlatDelta(Vector3 from, Vector3 to)
    {
        var d = to - from;
        d.y = 0f;
        return d;
    }

    static void FaceTarget(Transform self, Transform target)
    {
        if (!self || !target)
            return;

        var flat = FlatDelta(self.position, target.position);
        if (flat.sqrMagnitude < 0.001f)
            return;

        self.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
    }
}
