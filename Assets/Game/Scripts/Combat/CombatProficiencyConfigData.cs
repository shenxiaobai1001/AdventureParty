using System.Collections.Generic;
using UnityEngine;

public class WeaponProficiencyConfigData : Singleton<WeaponProficiencyConfigData>
{
    public ConfigTable<WeaponProficiencyConfigRow> rows = new ConfigTable<WeaponProficiencyConfigRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("WeaponProficiencyConfig.csv");
        return _loaded;
    }

    public IReadOnlyList<WeaponProficiencyConfigRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }

    public bool TryGet(WeaponProficiencyType type, out WeaponProficiencyConfigRow row)
    {
        EnsureLoaded();
        foreach (var entry in rows.GetListInfo())
        {
            if (entry.GetProficiencyType() == type)
            {
                row = entry;
                return true;
            }
        }

        row = null;
        return false;
    }
}

/// <summary>体质属性配置（力量/韧性/灵巧/精准）。</summary>
public class BodyAttributesConfigData : Singleton<BodyAttributesConfigData>
{
    public ConfigTable<BodyAttributeConfigRow> rows = new ConfigTable<BodyAttributeConfigRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        // Prefer new filename; fall back to legacy CombatAttributesConfig.csv
        _loaded = rows.Load("BodyAttributesConfig.csv");
        if (!_loaded || rows.GetListInfo().Count == 0)
            _loaded = rows.Load("CombatAttributesConfig.csv");

        return _loaded;
    }

    public IReadOnlyList<BodyAttributeConfigRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }

    public bool TryGet(BodyAttributeType type, out BodyAttributeConfigRow row)
    {
        EnsureLoaded();
        foreach (var entry in rows.GetListInfo())
        {
            if (entry.GetAttributeType() == type)
            {
                row = entry;
                return true;
            }
        }

        row = null;
        return false;
    }
}

/// <summary>战斗属性配置（攻击/防御/感知）。</summary>
public class FightAttributesConfigData : Singleton<FightAttributesConfigData>
{
    public ConfigTable<FightAttributeConfigRow> rows = new ConfigTable<FightAttributeConfigRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("FightAttributesConfig.csv");
        return _loaded;
    }

    public IReadOnlyList<FightAttributeConfigRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }

    public bool TryGet(FightAttributeType type, out FightAttributeConfigRow row)
    {
        EnsureLoaded();
        foreach (var entry in rows.GetListInfo())
        {
            if (entry.GetAttributeType() == type)
            {
                row = entry;
                return true;
            }
        }

        row = null;
        return false;
    }
}

public class WeaponProficiencyGainConfigData : Singleton<WeaponProficiencyGainConfigData>
{
    public ConfigTable<WeaponProficiencyGainRow> rows = new ConfigTable<WeaponProficiencyGainRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("WeaponProficiencyGainConfig.csv");
        return _loaded;
    }

    public IReadOnlyList<WeaponProficiencyGainRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }
}

public class BodyAttributeGainConfigData : Singleton<BodyAttributeGainConfigData>
{
    public ConfigTable<BodyAttributeGainRow> rows = new ConfigTable<BodyAttributeGainRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("BodyAttributeGainConfig.csv");
        if (!_loaded || rows.GetListInfo().Count == 0)
            _loaded = rows.Load("CombatAttributeGainConfig.csv");

        return _loaded;
    }

    public IReadOnlyList<BodyAttributeGainRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }
}

public class FightAttributeGainConfigData : Singleton<FightAttributeGainConfigData>
{
    public ConfigTable<FightAttributeGainRow> rows = new ConfigTable<FightAttributeGainRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("FightAttributeGainConfig.csv");
        return _loaded;
    }

    public IReadOnlyList<FightAttributeGainRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }
}

public class WeaponProficiencyLevelEffectsData : Singleton<WeaponProficiencyLevelEffectsData>
{
    public ConfigTable<WeaponProficiencyLevelEffectRow> rows = new ConfigTable<WeaponProficiencyLevelEffectRow>();

    bool _loaded;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("WeaponProficiencyLevelEffects.csv");
        return _loaded;
    }

    public IReadOnlyList<WeaponProficiencyLevelEffectRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }

    public List<WeaponProficiencyLevelEffectRow> GetFor(WeaponProficiencyType type)
    {
        EnsureLoaded();
        var result = new List<WeaponProficiencyLevelEffectRow>();
        foreach (var row in rows.GetListInfo())
        {
            if (row.GetProficiencyType() == type)
                result.Add(row);
        }

        return result;
    }
}

public class WeaponProficiencyConfigRow : NamedData
{
    public string proficiencyType;
    public float xpPerLevel;
    public float baseDamageMultiplier;
    public float baseWearMultiplier;
    public string description;
    public string trainingHint;
    public string levelEffects;
    public string strengths;
    public string weaknesses;

