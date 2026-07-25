using UnityEngine;

/// <summary>
/// Awards weapon / body / fight XP for combat-sim and live hits.
/// </summary>
public static class CombatSimXp
{
    public static void AwardWeaponHit(HeroCombatProficiency hero, WeaponProficiencyType type)
    {
        AwardWeapon(hero, type, "hit", 12f);
        AwardFight(hero, FightAttributeType.Offense, "tradeHit", 10f);
        AwardFight(hero, FightAttributeType.Offense, "initiateAttack", 4f);
    }

    public static void AwardEngage(HeroCombatProficiency hero, WeaponProficiencyType type)
    {
        AwardWeapon(hero, type, "engage", 4f);
    }

    public static void AwardDefendSuccess(HeroCombatProficiency hero)
    {
        AwardFight(hero, FightAttributeType.Defense, "block", 10f);
        AwardBody(hero, BodyAttributeType.Toughness, "hitTaken", 3f);
    }

    public static void AwardDefendFail(HeroCombatProficiency hero)
    {
        AwardBody(hero, BodyAttributeType.Toughness, "hitTaken", 8f);
        AwardFight(hero, FightAttributeType.Defense, "endureCombo", 4f);
    }

    public static void AwardDodgeSuccess(HeroCombatProficiency hero)
    {
        AwardFight(hero, FightAttributeType.Awareness, "evadeOrKite", 12f);
        AwardFight(hero, FightAttributeType.Awareness, "readThreat", 6f);
        AwardBody(hero, BodyAttributeType.Agility, "lightCombat", 7f);
    }

    public static void AwardDodgeFail(HeroCombatProficiency hero)
    {
        AwardBody(hero, BodyAttributeType.Toughness, "hitTaken", 8f);
        AwardFight(hero, FightAttributeType.Awareness, "readThreat", 2f);
    }

    public static void AwardPlayerAttackCommit(HeroCombatProficiency hero, WeaponProficiencyType type)
    {
        AwardWeapon(hero, type, "engage", 4f);
        AwardFight(hero, FightAttributeType.Offense, "initiateAttack", 8f);
        AwardBody(hero, BodyAttributeType.Agility, "lightCombat", 4f);
    }

    static void AwardWeapon(HeroCombatProficiency hero, WeaponProficiencyType type, string key, float fallback)
    {
        var profile = hero ? hero.EnsureProfile() : null;
        if (profile == null)
            return;

        var xp = fallback;
        if (WeaponProficiencyGainConfigData.Instance.EnsureLoaded())
        {
            foreach (var row in WeaponProficiencyGainConfigData.Instance.GetAll())
            {
                if (row.GetProficiencyType() != type)
                    continue;
                // CSV may use trigger or actionKey depending on loader; accept both via actionKey/name.
                if (!KeyMatch(row.actionKey, key) && !KeyMatch(row.name, key))
                    continue;
                xp = row.baseXp > 0f ? row.baseXp : fallback;
                break;
            }
        }

        CombatProficiencyRuntime.AddWeaponXp(profile, type, xp);
        if (hero.debugXp)
            Debug.Log($"[CombatSimXp] {hero.name} weapon {type} +{xp:0.#} ({key})");
    }

    static void AwardBody(HeroCombatProficiency hero, BodyAttributeType type, string key, float fallback)
    {
        var profile = hero ? hero.EnsureProfile() : null;
        if (profile == null)
            return;

        var xp = fallback;
        if (BodyAttributeGainConfigData.Instance.EnsureLoaded())
        {
            foreach (var row in BodyAttributeGainConfigData.Instance.GetAll())
            {
                if (row.GetAttributeType() != type)
                    continue;
                if (!KeyMatch(row.actionKey, key))
                    continue;
                xp = row.baseXp > 0f ? row.baseXp : fallback;
                break;
            }
        }

        CombatProficiencyRuntime.AddAttributeXp(profile, type, xp);
        if (hero.debugXp)
            Debug.Log($"[CombatSimXp] {hero.name} body {type} +{xp:0.#} ({key})");
    }

    static void AwardFight(HeroCombatProficiency hero, FightAttributeType type, string key, float fallback)
    {
        var profile = hero ? hero.EnsureProfile() : null;
        if (profile == null)
            return;

        var xp = fallback;
        if (FightAttributeGainConfigData.Instance.EnsureLoaded())
        {
            foreach (var row in FightAttributeGainConfigData.Instance.GetAll())
            {
                if (row.GetAttributeType() != type)
                    continue;
                if (!KeyMatch(row.actionKey, key))
                    continue;
                xp = row.baseXp > 0f ? row.baseXp : fallback;
                break;
            }
        }

        CombatProficiencyRuntime.AddFightAttributeXp(profile, type, xp);
        if (hero.debugXp)
            Debug.Log($"[CombatSimXp] {hero.name} fight {type} +{xp:0.#} ({key})");
    }

    static bool KeyMatch(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return false;
        return string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase)
               || a.IndexOf(b, System.StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
