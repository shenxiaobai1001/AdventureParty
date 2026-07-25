using UnityEngine;

/// <summary>
/// Receives animation events from RPG Character clips (Hit, footsteps, weapon switch).
/// Attach to the same GameObject as the Animator.
/// </summary>
public class RpgAnimatorEvents : MonoBehaviour
{
    PlayerStanceController _stance;
    MeleeAttackController _melee;

    void Awake()
    {
        _stance = GetComponentInParent<PlayerStanceController>();
        if (!_stance)
            _stance = transform.root.GetComponentInChildren<PlayerStanceController>();

        _melee = GetComponentInParent<MeleeAttackController>();
        if (!_melee)
            _melee = transform.root.GetComponentInChildren<MeleeAttackController>();
    }

    public void Hit()
    {
        if (!_melee)
            _melee = GetComponentInParent<MeleeAttackController>()
                     ?? transform.root.GetComponentInChildren<MeleeAttackController>();
        _melee?.NotifyHitEvent();
    }

    public void Shoot() { }
    public void FootR() { }
    public void FootL() { }
    public void Land() { }

    public void WeaponSwitch()
    {
        _stance?.OnWeaponSwitchEvent();
    }
}
