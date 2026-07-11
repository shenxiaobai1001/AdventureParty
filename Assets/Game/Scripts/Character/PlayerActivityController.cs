using UnityEngine;

/// <summary>
/// Stealth (crouch) and rest (sit / lay) activities for party heroes in casual stance.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerActivityController : MonoBehaviour
{
    public enum RestPose
    {
        None,
        Sit,
        Lay,
    }

    [Header("Stealth")]
    [Tooltip("Move speed while sneaking. Defaults to half walk speed if zero.")]
    public float crouchWalkSpeed;

    Animator _animator;
    PlayerController _player;
    PlayerStanceController _stance;

    RestPose _restPose = RestPose.None;

    public bool IsStealthing { get; private set; }
    public bool IsResting => _restPose != RestPose.None;
    public RestPose CurrentRestPose => _restPose;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _player = GetComponent<PlayerController>();
        _stance = GetComponent<PlayerStanceController>();
    }

    public float GetCrouchWalkSpeed()
    {
        if (crouchWalkSpeed > 0f)
            return crouchWalkSpeed;

        return _player ? _player.walkSpeed * 0.5f : 1.75f;
    }

    public bool SetStealth(bool enabled)
    {
        if (enabled)
            return TryEnterStealth();

        ExitStealth();
        return true;
    }

    public bool TryToggleStealth()
    {
        if (IsStealthing)
        {
            ExitStealth();
            return true;
        }

        return TryEnterStealth();
    }

    public bool SetRest(bool enabled)
    {
        if (enabled)
            return TryEnterRest();

        if (!IsResting)
            return true;

        ExitRest();
        return true;
    }

    public bool TryToggleRest()
    {
        if (IsResting)
        {
            ExitRest();
            return true;
        }

        return TryEnterRest();
    }

    bool TryEnterStealth()
    {
        if (!CanUseCasualActivity())
            return false;

        if (IsResting)
            ExitRest();

        IsStealthing = true;
        RpgAnimParams.SetCrouching(_animator, true);
        return true;
    }

    void ExitStealth()
    {
        if (!IsStealthing)
            return;

        IsStealthing = false;
        RpgAnimParams.SetCrouching(_animator, false);
    }

    bool TryEnterRest()
    {
        if (!CanUseCasualActivity())
            return false;

        if (_player && !_player.IsStandingStill)
            return false;

        if (IsStealthing)
            ExitStealth();

        _player?.CancelMovement();

        var useSit = Random.value < 0.5f;
        _restPose = useSit ? RestPose.Sit : RestPose.Lay;
        RpgAnimParams.TriggerEmoteAction(
            _animator,
            useSit ? RpgAnimParams.EmoteSit : RpgAnimParams.EmoteLaydown);

        return true;
    }

    void ExitRest()
    {
        if (_restPose == RestPose.None)
            return;

        var standEmote = _restPose == RestPose.Sit
            ? RpgAnimParams.EmoteStandFromSitting
            : RpgAnimParams.EmoteStandFromLaying;

        _restPose = RestPose.None;
        RpgAnimParams.TriggerEmoteAction(_animator, standEmote);
    }

    bool CanUseCasualActivity()
    {
        if (!_stance)
            return true;

        return _stance.CurrentStance == PlayerStanceController.StanceMode.Casual && !_stance.IsSwitching;
    }
}
