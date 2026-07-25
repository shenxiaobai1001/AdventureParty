using UnityEngine;

/// <summary>
/// Forwards Warrior pack Animation Events (WeaponSwitch, etc.) to game stance / visuals.
/// Add on the same GameObject as the Animator (or a child that receives anim events).
/// </summary>
public class WarriorAnimEventBridge : MonoBehaviour
{
    public PlayerStanceController stance;
    public HeroWeaponVisual weaponVisual;
    MeleeAttackController _melee;

    void Awake()
    {
        if (!stance)
            stance = GetComponentInParent<PlayerStanceController>();
        if (!weaponVisual)
            weaponVisual = GetComponentInParent<HeroWeaponVisual>();
        _melee = GetComponentInParent<MeleeAttackController>();
    }

    /// <summary>Called by Warrior WeaponUnsheath / WeaponSheath clips.</summary>
    public void WeaponSwitch()
    {
        weaponVisual?.ApplyPendingAttach();
        stance?.OnWeaponSwitchEvent();
    }

    public void Hit()
    {
        if (!_melee)
            _melee = GetComponentInParent<MeleeAttackController>();
        _melee?.NotifyHitEvent();
    }

    public void FootR() { }
    public void FootL() { }
    public void Land() { }
    public void Shoot() { }
}
