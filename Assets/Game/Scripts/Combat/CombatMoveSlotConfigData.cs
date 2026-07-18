using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads <c>CombatMoveSlots.csv</c> — stance × slot → animation clip / combo.
/// Art XP stays on proficiency; this table is keyed by <see cref="CombatMoveStance"/>.
/// </summary>
public class CombatMoveSlotConfigData : Singleton<CombatMoveSlotConfigData>
{
    public const string SharedArmedRef = "SHARED_ARMED";
    public const string SharedArmedKickRef = "SHARED_ARMED_KICK";
    public const string SharedArmedBlockDualRef = "SHARED_ARMED_BLOCK_DUAL";
    public const string SharedUnarmedRef = "SHARED_UNARMED";

    public ConfigTable<CombatMoveSlotRow> rows = new ConfigTable<CombatMoveSlotRow>();

    bool _loaded;
    Dictionary<string, CombatMoveSlotRow> _byStanceSlot;

    public bool EnsureLoaded()
    {
        if (_loaded && rows.GetListInfo().Count > 0)
            return true;

        _loaded = rows.Load("CombatMoveSlots.csv");
        _byStanceSlot = null;
        return _loaded;
    }

    public IReadOnlyList<CombatMoveSlotRow> GetAll()
    {
        EnsureLoaded();
        return rows.GetListInfo();
    }

    public IReadOnlyList<CombatMoveSlotRow> GetForStance(CombatMoveStance stance)
    {
        EnsureLoaded();
        var result = new List<CombatMoveSlotRow>();
        foreach (var row in rows.GetListInfo())
        {
            if (row.GetStance() == stance)
                result.Add(row);
        }

        return result;
    }

    public bool TryGetRaw(CombatMoveStance stance, string slotId, out CombatMoveSlotRow row)
    {
        EnsureLoaded();
        BuildIndex();
        return _byStanceSlot.TryGetValue(IndexKey(stance, slotId), out row);
    }

    /// <summary>
    /// Resolves shared refs and shield overrides into a playable row snapshot.
    /// </summary>
    public bool TryResolve(
        ResolvedCombatLoadout loadout,
        string slotId,
        out CombatMoveSlotRow resolved,
        bool preferShieldBash = false)
    {
        resolved = null;
        if (string.IsNullOrEmpty(slotId))
            return false;

        EnsureLoaded();

        var hasShield = HasShield(loadout);
        if (hasShield && IsRollSlot(slotId))
        {
            if (TryGetRaw(CombatMoveStance.Shield, slotId, out var disabledRoll))
            {
                resolved = disabledRoll;
                return true;
            }

            return false;
        }

        if (hasShield && slotId == "melee.block")
        {
            if (TryGetRaw(CombatMoveStance.Shield, slotId, out var shieldBlock))
            {
                resolved = ExpandShared(shieldBlock);
                return resolved != null;
            }
        }

        if (preferShieldBash
            && hasShield
            && (slotId == "melee.attack_a" || slotId == "melee.attack_b"))
        {
            if (TryGetRaw(CombatMoveStance.Shield, slotId, out var shieldBash))
            {
                resolved = ExpandShared(shieldBash);
                return resolved != null;
            }
        }

        var stance = CombatMoveStanceResolver.ResolvePrimaryStance(loadout);
        if (!TryGetRaw(stance, slotId, out var raw))
        {
            // Fallback: ranged.dodge.* → SharedArmed melee.dodge.*
            if (slotId.StartsWith("ranged.dodge.", StringComparison.Ordinal))
            {
                var armedDodge = "melee.dodge." + slotId.Substring("ranged.dodge.".Length);
                if (TryGetRaw(CombatMoveStance.SharedArmed, armedDodge, out var sharedDodge))
                {
                    resolved = sharedDodge;
                    return true;
                }
            }

            return false;
        }

        resolved = ExpandShared(raw);
        return resolved != null;
    }

