using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Opens a melee strike window on anim Hit events and sweeps active weapon/limb capsules.
/// Also applies a locked-target proximity assist for UI-directed attacks.
/// </summary>
public class MeleeAttackController : MonoBehaviour
{
    [Header("Window")]
    [Tooltip("How long each Hit event keeps the sweep active.")]
    public float hitWindowDuration = 0.22f;

    [Tooltip("Samples along the motion between last frame and this frame.")]
    [Range(2, 8)]
    public int sweepSamples = 5;

    [Header("Locked target assist")]
    [Tooltip("If CombatEngageService has a target, hit when blade/chest are within this range.")]
    public float lockedTargetReach = 2.2f;

    [Header("Damage (MVP)")]
    public float baseDamage = 12f;
    public float strengthScale = 1.2f;
    public float proficiencyScale = 1.5f;
    public float toughnessScale = 0.8f;

    [Header("Debug")]
    public bool debugLog = true;
    public bool debugDraw = true;

    HeroWeaponVisual _weapons;
    HeroCombatProficiency _proficiency;
    MartialArtsLimbHitboxes _limbs;
    readonly HashSet<CombatHealth> _hitThisWindow = new HashSet<CombatHealth>();
    readonly Collider[] _overlapBuffer = new Collider[32];
    readonly List<MeleeSweepSource> _sources = new List<MeleeSweepSource>(8);

    float _windowEndsAt = -1f;
    int _strikeId;
    int _lastHitFrame = -1;
    bool _loggedMissThisWindow;

    public bool IsWindowOpen => Time.time <= _windowEndsAt;

    void Awake()
    {
        _weapons = GetComponent<HeroWeaponVisual>();
        _proficiency = GetComponent<HeroCombatProficiency>();
        _limbs = GetComponent<MartialArtsLimbHitboxes>();
        if (!_limbs)
            _limbs = gameObject.AddComponent<MartialArtsLimbHitboxes>();
    }

    void FixedUpdate()
    {
        if (!IsWindowOpen)
            return;

        SweepActiveSources();
        TryHitLockedTarget();
    }

    /// <summary>Called from Warrior / RPG animation Hit events.</summary>
    public void NotifyHitEvent()
    {
        if (_lastHitFrame == Time.frameCount)
            return;
        _lastHitFrame = Time.frameCount;

        _strikeId++;
        _hitThisWindow.Clear();
        _loggedMissThisWindow = false;
        _windowEndsAt = Time.time + hitWindowDuration;

        CollectActiveSources(_sources);
        for (var i = 0; i < _sources.Count; i++)
            _sources[i]?.ResetPrevious();

        if (debugLog)
            Debug.Log($"[MeleeAttack] {name} Hit window #{_strikeId} ({_sources.Count} sources) for {hitWindowDuration:0.###}s");

        SweepActiveSources();
        TryHitLockedTarget();
    }

    void SweepActiveSources()
    {
        CollectActiveSources(_sources);
        for (var i = 0; i < _sources.Count; i++)
        {
            var source = _sources[i];
            if (!source || !source.contributesToMelee)
                continue;

            SweepSource(source);
            source.CapturePrevious();
        }
    }

    void CollectActiveSources(List<MeleeSweepSource> into)
    {
        into.Clear();

        if (_weapons)
            _weapons.CollectDrawnMeleeSweepSources(into);

        if (into.Count == 0 && _limbs)
            _limbs.CollectActiveSources(into);
    }

    void SweepSource(MeleeSweepSource source)
    {
        source.TryGetSweepSegment(out var rootA, out var tipA, out var rootB, out var tipB);
        var samples = Mathf.Max(2, sweepSamples);
        // CombatHurt + Default (in case hurtbox layer failed to assign).
        var mask = CombatLayers.HurtMask;
        var defaultLayer = LayerMask.NameToLayer("Default");
        if (defaultLayer >= 0)
            mask |= 1 << defaultLayer;

        var radius = Mathf.Max(0.12f, source.radius);

        for (var s = 0; s < samples; s++)
        {
            var t = samples == 1 ? 1f : s / (float)(samples - 1);
            var root = Vector3.Lerp(rootA, rootB, t);
            var tip = Vector3.Lerp(tipA, tipB, t);
            if ((tip - root).sqrMagnitude < 0.0001f)
                tip = root + transform.forward * 0.35f;

            var count = Physics.OverlapCapsuleNonAlloc(
                root, tip, radius, _overlapBuffer, mask, QueryTriggerInteraction.Collide);

            for (var i = 0; i < count; i++)
                TryRegisterColliderHit(_overlapBuffer[i], Vector3.Lerp(root, tip, 0.5f));

            if (debugDraw)
                Debug.DrawLine(root, tip, Color.red, 0.08f);
        }
    }

