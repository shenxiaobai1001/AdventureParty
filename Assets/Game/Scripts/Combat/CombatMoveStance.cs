/// <summary>
/// Combat move template stance. Animation templates key off grip/stance, not weapon art XP.
/// </summary>
public enum CombatMoveStance
{
    /// <summary>Shared Armed dodge / roll / block / kick (§1.5).</summary>
    SharedArmed,

    /// <summary>Shared Unarmed dodge / roll (§1.5).</summary>
    SharedUnarmed,

    GreatSword2H,
    HeavyWeapon2H,
    Polearm2H,

    /// <summary>One 1H weapon (optional shield). Sword / hammer / axe / dagger share this.</summary>
    OneHandSingle,

    /// <summary>Two 1H weapons. Shares Armed block / kick / dodge with SharedArmed.</summary>
    OneHandDual,

    MartialArts,

    /// <summary>Shield overlay: bash + forced block while paired with OneHandSingle.</summary>
    Shield,

    RangedBow,
    RangedCrossbow,
    RangedRifle,
    RangedPistol,
    RangedThrowing,
}
