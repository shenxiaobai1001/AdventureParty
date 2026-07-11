using System.Collections;
using UnityEngine;

/// <summary>
/// Casual (Relax, sword on back) vs Combat (1H right sword) stance. Toggle with E while standing still.
/// </summary>
public class PlayerStanceController : MonoBehaviour
{
    public enum StanceMode
    {
        Casual,
        Combat,
    }

    [Header("Input")]
    [Tooltip("When false, E works without LMB selection (useful for single-hero scenes).")]
    public bool requireSelection = false;

    [Header("Debug")]
    public bool debugLog;

    Animator _animator;
    PlayerController _player;
    HeroWeaponVisual _weaponVisual;
    PlayerActivityController _activity;
    Coroutine _switchTimeoutRoutine;

    StanceMode _targetStance;

    public StanceMode CurrentStance { get; private set; } = StanceMode.Casual;
    public bool IsSwitching { get; private set; }

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _player = GetComponent<PlayerController>();
        _weaponVisual = GetComponent<HeroWeaponVisual>();
        _activity = GetComponent<PlayerActivityController>();
    }

    void Start()
    {
        _weaponVisual?.PlaceWeaponsForCasualStance();
        EnterCasualInstant();
    }

    void Update()
    {
        if (requireSelection && _player && !_player.IsSelected)
            return;

        if (Input.GetKeyDown(KeyCode.E))
            TryToggleStance();
    }

    public bool TryToggleStance()
    {
        if (IsSwitching)
        {
            LogDebug("Blocked: already switching stance.");
            return false;
        }

        if (_player && !_player.IsStandingStill)
        {
            LogDebug("Blocked: must stand still to switch stance.");
            return false;
        }

        if (_activity && _activity.IsResting)
        {
            LogDebug("Blocked: cannot switch stance while resting.");
            return false;
        }

        if (_activity && _activity.IsStealthing)
            _activity.SetStealth(false);

        if (!_weaponVisual)
        {
            LogDebug("Blocked: HeroWeaponVisual missing.");
            return false;
        }

        _weaponVisual.RefreshWeaponDetection();

        if (!_weaponVisual.HasMainWeapon)
        {
            LogDebug("Blocked: no main-hand weapon on back mount socket.");
            return false;
        }

        if (!_animator)
        {
            LogDebug("Warning: Animator missing, switching instantly without animation.");
        }

        if (CurrentStance == StanceMode.Casual)
            BeginEnterCombat();
        else
            BeginEnterCasual();

        LogDebug($"Begin switch: {CurrentStance} -> {_targetStance}");
        return true;
    }

    public void OnWeaponSwitchEvent()
    {
        _weaponVisual?.ApplyPendingAttach();
        CompleteSwitch();
    }

    void CompleteSwitch()
    {
        if (!IsSwitching)
            return;

        if (_switchTimeoutRoutine != null)
        {
            StopCoroutine(_switchTimeoutRoutine);
            _switchTimeoutRoutine = null;
        }

        if (_targetStance == StanceMode.Combat)
            RpgAnimParams.FinalizeCombatRightSword(_animator);
        else
            RpgAnimParams.FinalizeRelaxAfterSheath(_animator);

        CurrentStance = _targetStance;
        IsSwitching = false;
        LogDebug($"Switch complete: now {CurrentStance}");
    }

    void EnterCasualInstant()
    {
        CurrentStance = StanceMode.Casual;
        IsSwitching = false;
        RpgAnimParams.ApplyRelaxMode(_animator, true);
    }

    void BeginEnterCombat()
    {
        _targetStance = StanceMode.Combat;
        IsSwitching = true;
        _player?.CancelMovement();
        _weaponVisual.RequestAttachOnSwitch(HeroWeaponVisual.AttachTarget.Hand);

        if (_animator)
        {
            RpgAnimParams.BeginUnsheathRightSwordFromRelax(_animator);
            _switchTimeoutRoutine = StartCoroutine(SwitchTimeoutRoutine());
        }
        else
        {
            _weaponVisual.ApplyPendingAttach();
            CompleteSwitch();
        }
    }

    void BeginEnterCasual()
    {
        _targetStance = StanceMode.Casual;
        IsSwitching = true;
        _player?.CancelMovement();
        _weaponVisual.RequestAttachOnSwitch(HeroWeaponVisual.AttachTarget.BackMount);

        if (_animator)
        {
            RpgAnimParams.BeginSheathRightSwordToRelax(_animator);
            _switchTimeoutRoutine = StartCoroutine(SwitchTimeoutRoutine());
        }
        else
        {
            _weaponVisual.ApplyPendingAttach();
            CompleteSwitch();
        }
    }

    IEnumerator SwitchTimeoutRoutine()
    {
        var delay = _targetStance == StanceMode.Casual ? 0.65f : 1.2f;
        yield return new WaitForSeconds(delay);

        if (!IsSwitching)
            yield break;

        LogDebug("WeaponSwitch animation event timeout; completing switch anyway.");
        _weaponVisual?.ApplyPendingAttach();
        CompleteSwitch();
    }

    void LogDebug(string message)
    {
        if (debugLog)
            Debug.Log($"[PlayerStance] {message}", this);
    }
}
