using System.Collections;
using UnityEngine;

/// <summary>
/// Casual (Relax) vs Combat stance driven by resolved weapon loadout. Toggle with E (cancels movement first).
/// Also handles in-combat weapon swap: sheath old → unsheath new.
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
    Coroutine _weaponSwapRoutine;

    StanceMode _targetStance;
    bool _waitingSwitchEvent;

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

        _player?.CancelMovement();

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

        if (!_weaponVisual.HasDrawableWeapon)
        {
            LogDebug("Blocked: no drawable weapon equipped.");
            return false;
        }

        if (!_animator)
            LogDebug("Warning: Animator missing, switching instantly without animation.");

        if (CurrentStance == StanceMode.Casual)
            BeginEnterCombat();
        else
            BeginEnterCasual();

        LogDebug($"Begin switch: {CurrentStance} -> {_targetStance}");
        return true;
    }

    /// <summary>
    /// While in combat: play sheath of previous loadout, then unsheath the new one.
    /// While casual: no anim — weapons stay on back.
    /// </summary>
    public void BeginDrawnWeaponSwap(
        SyntyWeaponItemData oldRight,
        SyntyWeaponItemData oldLeft,
        SyntyWeaponItemData newRight,
        SyntyWeaponItemData newLeft)
    {
        if (!_weaponVisual)
            return;

        if (CurrentStance != StanceMode.Combat || !_animator)
        {
            _weaponVisual.ForceSetHands(newRight, newLeft);
            _weaponVisual.PlaceWeaponsForCasualStance();
            return;
        }

        if (IsSwitching)
        {
            LogDebug("Blocked: already switching; applying new hands instantly.");
            _weaponVisual.ForceSetHands(newRight, newLeft);
            _weaponVisual.PlaceWeaponsForCombatStance();
            CombatAnimBinding.FinalizeCombat(_animator, _weaponVisual.CurrentLoadout);
            return;
        }

        if (_weaponSwapRoutine != null)
            StopCoroutine(_weaponSwapRoutine);

        _weaponSwapRoutine = StartCoroutine(WeaponSwapRoutine(oldRight, oldLeft, newRight, newLeft));
    }

    public void OnWeaponSwitchEvent()
    {
        if (_waitingSwitchEvent)
        {
            _weaponVisual?.ApplyPendingAttach();
            _waitingSwitchEvent = false;
            return;
        }

        _weaponVisual?.ApplyPendingAttach();
        CompleteSwitch();
    }

    void EnterCasualInstant()
    {
        CurrentStance = StanceMode.Casual;
        IsSwitching = false;
        var loadout = _weaponVisual != null ? _weaponVisual.CurrentLoadout : ResolvedCombatLoadout.Empty;
        if (loadout.HasDrawableWeapon)
            CombatAnimBinding.EnsureForLoadout(_animator, loadout, drawn: false);
        else
            RpgAnimParams.ApplyRelaxMode(_animator, true);
    }

    void BeginEnterCombat()
    {
        _targetStance = StanceMode.Combat;
        IsSwitching = true;
        _player?.CancelMovement();
        _weaponVisual.RequestAttachOnSwitch(HeroWeaponVisual.AttachTarget.Hand);

        var loadout = _weaponVisual.CurrentLoadout;
        if (_animator)
        {
            CombatAnimBinding.BeginUnsheathFromRelax(_animator, loadout);
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

        var loadout = _weaponVisual.CurrentLoadout;
        if (_animator)
        {
            CombatAnimBinding.BeginSheathToRelax(_animator, loadout);
            _switchTimeoutRoutine = StartCoroutine(SwitchTimeoutRoutine());
        }
        else
        {
            _weaponVisual.ApplyPendingAttach();
            CompleteSwitch();
        }
    }

    IEnumerator WeaponSwapRoutine(
        SyntyWeaponItemData oldRight,
        SyntyWeaponItemData oldLeft,
        SyntyWeaponItemData newRight,
        SyntyWeaponItemData newLeft)
    {
        IsSwitching = true;
        _player?.CancelMovement();
        LogDebug("Weapon swap: sheath old → unsheath new");

        // Show old weapons in hands for sheath anim.
        _weaponVisual.ForceSetHands(oldRight, oldLeft);
        _weaponVisual.PlaceWeaponsForCombatStance();
        var oldLoadout = _weaponVisual.CurrentLoadout;

        _weaponVisual.RequestAttachOnSwitch(HeroWeaponVisual.AttachTarget.BackMount);
        CombatAnimBinding.BeginSheathToRelax(_animator, oldLoadout);
        yield return WaitForSwitchEventOrTimeout(1.15f);
        _weaponVisual.ApplyPendingAttach();

        // Apply new equip and draw.
        _weaponVisual.ForceSetHands(newRight, newLeft);
        if (!newRight && !newLeft)
        {
            CombatAnimBinding.FinalizeRelax(_animator, ResolvedCombatLoadout.Empty);
            CurrentStance = StanceMode.Casual;
            IsSwitching = false;
            _weaponSwapRoutine = null;
            LogDebug("Weapon swap complete: unequipped → casual");
            yield break;
        }

        _weaponVisual.RequestAttachOnSwitch(HeroWeaponVisual.AttachTarget.Hand);
        CombatAnimBinding.BeginUnsheathFromRelax(_animator, _weaponVisual.CurrentLoadout);
        yield return WaitForSwitchEventOrTimeout(1.35f);
        _weaponVisual.ApplyPendingAttach();
        CombatAnimBinding.FinalizeCombat(_animator, _weaponVisual.CurrentLoadout);

        CurrentStance = StanceMode.Combat;
        IsSwitching = false;
        _weaponSwapRoutine = null;
        LogDebug("Weapon swap complete");
    }

    IEnumerator WaitForSwitchEventOrTimeout(float timeout)
    {
        _waitingSwitchEvent = true;
        var elapsed = 0f;
        while (_waitingSwitchEvent && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (_waitingSwitchEvent)
        {
            LogDebug("WeaponSwitch event timeout.");
            _waitingSwitchEvent = false;
        }
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

        var loadout = _weaponVisual != null ? _weaponVisual.CurrentLoadout : ResolvedCombatLoadout.Empty;
        if (_targetStance == StanceMode.Combat)
            CombatAnimBinding.FinalizeCombat(_animator, loadout);
        else
            CombatAnimBinding.FinalizeRelax(_animator, loadout);

        CurrentStance = _targetStance;
        IsSwitching = false;
        LogDebug($"Switch complete: now {CurrentStance}");
    }

    IEnumerator SwitchTimeoutRoutine()
    {
        var delay = _targetStance == StanceMode.Casual ? 1.1f : 1.35f;
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
