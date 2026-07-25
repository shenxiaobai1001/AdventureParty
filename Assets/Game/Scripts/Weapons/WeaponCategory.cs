/// <summary>
/// Inventory weapon type. Same taxonomy as <see cref="WeaponProficiencyType"/>
/// for equippable weapons (weapon = proficiency). MartialArts / Throwing are
/// proficiency-only and are not inventory categories.
/// Short blades (former daggers) use <see cref="Sword"/>.
/// </summary>
public enum WeaponCategory
{
    Sword = 0,
    GreatSword = 1,
    Hammer = 2,
    GreatHammer = 3,
    Axe = 4,
    GreatAxe = 5,
    /// <summary>Reserved (legacy dagger). Do not use — short blades are Sword.</summary>
    ObsoleteDagger = 6,
    Spear = 7,
    Staff = 8,
    Shield = 9,
    Bow = 10,
    Crossbow = 11,
    LongGun = 12,
    ShortGun = 13,
}

public enum WeaponPack
{
    Hero,
    Kingdom,
}

/// <summary>
/// Which hand a weapon may occupy / how it locks the other hand.
/// </summary>
public enum WeaponHandRule
{
    /// <summary>May equip to left or right.</summary>
    OneHand = 0,
    /// <summary>Occupies both hands when equipped (from right).</summary>
    TwoHand = 1,
    /// <summary>Equips to right; left must stay empty.</summary>
    RightLocksLeft = 2,
    /// <summary>Shield — left hand only.</summary>
    LeftOnly = 3,
    /// <summary>Bow — held in left hand; right draws the string (locks right).</summary>
    LeftTwoHand = 4,
}

public enum WeaponHand
{
    Right = 0,
    Left = 1,
}
