/// <summary>
/// Weapon proficiency lines. Equippable lines match <see cref="WeaponCategory"/>.
/// MartialArts and Throwing are stance/proficiency-only (no inventory folder).
/// Short blades share <see cref="Sword"/> — there is no separate dagger art.
/// </summary>
public enum WeaponProficiencyType
{
    Sword = 0,
    GreatSword = 1,
    Hammer = 2,
    GreatHammer = 3,
    Axe = 4,
    GreatAxe = 5,
    /// <summary>Reserved (legacy). Maps to Sword.</summary>
    ObsoleteDagger = 6,
    Spear = 7,
    Staff = 8,
    Shield = 9,
    Bow = 10,
    Crossbow = 11,
    LongGun = 12,
    ShortGun = 13,
    MartialArts = 14,
    Throwing = 15,
}
