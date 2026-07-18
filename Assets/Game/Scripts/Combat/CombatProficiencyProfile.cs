using System;
using System.Collections.Generic;

/// <summary>
/// Per-character combat progression: body attributes, weapon arts, and fight attributes.
/// </summary>
[Serializable]
public class CombatProficiencyProfile
{
    public List<WeaponProficiencyEntry> weaponProficiencies = new List<WeaponProficiencyEntry>();
    public List<BodyAttributeEntry> attributes = new List<BodyAttributeEntry>();
    public List<FightAttributeEntry> fightAttributes = new List<FightAttributeEntry>();

    public CombatProficiencyProfile Clone()
    {
        var clone = new CombatProficiencyProfile();
        foreach (var entry in weaponProficiencies)
            clone.weaponProficiencies.Add(entry.Clone());

        foreach (var entry in attributes)
            clone.attributes.Add(entry.Clone());

        foreach (var entry in fightAttributes)
            clone.fightAttributes.Add(entry.Clone());

        return clone;
    }

    public void EnsureDefaults()
    {
        EnsureWeaponDefaults();
        EnsureAttributeDefaults();
        EnsureFightAttributeDefaults();
        MigrateLegacyBodyAttributeNames();
    }

    public WeaponProficiencyEntry GetOrCreateWeapon(WeaponProficiencyType type)
    {
        EnsureWeaponDefaults();
        foreach (var entry in weaponProficiencies)
        {
            if (entry.type == type)
                return entry;
        }

        var created = WeaponProficiencyEntry.CreateDefault(type);
        weaponProficiencies.Add(created);
        return created;
    }

    public BodyAttributeEntry GetOrCreateAttribute(BodyAttributeType type)
    {
        EnsureAttributeDefaults();
        foreach (var entry in attributes)
        {
            if (entry.type == type)
                return entry;
        }

        var created = BodyAttributeEntry.CreateDefault(type);
        attributes.Add(created);
        return created;
    }

    public FightAttributeEntry GetOrCreateFightAttribute(FightAttributeType type)
    {
        EnsureFightAttributeDefaults();
        foreach (var entry in fightAttributes)
        {
            if (entry.type == type)
                return entry;
        }

        var created = FightAttributeEntry.CreateDefault(type);
        fightAttributes.Add(created);
        return created;
    }

    public float GetWeaponLevel(WeaponProficiencyType type)
    {
        return GetOrCreateWeapon(type).value.level;
    }

    public float GetAttributeLevel(BodyAttributeType type)
    {
        return GetOrCreateAttribute(type).value.level;
    }

    public float GetFightAttributeLevel(FightAttributeType type)
    {
        return GetOrCreateFightAttribute(type).value.level;
    }

    void EnsureWeaponDefaults()
    {
        foreach (WeaponProficiencyType type in Enum.GetValues(typeof(WeaponProficiencyType)))
        {
            if (ContainsWeapon(type))
                continue;

            weaponProficiencies.Add(WeaponProficiencyEntry.CreateDefault(type));
        }
    }

    void EnsureAttributeDefaults()
    {
        foreach (BodyAttributeType type in Enum.GetValues(typeof(BodyAttributeType)))
        {
            if (ContainsAttribute(type))
                continue;

            attributes.Add(BodyAttributeEntry.CreateDefault(type));
        }
    }

    void EnsureFightAttributeDefaults()
    {
        foreach (FightAttributeType type in Enum.GetValues(typeof(FightAttributeType)))
        {
            if (ContainsFightAttribute(type))
                continue;

            fightAttributes.Add(FightAttributeEntry.CreateDefault(type));
        }
    }

    /// <summary>
    /// Older builds stored Attack/Defense as body attrs (ordinal 0/1). Those now map to Strength/Toughness.
    /// No-op when enum already uses Strength/Toughness.
    /// </summary>
    void MigrateLegacyBodyAttributeNames()
    {
        // Kept as intentional hook; Unity serializes enums by int so Attack(0)->Strength(0) is automatic.
    }

    bool ContainsWeapon(WeaponProficiencyType type)
    {
        foreach (var entry in weaponProficiencies)
        {
            if (entry.type == type)
                return true;
        }

        return false;
    }

    bool ContainsAttribute(BodyAttributeType type)
    {
        foreach (var entry in attributes)
        {
            if (entry.type == type)
                return true;
        }

        return false;
    }

    bool ContainsFightAttribute(FightAttributeType type)
    {
        foreach (var entry in fightAttributes)
        {
            if (entry.type == type)
                return true;
        }

        return false;
    }
}

[Serializable]
public class WeaponProficiencyEntry
{
    public WeaponProficiencyType type;
    public ProficiencyValue value = ProficiencyValue.Default;

    public WeaponProficiencyEntry Clone()
    {
        return new WeaponProficiencyEntry
        {
            type = type,
            value = value,
        };
    }

    public static WeaponProficiencyEntry CreateDefault(WeaponProficiencyType type)
    {
        return new WeaponProficiencyEntry { type = type, value = ProficiencyValue.Default };
    }
}

[Serializable]
public class BodyAttributeEntry
{
    public BodyAttributeType type;
    public ProficiencyValue value = ProficiencyValue.Default;

    public BodyAttributeEntry Clone()
    {
        return new BodyAttributeEntry
        {
            type = type,
            value = value,
        };
    }

    public static BodyAttributeEntry CreateDefault(BodyAttributeType type)
    {
        return new BodyAttributeEntry { type = type, value = ProficiencyValue.Default };
    }
}

[Serializable]
public class FightAttributeEntry
{
    public FightAttributeType type;
    public ProficiencyValue value = ProficiencyValue.Default;

    public FightAttributeEntry Clone()
    {
        return new FightAttributeEntry
        {
            type = type,
            value = value,
        };
    }

    public static FightAttributeEntry CreateDefault(FightAttributeType type)
    {
        return new FightAttributeEntry { type = type, value = ProficiencyValue.Default };
    }
}
