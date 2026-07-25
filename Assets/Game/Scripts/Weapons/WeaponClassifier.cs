using System;
using UnityEngine;

public static class WeaponClassifier
{
    public static bool ShouldIncludePrefab(string assetPath, string prefabName)
    {
        if (string.IsNullOrEmpty(assetPath) || string.IsNullOrEmpty(prefabName))
            return false;

        var normalized = assetPath.Replace('\\', '/');
        if (normalized.Contains("/Modular/"))
            return false;

        if (prefabName.EndsWith("_Cover", StringComparison.OrdinalIgnoreCase))
            return false;

        if (prefabName.Contains("_Arrow_", StringComparison.OrdinalIgnoreCase)
            || prefabName.Contains("_Quiver_", StringComparison.OrdinalIgnoreCase))
            return false;

        return prefabName.StartsWith("SM_Wep_", StringComparison.OrdinalIgnoreCase)
            || prefabName.StartsWith("SM_Prop_Bow_", StringComparison.OrdinalIgnoreCase);
    }

    public static WeaponPack GetPack(string assetPath)
    {
        var normalized = assetPath.Replace('\\', '/');
        return normalized.Contains("PolygonFantasyKingdom/")
            ? WeaponPack.Kingdom
            : WeaponPack.Hero;
    }

    public static WeaponCategory Classify(string prefabName, WeaponPack pack)
    {
        var name = prefabName ?? string.Empty;

        if (name.Contains("Shield", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Shield;

        if (name.Contains("Crossbow", StringComparison.OrdinalIgnoreCase)
            || name.Contains("弩", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Crossbow;

        if (name.Contains("Bow", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Bow;

        if (name.Contains("Elephant_Gun_02", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.ShortGun;

        if (name.Contains("Elephant_Gun_01", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.LongGun;

        if (name.Contains("Staff", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Sceptre", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Staff;

        if (name.Contains("Spear", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Joust", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Spear;

        if (name.Contains("Sword_Large", StringComparison.OrdinalIgnoreCase)
            || (name.Contains("Large", StringComparison.OrdinalIgnoreCase)
                && name.Contains("Sword", StringComparison.OrdinalIgnoreCase)))
            return WeaponCategory.GreatSword;

        if (name.Contains("Hammer", StringComparison.OrdinalIgnoreCase))
            return pack == WeaponPack.Hero ? WeaponCategory.Hammer : WeaponCategory.GreatHammer;

        if (name.Contains("Axe", StringComparison.OrdinalIgnoreCase))
            return pack == WeaponPack.Hero ? WeaponCategory.Axe : WeaponCategory.GreatAxe;

        if (name.Contains("Mace", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Hammer;

        if (name.Contains("Dagger", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Knife", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Thowing", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Knuckle", StringComparison.OrdinalIgnoreCase)
            || name.Contains("IcePick", StringComparison.OrdinalIgnoreCase)
            || name.Contains("CorkScrew", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Sword;

        if (name.Contains("Sword", StringComparison.OrdinalIgnoreCase)
            || name.Contains("Rapier", StringComparison.OrdinalIgnoreCase))
            return WeaponCategory.Sword;

        return WeaponCategory.Sword;
    }

    public static WeaponProficiencyType GetProficiencyType(WeaponCategory category)
    {
        return WeaponProficiencyMapper.GetProficiencyType(category);
    }

    public static Vector2Int GetGridSize(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Shield:
                return new Vector2Int(4, 4);
            case WeaponCategory.Bow:
            case WeaponCategory.Crossbow:
            case WeaponCategory.Spear:
            case WeaponCategory.Staff:
            case WeaponCategory.GreatSword:
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe:
            case WeaponCategory.LongGun:
                return new Vector2Int(10, 2);
            default:
                return new Vector2Int(6, 2);
        }
    }

    public static bool UsesVerticalIconRender(WeaponCategory category)
    {
        return category == WeaponCategory.Shield
            || category == WeaponCategory.Bow
            || category == WeaponCategory.Crossbow;
    }

    public static float GetDefaultWeight(WeaponCategory category)
    {
        switch (category)
        {
            case WeaponCategory.Shield: return 5f;
            case WeaponCategory.Bow:
            case WeaponCategory.Crossbow: return 3f;
            case WeaponCategory.Spear:
            case WeaponCategory.Staff: return 6f;
            case WeaponCategory.GreatSword:
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe: return 7f;
            case WeaponCategory.LongGun: return 6f;
            case WeaponCategory.ShortGun: return 3.5f;
            case WeaponCategory.Hammer:
            case WeaponCategory.Axe: return 4f;
            case WeaponCategory.Sword: return 3f;
            default: return 2f;
        }
    }

    public static string GetDisplayName(string prefabName, WeaponPack pack)
    {
        var readable = prefabName
            .Replace("SM_Wep_", string.Empty)
            .Replace("SM_Prop_", string.Empty)
            .Replace('_', ' ');
        return $"{pack} {readable}";
    }

    public static string GetAssetStem(WeaponPack pack, string prefabName)
    {
        var prefix = pack == WeaponPack.Kingdom ? "kingdom" : "hero";
        return $"{prefix}_{prefabName.ToLowerInvariant()}";
    }

    /// <summary>Parse category with legacy CSV / folder names.</summary>
    public static bool TryParseCategory(string raw, out WeaponCategory category)
    {
        category = WeaponCategory.Sword;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (Enum.TryParse(raw, true, out category))
            return true;

        switch (raw.Trim())
        {
            case "Sword1H": category = WeaponCategory.Sword; return true;
            case "Hammer1H": category = WeaponCategory.Hammer; return true;
            case "Dagger1H":
            case "Dagger": category = WeaponCategory.Sword; return true;
            case "Polearm2H": category = WeaponCategory.Spear; return true;
            case "GreatSword2H": category = WeaponCategory.GreatSword; return true;
            case "HeavyWeapon2H": category = WeaponCategory.GreatHammer; return true;
            case "FirearmRifle": category = WeaponCategory.LongGun; return true;
            case "FirearmPistol": category = WeaponCategory.ShortGun; return true;
            case "Misc1H": category = WeaponCategory.Sword; return true;
            default: return false;
        }
    }

    public static bool TryParseProficiency(string raw, out WeaponProficiencyType type)
    {
        type = WeaponProficiencyType.MartialArts;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (Enum.TryParse(raw, true, out type))
            return true;

        switch (raw.Trim())
        {
            case "Longsword": type = WeaponProficiencyType.Sword; return true;
            case "HammerAxe": type = WeaponProficiencyType.Hammer; return true;
            case "HeavyWeapon": type = WeaponProficiencyType.GreatHammer; return true;
            case "Polearm": type = WeaponProficiencyType.Spear; return true;
            case "BowCrossbow": type = WeaponProficiencyType.Bow; return true;
            case "Firearm": type = WeaponProficiencyType.LongGun; return true;
            case "Dagger": type = WeaponProficiencyType.Sword; return true;
            default: return false;
        }
    }
}
