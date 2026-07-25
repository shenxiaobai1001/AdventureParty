/// <summary>
/// Combat move animation templates. Derived from left/right loadout, not 1:1 with weapon type.
/// </summary>
public enum CombatMoveStance
{
    SharedArmed,
    SharedUnarmed,

    /// <summary>Single 1H (sword/hammer/axe), left empty — RPG RightSword sheath + Sword-Attack-R* (shared template).</summary>
    OneHandSingle,

    /// <summary>1H + shield — Warrior Knight.</summary>
    SwordShield,

    /// <summary>Two edged 1H (sword/dagger) — Warrior Ninja.</summary>
    DualBlades,

    /// <summary>Dual including hammer/axe — Warrior Swordsman.</summary>
    DualHeavy,

    GreatSword,
    /// <summary>Great hammer / great axe — Warrior Hammer pack.</summary>
    HeavyWeapon2H,
    Spear,
    Staff,
    MartialArts,

    RangedBow,
    RangedCrossbow,
    RangedRifle,
    RangedPistol,
    RangedThrowing,
}
