using System.Collections;
using UnityEngine;

/// <summary>
/// Plays combat move slots on the hero via RPG animator triggers.
/// Numpad debug bindings (enable <see cref="enableNumpadDebug"/>).
/// </summary>
public class CombatMovePlayer : MonoBehaviour
{
    [Header("Debug")]
    public bool enableNumpadDebug = true;
    public bool debugLog = true;
    [Tooltip("Attacks require E-drawn combat stance (recommended).")]
    public bool requireCombatStance = true;
    public float comboStepDelay = 0.45f;

    [Header("Refs (optional auto)")]
    public Animator animator;
    public HeroWeaponVisual weaponVisual;
    public PlayerStanceController stance;
    public PlayerController player;

    Coroutine _comboRoutine;
    bool _blockingHeld;
    int _attackComboIndex;
    float _attackComboResetAt;
    public float attackComboWindow = 1.2f;
    bool _useRpgController = true;

    void Awake()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();
        if (!weaponVisual)
            weaponVisual = GetComponent<HeroWeaponVisual>();
        if (!stance)
            stance = GetComponent<PlayerStanceController>();
        if (!player)
            player = GetComponent<PlayerController>();

        EnsureWarriorEventBridge();
    }

    void EnsureWarriorEventBridge()
    {
        if (!animator)
            return;

        var host = animator.gameObject;
        if (!host.GetComponent<WarriorAnimEventBridge>())
            host.AddComponent<WarriorAnimEventBridge>();
    }

    void Update()
    {
        if (_attackComboIndex > 0 && Time.time > _attackComboResetAt)
            _attackComboIndex = 0;

        if (!enableNumpadDebug)
            return;

        if (player && !player.IsSelected)
        {
            if (PlayerController.Selected && PlayerController.Selected != player)
                return;
        }

        // Numpad 1 (or top-row 1) light attack combo; 3 dash; 4/6 dodge; 5 block; …
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) PlayLightAttack();
        else if (Input.GetKeyDown(KeyCode.Keypad3)) Play("melee.dash_attack");
        else if (Input.GetKeyDown(KeyCode.Keypad4)) { ResetAttackCombo(); Play("melee.dodge.left"); }
        else if (Input.GetKeyDown(KeyCode.Keypad5)) ToggleBlock();
        else if (Input.GetKeyDown(KeyCode.Keypad6)) { ResetAttackCombo(); Play("melee.dodge.right"); }
        else if (Input.GetKeyDown(KeyCode.Keypad7)) { ResetAttackCombo(); Play("melee.roll.forward"); }
        else if (Input.GetKeyDown(KeyCode.Keypad8)) { ResetAttackCombo(); Play("melee.dodge.back"); }
        else if (Input.GetKeyDown(KeyCode.Keypad0)) Play("react.hit.front");
        else if (Input.GetKeyDown(KeyCode.KeypadPeriod)) Play("react.block_hit");
        else if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.KeypadEquals))
            Play("react.block_break");
        else if (Input.GetKeyDown(KeyCode.KeypadMinus)) EndBlock();
    }

    public bool PlayLightAttack()
    {
        if (_attackComboIndex < 0 || _attackComboIndex > 2)
            _attackComboIndex = 0;

        var slot = _attackComboIndex == 0 ? "melee.attack_1"
            : _attackComboIndex == 1 ? "melee.attack_2"
            : "melee.attack_3";

        if (!Play(slot))
            return false;

        _attackComboIndex = (_attackComboIndex + 1) % 3;
        _attackComboResetAt = Time.time + attackComboWindow;
        return true;
    }

    public void ResetAttackCombo()
    {
        _attackComboIndex = 0;
    }

    /// <summary>
    /// Hit reaction — does not require combat stance (victim may be Casual).
    /// </summary>
    public bool PlayHitReact(string slotId = "react.hit.front")
    {
        if (!animator)
            return false;

        EnsureAnimatorReady();
        player?.CancelMovement();

        if (weaponVisual)
        {
            var loadout = weaponVisual.CurrentLoadout;
            _useRpgController = CombatAnimBinding.EnsureForLoadout(animator, loadout, drawn: stance && stance.CurrentStance == PlayerStanceController.StanceMode.Combat);
        }

        animator.applyRootMotion = false;

        if (!string.IsNullOrEmpty(slotId)
            && weaponVisual
            && CombatMoveSlotConfigData.Instance.TryResolve(weaponVisual.CurrentLoadout, slotId, out var row)
            && !row.IsDisabled())
        {
            var cmd = CombatMoveClipMapper.FromSlotHint(slotId, row.animAsset);
            if (!cmd.ok && !string.IsNullOrEmpty(row.animAsset))
                cmd = CombatMoveClipMapper.FromClipName(row.animAsset);
            if (cmd.ok)
                return ExecuteCommand(cmd);
        }

        // Fallback raw get-hit.
        if (_useRpgController)
            RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerGetHit, 1);
        else
            WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.LightHitTrigger, 1);

        Log($"HitReact fallback ({slotId})");
        return true;
    }

    public bool Play(string slotId)
    {
        if (!animator)
        {
            Log("No Animator.");
            return false;
        }

        if (weaponVisual == null)
        {
            Log("No HeroWeaponVisual.");
            return false;
        }

        if (requireCombatStance
            && stance
            && stance.CurrentStance != PlayerStanceController.StanceMode.Combat)
        {
            Log("Need combat stance (E) first.");
            return false;
        }

        if (stance && stance.IsSwitching)
        {
            Log("Blocked: stance/weapon switch in progress.");
            return false;
        }

        player?.CancelMovement();
        if (animator)
        {
            animator.SetBool(RpgAnimParams.Moving, false);
            animator.SetFloat(RpgAnimParams.VelocityX, 0f);
            animator.SetFloat(RpgAnimParams.VelocityZ, 0f);
            animator.applyRootMotion = false;
        }

        weaponVisual.RefreshWeaponDetection();
        var loadout = weaponVisual.CurrentLoadout;
        _useRpgController = CombatAnimBinding.EnsureForLoadout(animator, loadout, drawn: true);
        animator.applyRootMotion = false;

        if (!CombatMoveSlotConfigData.Instance.TryResolve(loadout, slotId, out var row))
        {
            Log($"Resolve failed: {slotId}");
            return false;
        }

        if (row.IsDisabled())
        {
            Log($"Disabled slot: {slotId}");
            return false;
        }

        if (row.IsUiOnly())
        {
            Log($"UI_ONLY: {slotId}");
            return true;
        }

        if (row.IsCombo())
            return PlayCombo(loadout, row);

        return PlayResolvedRow(loadout, row, slotId);
    }

    public void ToggleBlock()
    {
        if (_blockingHeld)
            EndBlock();
        else
            StartBlock();
    }

    public void StartBlock()
    {
        ResetAttackCombo();
        EnsureAnimatorReady();
        if (_useRpgController)
            RpgAnimParams.StartBlock(animator);
        else
            WarriorAnimParams.StartBlock(animator);
        _blockingHeld = true;
        Log("Block start");
    }

    public void EndBlock()
    {
        if (!animator)
            return;

        if (_useRpgController)
            RpgAnimParams.EndBlock(animator);
        else
            WarriorAnimParams.EndBlock(animator);
        _blockingHeld = false;
        Log("Block end");
    }

    bool PlayCombo(ResolvedCombatLoadout loadout, CombatMoveSlotRow row)
    {
        if (_comboRoutine != null)
            StopCoroutine(_comboRoutine);

        _comboRoutine = StartCoroutine(ComboRoutine(loadout, row));
        return true;
    }

    IEnumerator ComboRoutine(ResolvedCombatLoadout loadout, CombatMoveSlotRow row)
    {
        var steps = row.GetComboSteps();
        Log($"Combo ({steps.Length}): {row.slotId}");
        foreach (var step in steps)
        {
            if (string.IsNullOrEmpty(step))
                continue;

            if (step.StartsWith("slot:", System.StringComparison.OrdinalIgnoreCase))
            {
                var nestedId = step.Substring("slot:".Length);
                if (CombatMoveSlotConfigData.Instance.TryResolve(loadout, nestedId, out var nested))
                    PlayResolvedRow(loadout, nested, nestedId);
                else
                    Log($"Combo missing slot: {nestedId}");
            }
            else
            {
                var cmd = CombatMoveClipMapper.FromClipName(step);
                ExecuteCommand(cmd);
            }

            yield return new WaitForSeconds(comboStepDelay);
        }

        _comboRoutine = null;
    }

    bool PlayResolvedRow(ResolvedCombatLoadout loadout, CombatMoveSlotRow row, string slotId)
    {
        var cmd = CombatMoveClipMapper.FromSlotHint(slotId, row.animAsset);
        if (!cmd.ok && !string.IsNullOrEmpty(row.animAsset))
            cmd = CombatMoveClipMapper.FromClipName(row.animAsset);

        if (cmd.kind == CombatMovePlaybackKind.UiOnly)
        {
            Log($"UI_ONLY: {slotId}");
            return true;
        }

        if (!cmd.ok)
        {
            Log($"Cannot play {slotId}: {cmd.error} (asset={row.animAsset})");
            return false;
        }

        return ExecuteCommand(cmd);
    }

    bool ExecuteCommand(CombatMovePlaybackCommand cmd)
    {
        if (!animator)
            return false;

        if (!_useRpgController)
            return ExecuteWarriorCommand(cmd);

        switch (cmd.kind)
        {
            case CombatMovePlaybackKind.Attack:
                if (cmd.side > 0)
                    animator.SetInteger(RpgAnimParams.Side, cmd.side);
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerAttack, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.AttackDual:
                animator.SetInteger(RpgAnimParams.Side, RpgAnimParams.SideDual);
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerAttackDual, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.AttackKick:
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerAttackKick, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.AttackRanged:
                // RPG pack has no Warrior-style RangeAttack; fall back to Attack.
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerAttack, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.Special:
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerSpecialAttack, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.BlockStart:
                StartBlock();
                return true;

            case CombatMovePlaybackKind.BlockEnd:
                EndBlock();
                return true;

            case CombatMovePlaybackKind.BlockHit:
                animator.SetBool(RpgAnimParams.Blocking, true);
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerGetHit, Mathf.Max(1, cmd.action));
                _blockingHeld = true;
                break;

            case CombatMovePlaybackKind.BlockBreak:
                RpgAnimParams.FireBlockBreak(animator);
                _blockingHeld = false;
                StartCoroutine(ClearBlockSoon());
                break;

            case CombatMovePlaybackKind.Dodge:
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerDodge, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.Roll:
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerRoll, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.GetHit:
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerGetHit, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.Reload:
                RpgAnimParams.FireActionTrigger(animator, RpgAnimParams.TriggerReload, Mathf.Max(1, cmd.action));
                break;

            default:
                Log($"Unhandled kind {cmd.kind}");
                return false;
        }

        Log($"Play RPG {cmd.kind} action={cmd.action} src={cmd.source}");
        return true;
    }

    bool ExecuteWarriorCommand(CombatMovePlaybackCommand cmd)
    {
        var src = cmd.source ?? string.Empty;
        var isMoveAttack = src.IndexOf("MoveAttack", System.StringComparison.OrdinalIgnoreCase) >= 0
            || src.IndexOf("dash_attack", System.StringComparison.OrdinalIgnoreCase) >= 0;

        switch (cmd.kind)
        {
            case CombatMovePlaybackKind.Attack:
            case CombatMovePlaybackKind.AttackDual:
            case CombatMovePlaybackKind.AttackKick:
                if (isMoveAttack)
                    WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.AttackMoveTrigger, Mathf.Max(1, cmd.action));
                else
                    WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.AttackTrigger, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.AttackRanged:
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.AttackRanged, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.Special:
                if (isMoveAttack)
                    WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.AttackMoveTrigger, Mathf.Max(1, cmd.action));
                else
                    WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.AttackSpecialTrigger, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.BlockStart:
                StartBlock();
                return true;

            case CombatMovePlaybackKind.BlockEnd:
                EndBlock();
                return true;

            case CombatMovePlaybackKind.BlockHit:
                animator.SetBool(WarriorAnimParams.Blocking, true);
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.LightHitTrigger, Mathf.Max(1, cmd.action));
                _blockingHeld = true;
                break;

            case CombatMovePlaybackKind.BlockBreak:
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.BlockBreakTrigger);
                _blockingHeld = false;
                StartCoroutine(ClearBlockSoon());
                break;

            case CombatMovePlaybackKind.Dodge:
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.DashTrigger, MapRpgDodgeToWarriorDash(cmd));
                break;

            case CombatMovePlaybackKind.Roll:
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.RollTrigger, MapRpgDodgeToWarriorDash(cmd));
                break;

            case CombatMovePlaybackKind.GetHit:
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.LightHitTrigger, Mathf.Max(1, cmd.action));
                break;

            case CombatMovePlaybackKind.Reload:
                WarriorAnimParams.FireTrigger(animator, WarriorAnimParams.ReloadTrigger, Mathf.Max(1, cmd.action));
                break;

            default:
                Log($"Unhandled Warrior kind {cmd.kind}");
                return false;
        }

        Log($"Play Warrior {cmd.kind} action={cmd.action} src={cmd.source}");
        return true;
    }

    static int MapRpgDodgeToWarriorDash(CombatMovePlaybackCommand cmd)
    {
        var src = cmd.source ?? string.Empty;
        if (src.EndsWith(".left", System.StringComparison.Ordinal))
            return WarriorAnimParams.DashLeft;
        if (src.EndsWith(".right", System.StringComparison.Ordinal))
            return WarriorAnimParams.DashRight;
        if (src.EndsWith(".forward", System.StringComparison.Ordinal))
            return WarriorAnimParams.DashForward;
        return WarriorAnimParams.DashBack;
    }

    IEnumerator ClearBlockSoon()
    {
        yield return new WaitForSeconds(0.05f);
        if (!animator)
            yield break;

        if (_useRpgController)
            RpgAnimParams.EndBlock(animator);
        else
            WarriorAnimParams.EndBlock(animator);
    }

    void EnsureAnimatorReady()
    {
        if (!animator)
            animator = GetComponentInChildren<Animator>();

        if (weaponVisual)
        {
            weaponVisual.RefreshWeaponDetection();
            _useRpgController = CombatAnimBinding.EnsureForLoadout(
                animator, weaponVisual.CurrentLoadout, drawn: true);
        }
    }

    void Log(string message)
    {
        if (debugLog)
            Debug.Log($"[CombatMovePlayer] {message}", this);
    }
}
