using System;
using UnityEngine;

/// <summary>
/// Attack telegraph packet from sim opponent → combat brain.
/// </summary>
[Serializable]
public class CombatSimAttackSignal
{
    public int signalId;
    public CombatSimOpponent source;
    public CombatSimStats attackerStats;

    [Tooltip("Seconds from receive until the hit resolves.")]
    public float telegraphSeconds = 0.55f;

    [Tooltip("How long the player has after telegraph start to commit defend/dodge.")]
    public float reactWindowSeconds = 0.45f;

    public float rawDamage;
    public float issuedAt;
    public float resolvesAt;

    public bool IsExpired => Time.time >= resolvesAt;
    public bool IsInReactWindow => Time.time < issuedAt + reactWindowSeconds && Time.time < resolvesAt;
    public float TimeUntilResolve => Mathf.Max(0f, resolvesAt - Time.time);

    public static CombatSimAttackSignal Create(CombatSimOpponent source, CombatSimStats stats, float telegraph, float reactWindow)
    {
        var now = Time.time;
        return new CombatSimAttackSignal
        {
            signalId = unchecked(Environment.TickCount ^ source.GetInstanceID()),
            source = source,
            attackerStats = stats,
            telegraphSeconds = telegraph,
            reactWindowSeconds = reactWindow,
            rawDamage = stats.EstimateDamage(),
            issuedAt = now,
            resolvesAt = now + telegraph,
        };
    }
}

public enum CombatBrainIntent
{
    None = 0,
    Attack = 1,
    Defend = 2,
    Dodge = 3,
}

public enum CombatSimResolveResult
{
    None = 0,
    AttackHit,
    AttackWhiff,
    DefendSuccess,
    DefendFail,
    DodgeSuccess,
    DodgeFail,
}
