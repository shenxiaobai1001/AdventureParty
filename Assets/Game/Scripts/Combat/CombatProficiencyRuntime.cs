using UnityEngine;

/// <summary>
/// Applies configured XP curves to a character's combat progression profile.
/// </summary>
public static class CombatProficiencyRuntime
{
    public static void AddWeaponXp(CombatProficiencyProfile profile, WeaponProficiencyType type, float amount)
    {
        if (profile == null || amount <= 0f)
            return;

        profile.EnsureDefaults();
        var entry = profile.GetOrCreateWeapon(type);
        var xpPerLevel = GetWeaponXpPerLevel(type);
        entry.value = ProficiencyProgression.AddXp(entry.value, amount, xpPerLevel);
    }

    public static void AddAttributeXp(CombatProficiencyProfile profile, BodyAttributeType type, float amount)
    {
        if (profile == null || amount <= 0f)
            return;

        profile.EnsureDefaults();
        var entry = profile.GetOrCreateAttribute(type);
        var xpPerLevel = GetAttributeXpPerLevel(type);
        entry.value = ProficiencyProgression.AddXp(entry.value, amount, xpPerLevel);
    }

    public static void AddFightAttributeXp(CombatProficiencyProfile profile, FightAttributeType type, float amount)
    {
        if (profile == null || amount <= 0f)
            return;

        profile.EnsureDefaults();
        var entry = profile.GetOrCreateFightAttribute(type);
        var xpPerLevel = GetFightAttributeXpPerLevel(type);
        entry.value = ProficiencyProgression.AddXp(entry.value, amount, xpPerLevel);
    }

    public static float GetWeaponXpPerLevel(WeaponProficiencyType type)
    {
        if (WeaponProficiencyConfigData.Instance.TryGet(type, out var row) && row.xpPerLevel > 0f)
            return row.xpPerLevel;

        return ProficiencyProgression.DefaultXpPerLevel;
    }

    public static float GetAttributeXpPerLevel(BodyAttributeType type)
    {
        if (BodyAttributesConfigData.Instance.TryGet(type, out var row) && row.xpPerLevel > 0f)
            return row.xpPerLevel;

        return ProficiencyProgression.DefaultXpPerLevel;
    }

    public static float GetFightAttributeXpPerLevel(FightAttributeType type)
    {
        if (FightAttributesConfigData.Instance.TryGet(type, out var row) && row.xpPerLevel > 0f)
            return row.xpPerLevel;

        return ProficiencyProgression.DefaultXpPerLevel;
    }
}
