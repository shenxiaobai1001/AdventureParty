using System.Collections;
using UnityEngine;

/// <summary>
/// Stationary NPC combat simulator. Starts when the player engages this target.
/// Periodically telegraphs <see cref="CombatSimAttackSignal"/> at the player's <see cref="CombatBrain"/>.
/// Does not move — later real NPC AI can reuse CombatBrain instead of this emitter.
/// </summary>
public class CombatSimOpponent : MonoBehaviour
{
    [Header("Difficulty")]
    public CombatSimStrength strength = CombatSimStrength.Normal;
    public WeaponProficiencyType simulatedWeapon = WeaponProficiencyType.Sword;

    [Header("Timing")]
    [Tooltip("Delay before first signal after session start.")]
    public float firstSignalDelay = 0.8f;
    [Tooltip("Seconds between resolved attacks.")]
    public float attackInterval = 1.6f;
    [Tooltip("Telegraph length (player react window).")]
    public float telegraphSeconds = 0.55f;
    [Tooltip("Portion of telegraph during which defend/dodge can still be chosen.")]
    public float reactWindowSeconds = 0.45f;

    [Header("Session")]
    public bool autoStartOnPlayerEngage = true;
    public bool debugLog = true;

    CombatHealth _health;
    CombatBrain _playerBrain;
    Coroutine _loop;
    bool _sessionActive;

    public bool IsSessionActive => _sessionActive;
    public CombatSimStats CurrentStats => CombatSimStats.FromStrength(strength, simulatedWeapon);

    void Awake()
    {
        _health = GetComponent<CombatHealth>();
        if (!_health)
            _health = gameObject.AddComponent<CombatHealth>();
    }

    /// <summary>Called when player begins an attack engage on this NPC.</summary>
    public void BeginSession(PlayerController player)
    {
        if (!player)
            return;

        _playerBrain = player.GetComponent<CombatBrain>();
        if (!_playerBrain)
            _playerBrain = player.gameObject.AddComponent<CombatBrain>();

        CombatEngageService.EnsureCombatant(player.gameObject);
        CombatEngageService.EnsureCombatant(gameObject);

        if (_loop != null)
            StopCoroutine(_loop);

        _sessionActive = true;
        _loop = StartCoroutine(SessionLoop(player));

        if (debugLog)
            Debug.Log($"[CombatSim] Session start vs '{player.name}' strength={strength}");
    }

    public void EndSession()
    {
        _sessionActive = false;
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }

        if (debugLog)
            Debug.Log($"[CombatSim] Session end on '{name}'");
    }

    IEnumerator SessionLoop(PlayerController player)
    {
        var engageWeapon = player.GetComponent<HeroCombatProficiency>()?.GetEquippedWeaponType()
                           ?? WeaponProficiencyType.Sword;
        CombatSimXp.AwardEngage(player.GetComponent<HeroCombatProficiency>(), engageWeapon);

        yield return new WaitForSeconds(firstSignalDelay);

        while (_sessionActive && player && (!_health || !_health.IsDead))
        {
            var brain = player.GetComponent<CombatBrain>();
            if (!brain)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            // Wait until brain is free to receive a new telegraph.
            while (_sessionActive && brain.HasPendingSignal)
                yield return null;

            if (!_sessionActive)
                yield break;

            var stats = CurrentStats;
            var signal = CombatSimAttackSignal.Create(this, stats, telegraphSeconds, reactWindowSeconds);
            brain.ReceiveAttackSignal(signal);

            if (debugLog)
                Debug.Log($"[CombatSim] Signal → dmg={signal.rawDamage:0.#} telegraph={telegraphSeconds:0.##}s stats Wpn={stats.weaponLevel:0.#} Off={stats.offense:0.#}");

            // Wait until resolve completes (brain clears pending).
            var waitUntil = Time.time + telegraphSeconds + 0.35f;
            while (_sessionActive && Time.time < waitUntil && brain.HasPendingSignal)
                yield return null;

            // Extra beat so player brain can sneak an attack in.
            yield return new WaitForSeconds(attackInterval);
        }

        _sessionActive = false;
        _loop = null;
    }
}