    public CombatMoveSlotRow ExpandShared(CombatMoveSlotRow row)
    {
        if (row == null)
            return null;

        var asset = row.animAsset;
        if (string.IsNullOrEmpty(asset) || !IsSharedRef(asset))
            return row;

        if (string.Equals(asset, SharedArmedKickRef, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetRaw(CombatMoveStance.SharedArmed, "melee.guard_break", out var kick)
                ? kick
                : row;
        }

        if (string.Equals(asset, SharedArmedBlockDualRef, StringComparison.OrdinalIgnoreCase))
        {
            return TryGetRaw(CombatMoveStance.SharedArmed, "melee.block_dual", out var dualBlock)
                ? dualBlock
                : row;
        }

        if (string.Equals(asset, SharedArmedRef, StringComparison.OrdinalIgnoreCase))
        {
            var sharedSlot = MapToSharedArmedSlot(row.slotId);
            return TryGetRaw(CombatMoveStance.SharedArmed, sharedSlot, out var shared)
                ? shared
                : row;
        }

        if (string.Equals(asset, SharedUnarmedRef, StringComparison.OrdinalIgnoreCase))
        {
            var sharedSlot = MapToSharedUnarmedSlot(row.slotId);
            return TryGetRaw(CombatMoveStance.SharedUnarmed, sharedSlot, out var shared)
                ? shared
                : row;
        }

        return row;
    }

    static string MapToSharedArmedSlot(string slotId)
    {
        if (string.IsNullOrEmpty(slotId))
            return slotId;

        if (slotId.StartsWith("ranged.dodge.", StringComparison.Ordinal))
            return "melee.dodge." + slotId.Substring("ranged.dodge.".Length);

        if (slotId == "melee.block")
            return "melee.block";

        return slotId;
    }

    static string MapToSharedUnarmedSlot(string slotId) => slotId;

    static bool IsSharedRef(string asset)
    {
        return string.Equals(asset, SharedArmedRef, StringComparison.OrdinalIgnoreCase)
            || string.Equals(asset, SharedArmedKickRef, StringComparison.OrdinalIgnoreCase)
            || string.Equals(asset, SharedArmedBlockDualRef, StringComparison.OrdinalIgnoreCase)
            || string.Equals(asset, SharedUnarmedRef, StringComparison.OrdinalIgnoreCase);
    }

    static bool IsRollSlot(string slotId)
    {
        return !string.IsNullOrEmpty(slotId)
            && slotId.StartsWith("melee.roll.", StringComparison.Ordinal);
    }

    static bool HasShield(ResolvedCombatLoadout loadout)
    {
        return loadout != null
            && loadout.offHand != null
            && loadout.offHand.category == WeaponCategory.Shield;
    }

    void BuildIndex()
    {
        if (_byStanceSlot != null)
            return;

        _byStanceSlot = new Dictionary<string, CombatMoveSlotRow>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows.GetListInfo())
        {
            if (string.IsNullOrEmpty(row.slotId))
                continue;

            _byStanceSlot[IndexKey(row.GetStance(), row.slotId)] = row;
        }
    }

    static string IndexKey(CombatMoveStance stance, string slotId)
        => stance + "|" + slotId;
}

public class CombatMoveSlotRow : NamedData
{
    public string stanceKey;
    public string slotId;
    public string animAsset;
    public string comboSequence;
    public string unlock;
    public string flags;
    public string aiWeightHint;
    public string notes;

    public CombatMoveStance GetStance()
    {
        return Enum.TryParse(stanceKey, true, out CombatMoveStance parsed)
            ? parsed
            : CombatMoveStance.OneHandSingle;
    }

    public bool IsDefaultUnlock()
        => string.IsNullOrEmpty(unlock)
           || string.Equals(unlock, "default", StringComparison.OrdinalIgnoreCase);

    public bool RequiresArt66()
        => string.Equals(unlock, "art>=66", StringComparison.OrdinalIgnoreCase);

    public bool IsDisabled()
        => ContainsFlag("disabled")
           || string.Equals(animAsset, "—", StringComparison.Ordinal)
           || string.Equals(unlock, "none", StringComparison.OrdinalIgnoreCase);

    public bool IsUiOnly()
        => ContainsFlag("ui_only")
           || string.Equals(animAsset, "UI_ONLY", StringComparison.OrdinalIgnoreCase);

