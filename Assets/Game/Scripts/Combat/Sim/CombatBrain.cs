using System.Collections;
using UnityEngine;

/// <summary>
/// Player (and later NPC) combat decision brain for simulated exchanges.
/// <list type="bullet">
/// <item>Pending attack signal → Defend or Dodge</item>
/// <item>No signal → try Attack</item>
/// </list>
/// Designed as the reusable AI template for future moving NPCs.
/// </summary>
public class CombatBrain : MonoBehaviour
{
    [Header("Auto play")]
    [Tooltip("When true, brain auto-picks Defend/Dodge/Attack. When false, wait for manual keys.")]
    public bool autoDecide = true;

    [Header("Manual override (optional)")]
    public KeyCode defendKey = KeyCode.Mouse1;
    public KeyCode dodgeKey = KeyCode.LeftShift;
    public KeyCode attackKey = KeyCode.Mouse0;
    public bool enableManualKeys;

    [Header("Tuning")]
    public float attackCooldown = 1.1f;
    public float staggerSeconds = 0.7f;
    public float defendDamageFactor = 0.15f;
    public float crushBreakBonus = 0.25f;

    [Header("Debug")]
    public bool debugLog = true;

    CombatSimAttackSignal _pending;
    CombatBrainIntent _committedIntent;
    float _nextAttackAt;
    float _staggerUntil;
    bool _resolving;

    HeroCombatProficiency _proficiency;
    CombatMovePlayer _moves;
    PlayerStanceController _stance;
    CombatHealth _health;
    HeroWeaponVisual _weapons;

    public bool HasPendingSignal => _pending != null && !_pending.IsExpired;
    public CombatSimAttackSignal PendingSignal => _pending;
    public bool IsStaggered => Time.time < _staggerUntil;
    public CombatBrainIntent CommittedIntent => _committedIntent;

    void Awake()
    {
        _proficiency = GetComponent<HeroCombatProficiency>();
        if (!_proficiency)
            _proficiency = gameObject.AddComponent<HeroCombatProficiency>();

        _moves = GetComponent<CombatMovePlayer>();
        _stance = GetComponent<PlayerStanceController>();
        _health = GetComponent<CombatHealth>();
        _weapons = GetComponent<HeroWeaponVisual>();
    }

    void Update()
    {
        if (IsStaggered || _resolving)
            return;

        if (_pending != null)
        {
            if (_pending.IsExpired)
            {
                StartCoroutine(ResolveIncoming(_pending, _committedIntent));
                return;
            }

            if (enableManualKeys)
                PollManualIntent();
            else if (autoDecide && _committedIntent == CombatBrainIntent.None && _pending.IsInReactWindow)
                _committedIntent = ChooseDefenseIntent(_pending);

            return;
        }

        // No incoming signal — try to attack.
        if (IsStaggered)
            return;

        if (enableManualKeys && Input.GetKeyDown(attackKey))
        {
            TryPlayerAttack();
            return;
        }

        if (autoDecide && Time.time >= _nextAttackAt)
            TryPlayerAttack();
    }

    public void ReceiveAttackSignal(CombatSimAttackSignal signal)
    {
        if (signal == null)
            return;

        _pending = signal;
        _committedIntent = CombatBrainIntent.None;

        if (debugLog)
            Debug.Log($"[CombatBrain] {name} received signal dmg={signal.rawDamage:0.#} react={signal.reactWindowSeconds:0.##}s");

        if (autoDecide && !enableManualKeys)
            _committedIntent = ChooseDefenseIntent(signal);
    }

    void PollManualIntent()
    {
        if (Input.GetKeyDown(defendKey))
            _committedIntent = CombatBrainIntent.Defend;
        else if (Input.GetKeyDown(dodgeKey))
            _committedIntent = CombatBrainIntent.Dodge;
    }

    CombatBrainIntent ChooseDefenseIntent(CombatSimAttackSignal signal)
    {
        var defense = _proficiency.GetFightAttributeLevel(FightAttributeType.Defense);
        var awareness = _proficiency.GetFightAttributeLevel(FightAttributeType.Awareness);
        var offense = _proficiency.GetFightAttributeLevel(FightAttributeType.Offense);

        // Prefer reading the telegraph with awareness; otherwise block.
        var dodgeWeight = awareness + _proficiency.GetAttributeLevel(BodyAttributeType.Agility) * 0.5f;
        var defendWeight = defense + _proficiency.GetAttributeLevel(BodyAttributeType.Toughness) * 0.35f;
        // Slight offense bias to "eat and counter" later — still pick a defensive option while signal is up.
        var pickDodge = dodgeWeight + Random.Range(0f, 3f) > defendWeight + Random.Range(0f, 3f);

        var intent = pickDodge ? CombatBrainIntent.Dodge : CombatBrainIntent.Defend;
        if (debugLog)
            Debug.Log($"[CombatBrain] {name} intent={intent} (def={defense:0.#} awa={awareness:0.#} off={offense:0.#})");
        return intent;
    }

