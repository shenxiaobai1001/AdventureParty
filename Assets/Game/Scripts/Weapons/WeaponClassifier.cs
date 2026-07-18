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



        if (name.Contains("Bow", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.Bow;



        if (name.Contains("Elephant_Gun_02", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.FirearmPistol;



        if (name.Contains("Elephant_Gun_01", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.FirearmRifle;



        if (name.Contains("Spear", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Joust", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Staff", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Sceptre", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.Polearm2H;



        if (name.Contains("Hammer", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.HeavyWeapon2H;



        if (name.Contains("Sword_Large", StringComparison.OrdinalIgnoreCase)

            || (name.Contains("Large", StringComparison.OrdinalIgnoreCase)

                && name.Contains("Sword", StringComparison.OrdinalIgnoreCase)))

            return WeaponCategory.GreatSword2H;



        if (name.Contains("Axe", StringComparison.OrdinalIgnoreCase))

            return pack == WeaponPack.Hero ? WeaponCategory.Hammer1H : WeaponCategory.HeavyWeapon2H;



        if (name.Contains("Mace", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.Hammer1H;



        if (name.Contains("Dagger", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Knife", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Thowing", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Knuckle", StringComparison.OrdinalIgnoreCase)

            || name.Contains("IcePick", StringComparison.OrdinalIgnoreCase)

            || name.Contains("CorkScrew", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.Dagger1H;



        if (name.Contains("Sword", StringComparison.OrdinalIgnoreCase)

            || name.Contains("Rapier", StringComparison.OrdinalIgnoreCase))

            return WeaponCategory.Sword1H;



        return WeaponCategory.Misc1H;

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

            case WeaponCategory.Polearm2H:

            case WeaponCategory.GreatSword2H:

            case WeaponCategory.HeavyWeapon2H:

            case WeaponCategory.FirearmRifle:

                return new Vector2Int(10, 2);

            default:

                return new Vector2Int(6, 2);

        }

    }



    public static bool UsesVerticalIconRender(WeaponCategory category)

    {

        return category == WeaponCategory.Shield || category == WeaponCategory.Bow;

    }



    public static float GetDefaultWeight(WeaponCategory category)

    {

        switch (category)

        {

            case WeaponCategory.Shield: return 5f;

            case WeaponCategory.Bow: return 3f;

            case WeaponCategory.Polearm2H: return 6f;

            case WeaponCategory.GreatSword2H: return 7f;

            case WeaponCategory.HeavyWeapon2H: return 7f;

            case WeaponCategory.FirearmRifle: return 6f;

            case WeaponCategory.FirearmPistol: return 3.5f;

            case WeaponCategory.Hammer1H: return 4f;

            case WeaponCategory.Dagger1H: return 1.5f;

            case WeaponCategory.Sword1H: return 3f;

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

}


