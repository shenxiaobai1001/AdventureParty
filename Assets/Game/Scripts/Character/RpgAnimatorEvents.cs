using UnityEngine;

/// <summary>
/// Receives animation events from RPG Character clips (Hit, footsteps, weapon switch).
/// Attach to the same GameObject as the Animator.
/// </summary>
public class RpgAnimatorEvents : MonoBehaviour
{
    PlayerStanceController _stance;

    void Awake()
    {
        _stance = GetComponentInParent<PlayerStanceController>();
        if (!_stance)
            _stance = transform.root.GetComponentInChildren<PlayerStanceController>();
    }

    public void Hit() { }
    public void Shoot() { }
    public void FootR() { }
    public void FootL() { }
    public void Land() { }

    public void WeaponSwitch()
    {
        _stance?.OnWeaponSwitchEvent();
    }
}