    public WeaponProficiencyType GetProficiencyType()
    {
        if (System.Enum.TryParse(proficiencyType, true, out WeaponProficiencyType parsed))
            return parsed;

        // Legacy art line name from older CSV / saves.
        if (string.Equals(proficiencyType, "Dagger", System.StringComparison.OrdinalIgnoreCase))
            return WeaponProficiencyType.MartialArts;

        return WeaponProficiencyType.Longsword;
    }
}

public class BodyAttributeConfigRow : NamedData
{
    public string attributeType;
    public float xpPerLevel;
    public float levelScale;
    public string description;
    public string trainingHint;
    public string levelEffects;

    public BodyAttributeType GetAttributeType()
    {
        if (System.Enum.TryParse(attributeType, true, out BodyAttributeType parsed))
            return parsed;

        // Legacy CSV names
        if (string.Equals(attributeType, "Attack", System.StringComparison.OrdinalIgnoreCase))
            return BodyAttributeType.Strength;
        if (string.Equals(attributeType, "Defense", System.StringComparison.OrdinalIgnoreCase))
            return BodyAttributeType.Toughness;

        return BodyAttributeType.Strength;
    }
}

public class FightAttributeConfigRow : NamedData
{
    public string attributeType;
    public float xpPerLevel;
    public float levelScale;
    public string description;
    public string trainingHint;
    public string levelEffects;

    public FightAttributeType GetAttributeType()
    {
        return System.Enum.TryParse(attributeType, true, out FightAttributeType parsed)
            ? parsed
            : FightAttributeType.Offense;
    }
}

public class WeaponProficiencyGainRow : NamedData
{
    public string proficiencyType;
    public string actionKey;
    public float baseXp;
    public float levelScale;
    public string notes;

    public WeaponProficiencyType GetProficiencyType()
    {
        if (System.Enum.TryParse(proficiencyType, true, out WeaponProficiencyType parsed))
            return parsed;

        if (string.Equals(proficiencyType, "Dagger", System.StringComparison.OrdinalIgnoreCase))
            return WeaponProficiencyType.MartialArts;

        return WeaponProficiencyType.Longsword;
    }
}

public class BodyAttributeGainRow : NamedData
{
    public string attributeType;
    public string actionKey;
    public float baseXp;
    public float levelScale;
    public string notes;

    public BodyAttributeType GetAttributeType()
    {
        if (System.Enum.TryParse(attributeType, true, out BodyAttributeType parsed))
            return parsed;

        if (string.Equals(attributeType, "Attack", System.StringComparison.OrdinalIgnoreCase))
            return BodyAttributeType.Strength;
        if (string.Equals(attributeType, "Defense", System.StringComparison.OrdinalIgnoreCase))
            return BodyAttributeType.Toughness;

        return BodyAttributeType.Strength;
    }
}

public class FightAttributeGainRow : NamedData
{
    public string attributeType;
    public string actionKey;
    public float baseXp;
    public float levelScale;
    public string notes;

    public FightAttributeType GetAttributeType()
    {
        return System.Enum.TryParse(attributeType, true, out FightAttributeType parsed)
            ? parsed
            : FightAttributeType.Offense;
    }
}

public class WeaponProficiencyLevelEffectRow : NamedData
{
    public string proficiencyType;
    public string statKey;
    public float perLevel;
    public float softCapLevel;
    public string notes;

    public WeaponProficiencyType GetProficiencyType()
    {
        if (System.Enum.TryParse(proficiencyType, true, out WeaponProficiencyType parsed))
            return parsed;

        if (string.Equals(proficiencyType, "Dagger", System.StringComparison.OrdinalIgnoreCase))
            return WeaponProficiencyType.MartialArts;

        return WeaponProficiencyType.Longsword;
    }
}

/// <summary>
/// Shared XP / level helpers for body attributes, weapon arts, and fight attributes.
/// </summary>
public static class ProficiencyProgression
{
    public const float DefaultXpPerLevel = 100f;
    public const float MaxLevel = 100f;

    public static float GetXpRequiredForNextLevel(float currentLevel, float xpPerLevel)
    {
        var step = xpPerLevel > 0f ? xpPerLevel : DefaultXpPerLevel;
        return step * Mathf.Max(1f, currentLevel);
    }

    public static ProficiencyValue AddXp(ProficiencyValue current, float amount, float xpPerLevel)
    {
        if (amount <= 0f)
            return current;

        var next = current;
        next.xp += amount;

        while (next.level < MaxLevel)
        {
            var required = GetXpRequiredForNextLevel(next.level, xpPerLevel);
            if (next.xp < required)
                break;

            next.xp -= required;
            next.level += 1f;
        }

        if (next.level >= MaxLevel)
        {
            next.level = MaxLevel;
            next.xp = 0f;
        }

        return next;
    }

    public static float GetProgress01(ProficiencyValue current, float xpPerLevel)
    {
        if (current.level >= MaxLevel)
            return 1f;

        var required = GetXpRequiredForNextLevel(current.level, xpPerLevel);
        if (required <= 0f)
            return 0f;

        return Mathf.Clamp01(current.xp / required);
    }
}