    IEnumerator ResolveIncoming(CombatSimAttackSignal signal, CombatBrainIntent intent)
    {
        _resolving = true;
        _pending = null;

        var result = CombatSimResolveResult.None;
        var atk = signal.attackerStats;
        var myDef = _proficiency.GetFightAttributeLevel(FightAttributeType.Defense);
        var myAwa = _proficiency.GetFightAttributeLevel(FightAttributeType.Awareness);
        var myAgi = _proficiency.GetAttributeLevel(BodyAttributeType.Agility);
        var myStr = _proficiency.GetAttributeLevel(BodyAttributeType.Strength);

        if (intent == CombatBrainIntent.Defend)
        {
            PlayDefendPose();
            yield return new WaitForSeconds(0.05f);

            var gap = atk.strength - myStr;
            var crush = gap >= Mathf.Max(4f, myStr * 0.15f);
            var successChance = 0.45f + (myDef - atk.offense) * 0.04f + (myStr - atk.strength) * 0.02f;
            if (crush)
                successChance -= crushBreakBonus;
            successChance = Mathf.Clamp01(successChance);

            if (Random.value <= successChance)
            {
                result = CombatSimResolveResult.DefendSuccess;
                var chip = signal.rawDamage * defendDamageFactor;
                if (chip > 0.5f)
                    _health?.ApplyDamage(chip, signal.source ? signal.source.gameObject : null, transform.position);
                CombatSimXp.AwardDefendSuccess(_proficiency);
                if (_moves)
                    _moves.Play("react.block_hit");
            }
            else
            {
                result = CombatSimResolveResult.DefendFail;
                ApplyIncomingHit(signal, fullDamage: true, stagger: true);
                CombatSimXp.AwardDefendFail(_proficiency);
            }

            EndDefendPose();
        }
        else if (intent == CombatBrainIntent.Dodge)
        {
            var successChance = 0.4f + (myAwa - atk.offense) * 0.035f + (myAgi - atk.agility) * 0.03f;
            successChance = Mathf.Clamp01(successChance);

            if (Random.value <= successChance)
            {
                result = CombatSimResolveResult.DodgeSuccess;
                if (_moves)
                    _moves.Play("melee.dodge.back");
                CombatSimXp.AwardDodgeSuccess(_proficiency);
            }
            else
            {
                result = CombatSimResolveResult.DodgeFail;
                ApplyIncomingHit(signal, fullDamage: true, stagger: true);
                CombatSimXp.AwardDodgeFail(_proficiency);
            }
        }
        else
        {
            // No intent committed — eat the hit.
            result = CombatSimResolveResult.DefendFail;
            ApplyIncomingHit(signal, fullDamage: true, stagger: true);
            CombatSimXp.AwardDefendFail(_proficiency);
        }

        if (debugLog)
            Debug.Log($"[CombatBrain] {name} resolve={result} intent={intent}");

        _committedIntent = CombatBrainIntent.None;
        _resolving = false;
        yield return null;
    }

    void ApplyIncomingHit(CombatSimAttackSignal signal, bool fullDamage, bool stagger)
    {
        var dmg = fullDamage ? signal.rawDamage : signal.rawDamage * defendDamageFactor;
        var src = signal.source ? signal.source.gameObject : null;
        _health?.ApplyDamage(dmg, src, transform.position + Vector3.up);
        if (stagger)
            _staggerUntil = Time.time + staggerSeconds;
    }

    void TryPlayerAttack()
    {
        if (HasPendingSignal || IsStaggered)
            return;

        if (_stance && _stance.IsSwitching)
            return;

        if (_stance && _stance.CurrentStance != PlayerStanceController.StanceMode.Combat)
        {
            _stance.TryToggleStance();
            return;
        }

        if (!_moves)
            _moves = GetComponent<CombatMovePlayer>() ?? gameObject.AddComponent<CombatMovePlayer>();

        var weaponType = _proficiency.GetEquippedWeaponType();
        CombatSimXp.AwardPlayerAttackCommit(_proficiency, weaponType);

        // Prefer live attack toward CurrentTarget; also apply sim damage if target has no live overlap.
        var played = _moves.PlayLightAttack();
        _nextAttackAt = Time.time + attackCooldown;

        var targetHealth = CombatEngageService.GetTargetHealth();
        if (targetHealth && !targetHealth.IsDead)
        {
            // Sim contact assist: guarantee a hit during turn sim if close enough.
            var dist = Vector3.Distance(transform.position, targetHealth.transform.position);
            if (dist <= CombatEngageService.MeleeStopDistance + 0.85f)
            {
                // Live MeleeAttackController may also hit — avoid double-dip by small delay check.
                StartCoroutine(SimAttackConfirm(targetHealth, weaponType, played));
            }
        }

        if (debugLog)
            Debug.Log($"[CombatBrain] {name} attack commit played={played}");
    }

    IEnumerator SimAttackConfirm(CombatHealth target, WeaponProficiencyType weaponType, bool animPlayed)
    {
        yield return new WaitForSeconds(0.22f);
        if (!target || target.IsDead || !animPlayed)
            yield break;

        // Damage stays on live melee sweep / locked-target assist.
        // Sim only guarantees weapon proficiency XP for committing the exchange.
        var sim = target.GetComponent<CombatSimOpponent>();
        if (sim)
            CombatSimXp.AwardWeaponHit(_proficiency, weaponType);
    }

    void PlayDefendPose()
    {
        if (_moves)
            _moves.StartBlock();
    }

    void EndDefendPose()
    {
        if (_moves)
            _moves.EndBlock();
    }
}
