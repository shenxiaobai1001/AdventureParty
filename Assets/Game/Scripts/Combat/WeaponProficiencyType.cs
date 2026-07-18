/// <summary>
/// Weapon arts (武器·技艺). Skill lines for mastering weapon families / unarmed fighting.
/// Separate from <see cref="WeaponCategory"/> (inventory footprint / visuals).
/// </summary>
public enum WeaponProficiencyType
{
    GreatSword,
    HeavyWeapon,
    Polearm,
    BowCrossbow,
    Shield,
    Longsword,
    HammerAxe,
    /// <summary>Unarmed / martial arts (空手武术). Replaces the old Dagger art line.</summary>
    MartialArts,
    Firearm,
    Throwing,
}
