using UnityEngine;

/// <summary>
/// Simple HP + hit react + death for combatants.
/// </summary>
public class CombatHealth : MonoBehaviour
{
    [SerializeField] float maxHp = 100f;
    [SerializeField] float currentHp = 100f;
    [SerializeField] bool destroyOnDeath;
    [SerializeField] bool debugLog = true;

    CombatMovePlayer _combat;
    PlayerController _player;
    bool _dead;

    public float MaxHp => maxHp;
    public float CurrentHp => currentHp;
    public bool IsDead => _dead;

    public event System.Action<CombatHealth, float, GameObject> Damaged;
    public event System.Action<CombatHealth, GameObject> Died;

    void Awake()
    {
        _combat = GetComponent<CombatMovePlayer>();
        _player = GetComponent<PlayerController>();
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);

        if (!GetComponent<CombatHurtbox>())
            gameObject.AddComponent<CombatHurtbox>();

        if (!GetComponent<MeleeAttackController>())
            gameObject.AddComponent<MeleeAttackController>();

        if (!GetComponent<MartialArtsLimbHitboxes>())
            gameObject.AddComponent<MartialArtsLimbHitboxes>();
    }

    public void ResetHp(float? newMax = null)
    {
        if (newMax.HasValue)
            maxHp = Mathf.Max(1f, newMax.Value);
        currentHp = maxHp;
        _dead = false;
    }

    public void ApplyDamage(float amount, GameObject attacker, Vector3 hitPoint)
    {
        if (_dead || amount <= 0f)
            return;

        currentHp = Mathf.Max(0f, currentHp - amount);
        if (debugLog)
            Debug.Log($"[CombatHealth] {name} took {amount:0.#} from {(attacker ? attacker.name : "?")} → HP {currentHp:0.#}/{maxHp:0.#}");

        Damaged?.Invoke(this, amount, attacker);
        PlayHitReact(attacker);

        if (currentHp <= 0f)
            Die(attacker);
    }

    void PlayHitReact(GameObject attacker)
    {
        if (!_combat)
            _combat = GetComponent<CombatMovePlayer>();

        if (_combat)
        {
            _combat.PlayHitReact("react.hit.front");
            return;
        }

        // Fallback: raw animator trigger if mapping unavailable.
        var animator = GetComponentInChildren<Animator>();
        if (animator)
            animator.SetTrigger("Hit");
    }

    void Die(GameObject killer)
    {
        if (_dead)
            return;

        _dead = true;
        if (debugLog)
            Debug.Log($"[CombatHealth] {name} died. Killer={(killer ? killer.name : "?")}");

        Died?.Invoke(this, killer);
        _player?.CancelMovement();

        if (destroyOnDeath)
            Destroy(gameObject);
    }
}