    public bool IsCombo()
        => ContainsFlag("combo") || !string.IsNullOrEmpty(comboSequence);

    public bool IsShieldOverride()
        => ContainsFlag("shield_override");

    public bool HasPlayableClip()
        => !IsDisabled()
           && !IsUiOnly()
           && !string.IsNullOrEmpty(animAsset)
           && !animAsset.StartsWith("SHARED_", StringComparison.OrdinalIgnoreCase)
           && !animAsset.StartsWith("slot:", StringComparison.OrdinalIgnoreCase);

    public string[] GetComboSteps()
    {
        if (string.IsNullOrEmpty(comboSequence))
            return Array.Empty<string>();

        var parts = comboSequence.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
            parts[i] = parts[i].Trim();
        return parts;
    }

    bool ContainsFlag(string flag)
    {
        if (string.IsNullOrEmpty(flags) || string.IsNullOrEmpty(flag))
            return false;

        var parts = flags.Split(new[] { '|', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (string.Equals(part.Trim(), flag, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}

/// <summary>
/// Maps a resolved loadout to the primary combat-move stance template.
/// </summary>
public static class CombatMoveStanceResolver
{
    public static CombatMoveStance ResolvePrimaryStance(ResolvedCombatLoadout loadout)
    {
        if (loadout == null || loadout.gripMode == CombatGripMode.Unarmed || loadout.primaryHand == null)
            return CombatMoveStance.MartialArts;

        switch (loadout.gripMode)
        {
            case CombatGripMode.DualWield:
                return CombatMoveStance.OneHandDual;

            case CombatGripMode.OneHanded:
            case CombatGripMode.OneHandPlusOffHand:
                return ResolveOneHandOrRangedSidearm(loadout.primaryHand);

            case CombatGripMode.TwoHanded:
                return ResolveTwoHanded(loadout.primaryHand);

            default:
                return CombatMoveStance.MartialArts;
        }
    }

    static CombatMoveStance ResolveOneHandOrRangedSidearm(SyntyWeaponItemData weapon)
    {
        if (!weapon)
            return CombatMoveStance.OneHandSingle;

        if (WeaponProficiencyMapper.GetProficiencyType(weapon) == WeaponProficiencyType.Throwing)
            return CombatMoveStance.RangedThrowing;

        switch (weapon.category)
        {
            case WeaponCategory.FirearmPistol:
                return CombatMoveStance.RangedPistol;
            default:
                return CombatMoveStance.OneHandSingle;
        }
    }

    static CombatMoveStance ResolveTwoHanded(SyntyWeaponItemData weapon)
    {
        if (!weapon)
            return CombatMoveStance.GreatSword2H;

        if (WeaponProficiencyMapper.GetProficiencyType(weapon) == WeaponProficiencyType.Throwing)
            return CombatMoveStance.RangedThrowing;

        switch (weapon.category)
        {
            case WeaponCategory.GreatSword2H:
                return CombatMoveStance.GreatSword2H;
            case WeaponCategory.HeavyWeapon2H:
                return CombatMoveStance.HeavyWeapon2H;
            case WeaponCategory.Polearm2H:
                return CombatMoveStance.Polearm2H;
            case WeaponCategory.Bow:
                return ResolveBowOrCrossbow(weapon);
            case WeaponCategory.FirearmRifle:
                return CombatMoveStance.RangedRifle;
            default:
                return CombatMoveStance.GreatSword2H;
        }
    }

    static CombatMoveStance ResolveBowOrCrossbow(SyntyWeaponItemData weapon)
    {
        if (!weapon)
            return CombatMoveStance.RangedBow;

        var label = (weapon.itemName ?? string.Empty) + " " + (weapon.syntyPrefabPath ?? string.Empty);
        if (label.IndexOf("crossbow", StringComparison.OrdinalIgnoreCase) >= 0
            || label.IndexOf("弩", StringComparison.OrdinalIgnoreCase) >= 0)
            return CombatMoveStance.RangedCrossbow;

        return CombatMoveStance.RangedBow;
    }
}
