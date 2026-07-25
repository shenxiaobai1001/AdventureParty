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
            if (TryGetRaw(CombatMoveStance.SwordShield, slotId, out var disabledRoll)
                && disabledRoll.IsDisabled())
            {
                resolved = disabledRoll;
                return true;
            }

            return false;
        }

        var stance = CombatMoveStanceResolver.ResolvePrimaryStance(loadout);
        var dual = loadout != null && loadout.gripMode == CombatGripMode.DualWield;

        // Dual uses Dual block-hit / break variants when present.
        if (dual)
        {
            if (slotId == "react.block_hit"
                && TryGetRaw(CombatMoveStance.SharedArmed, "react.block_hit_dual", out var dualHit))
            {
                resolved = dualHit;
                return true;
            }

            if (slotId == "react.block_break"
                && TryGetRaw(CombatMoveStance.SharedArmed, "react.block_break_dual", out var dualBreak))
            {
                resolved = dualBreak;
                return true;
            }
        }

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

            // Reaction slots: 1H / martial fall back to shared tables.
            if (slotId.StartsWith("react.", StringComparison.Ordinal)
                && TryResolveReactionFallback(stance, slotId, out var reactFallback))
            {
                resolved = reactFallback;
                return true;
            }

            return false;
        }

        resolved = ExpandShared(raw);
        return resolved != null;
    }

    bool TryResolveReactionFallback(CombatMoveStance stance, string slotId, out CombatMoveSlotRow row)
    {
        row = null;
        switch (stance)
        {
            case CombatMoveStance.MartialArts:
                return TryGetRaw(CombatMoveStance.SharedUnarmed, slotId, out row);
            case CombatMoveStance.OneHandSingle:
            case CombatMoveStance.DualBlades:
            case CombatMoveStance.DualHeavy:
            case CombatMoveStance.RangedPistol:
            case CombatMoveStance.RangedThrowing:
                return TryGetRaw(CombatMoveStance.SharedArmed, slotId, out row);
            default:
                return false;
        }
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
            // Guard-break / kick slots removed from templates.
            return row;
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
        => loadout != null && loadout.HasShield;

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
           || string.Equals(animAsset, "-", StringComparison.Ordinal)
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
/// Maps left/right loadout to the primary combat-move animation template.
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
                return ResolveDual(loadout.rightHand, loadout.leftHand);

            case CombatGripMode.OneHandPlusOffHand:
                if (loadout.HasShield)
                    return CombatMoveStance.SwordShield;
                if (loadout.primaryHand.category == WeaponCategory.ShortGun
                    || (loadout.offHand && loadout.offHand.category == WeaponCategory.ShortGun))
                    return CombatMoveStance.RangedPistol;
                return CombatMoveStance.OneHandSingle;

            case CombatGripMode.OneHanded:
                return ResolveOneHanded(loadout);

            case CombatGripMode.TwoHanded:
                return ResolveTwoHanded(loadout.primaryHand);

            default:
                return CombatMoveStance.MartialArts;
        }
    }

    static CombatMoveStance ResolveDual(SyntyWeaponItemData right, SyntyWeaponItemData left)
    {
        if (right && left
            && WeaponProficiencyMapper.IsEdged(right.category)
            && WeaponProficiencyMapper.IsEdged(left.category))
            return CombatMoveStance.DualBlades;

        return CombatMoveStance.DualHeavy;
    }

    static CombatMoveStance ResolveOneHanded(ResolvedCombatLoadout loadout)
    {
        var weapon = loadout.primaryHand;
        if (!weapon)
            return CombatMoveStance.OneHandSingle;

        if (WeaponProficiencyMapper.GetProficiencyType(weapon) == WeaponProficiencyType.Throwing)
            return CombatMoveStance.RangedThrowing;

        if (weapon.category == WeaponCategory.ShortGun)
            return CombatMoveStance.RangedPistol;

        if (weapon.category == WeaponCategory.Shield)
            return CombatMoveStance.SwordShield;

        // Single 1H melee, left empty → dedicated OneHandSingle (RPG) template.
        return CombatMoveStance.OneHandSingle;
    }

    static CombatMoveStance ResolveTwoHanded(SyntyWeaponItemData weapon)
    {
        if (!weapon)
            return CombatMoveStance.GreatSword;

        switch (weapon.category)
        {
            case WeaponCategory.GreatSword:
                return CombatMoveStance.GreatSword;
            case WeaponCategory.GreatHammer:
            case WeaponCategory.GreatAxe:
                return CombatMoveStance.HeavyWeapon2H;
            case WeaponCategory.Spear:
                return CombatMoveStance.Spear;
            case WeaponCategory.Staff:
                return CombatMoveStance.Staff;
            case WeaponCategory.Bow:
                return CombatMoveStance.RangedBow;
            case WeaponCategory.Crossbow:
                return CombatMoveStance.RangedCrossbow;
            case WeaponCategory.LongGun:
                return CombatMoveStance.RangedRifle;
            default:
                return CombatMoveStance.GreatSword;
        }
    }
}
