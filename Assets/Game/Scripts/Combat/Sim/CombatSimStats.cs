using System;
using UnityEngine;

/// <summary>
/// Snapshot of combat stats used by sim signals and resolution.
/// </summary>
[Serializable]
public struct CombatSimStats
{
    public float strength;
    public float toughness;
    public float agility;
    public float precision;
    public float offense;
    public float defense;
    public float awareness;
    public float weaponLevel;
    public WeaponProficiencyType weaponType;

    public static CombatSimStats FromStrength(CombatSimStrength tier, WeaponProficiencyType weaponType)
    {
        // Levels roughly: Weak≈3, Normal≈8, Strong≈14
        float body;
        float fight;
        float weapon;
        switch (tier)
        {
            case CombatSimStrength.Weak:
                body = 3f;
                fight = 3f;
                weapon = 2f;
                break;
            case CombatSimStrength.Strong:
                body = 14f;
                fight = 13f;
                weapon = 12f;
                break;
            default:
                body = 8f;
                fight = 8f;
                weapon = 7f;
                break;
        }

        return new CombatSimStats
        {
            strength = body,
            toughness = body,
            agility = body * 0.95f,
            precision = body * 0.9f,
            offense = fight,
            defense = fight,
            awareness = fight,
            weaponLevel = weapon,
            weaponType = weaponType,
        };
    }

    public static CombatSimStats FromHero(HeroCombatProficiency hero, WeaponProficiencyType weaponType)
    {
        if (!hero)
            return FromStrength(CombatSimStrength.Normal, weaponType);

        return new CombatSimStats
        {
            strength = hero.GetAttributeLevel(BodyAttributeType.Strength),
            toughness = hero.GetAttributeLevel(BodyAttributeType.Toughness),
            agility = hero.GetAttributeLevel(BodyAttributeType.Agility),
            precision = hero.GetAttributeLevel(BodyAttributeType.Precision),
            offense = hero.GetFightAttributeLevel(FightAttributeType.Offense),
            defense = hero.GetFightAttributeLevel(FightAttributeType.Defense),
            awareness = hero.GetFightAttributeLevel(FightAttributeType.Awareness),
            weaponLevel = hero.GetWeaponLevel(weaponType),
            weaponType = weaponType,
        };
    }

    public float EstimateDamage()
    {
        return 8f + strength * 0.9f + weaponLevel * 1.1f + offense * 0.35f;
    }
}