    /// <summary>
    /// UI-directed attacks: if we have a CurrentTarget, hit when close enough during the window.
    /// Covers cases where auto-fit blade capsule misses the body.
    /// </summary>
    void TryHitLockedTarget()
    {
        var health = CombatEngageService.GetTargetHealth();
        if (!health || health.IsDead)
            return;

        if (health.transform == transform)
            return;

        if (!CanDamage(health.gameObject))
            return;

        if (_hitThisWindow.Contains(health))
            return;

        var chest = health.transform.position + Vector3.up * 0.9f;
        var best = float.MaxValue;

        CollectActiveSources(_sources);
        if (_sources.Count == 0)
        {
            best = FlatDistance(transform.position, health.transform.position);
        }
        else
        {
            for (var i = 0; i < _sources.Count; i++)
            {
                var src = _sources[i];
                if (!src || !src.contributesToMelee)
                    continue;

                best = Mathf.Min(best, Vector3.Distance(src.TipWorld, chest));
                best = Mathf.Min(best, Vector3.Distance(src.RootWorld, chest));
                best = Mathf.Min(best, Vector3.Distance(Vector3.Lerp(src.RootWorld, src.TipWorld, 0.5f), chest));
            }
        }

        if (best > lockedTargetReach)
        {
            if (debugLog && !_loggedMissThisWindow && Time.time > _windowEndsAt - 0.02f)
            {
                _loggedMissThisWindow = true;
                Debug.Log($"[MeleeAttack] Miss locked target '{health.name}' (nearest={best:0.00}m reach={lockedTargetReach:0.00}m)");
            }

            return;
        }

        RegisterHit(health, chest);
    }

    void TryRegisterColliderHit(Collider col, Vector3 approxPoint)
    {
        if (!col)
            return;

        var hurt = col.GetComponentInParent<CombatHurtbox>();
        var health = hurt && hurt.Health
            ? hurt.Health
            : col.GetComponentInParent<CombatHealth>();

        if (!health)
            return;

        if (health.transform == transform)
            return;

        if (!CanDamage(health.gameObject))
            return;

        RegisterHit(health, approxPoint);
    }

    void RegisterHit(CombatHealth health, Vector3 hitPoint)
    {
        if (!_hitThisWindow.Add(health))
            return;

        var damage = ComputeDamage(health);
        health.ApplyDamage(damage, gameObject, hitPoint);

        if (debugLog)
            Debug.Log($"[MeleeAttack] {name} hit {health.name} for {damage:0.#}");
    }

    static float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    bool CanDamage(GameObject target)
    {
        if (!target)
            return false;

        if (GameTags.IsTeammate(gameObject) && GameTags.IsTeammate(target))
            return false;

        return true;
    }

    float ComputeDamage(CombatHealth defender)
    {
        var strength = _proficiency ? _proficiency.GetAttributeLevel(BodyAttributeType.Strength) : 1f;
        var toughness = 1f;
        var defProf = defender.GetComponent<HeroCombatProficiency>();
        if (defProf)
            toughness = defProf.GetAttributeLevel(BodyAttributeType.Toughness);

        var weaponLevel = 1f;
        if (_weapons && _weapons.equippedRight
            && !MeleeSweepDefaults.IsRangedOrNonMelee(_weapons.equippedRight.category))
        {
            weaponLevel = _proficiency
                ? _proficiency.GetWeaponLevel(_weapons.equippedRight.proficiencyType)
                : 1f;
        }
        else if (_weapons && _weapons.equippedLeft
                 && !MeleeSweepDefaults.IsRangedOrNonMelee(_weapons.equippedLeft.category))
        {
            weaponLevel = _proficiency
                ? _proficiency.GetWeaponLevel(_weapons.equippedLeft.proficiencyType)
                : 1f;
        }

        var raw = baseDamage + strength * strengthScale + weaponLevel * proficiencyScale;
        var mitigated = raw - toughness * toughnessScale;
        return Mathf.Max(1f, mitigated);
    }
}
