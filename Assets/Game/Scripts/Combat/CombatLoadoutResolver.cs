using System;
using System.Collections.Generic;

public enum CombatGripMode
{
    Unarmed,
    TwoHanded,
    OneHanded,
    OneHandPlusOffHand,
    DualWield,
}

/// <summary>
/// Resolved left/right hand assignment for visuals and animation templates.
/// </summary>
public sealed class ResolvedCombatLoadout
{
    public static readonly ResolvedCombatLoadout Empty = new ResolvedCombatLoadout();

    public CombatGripMode gripMode = CombatGripMode.Unarmed;
    public SyntyWeaponItemData primaryHand;
    public SyntyWeaponItemData offHand;
    public SyntyWeaponItemData rightHand;
    public SyntyWeaponItemData leftHand;
    public List<SyntyWeaponItemData> backWeapons = new List<SyntyWeaponItemData>();

    public bool HasDrawableWeapon =>
        gripMode != CombatGripMode.Unarmed && primaryHand != null;

    public bool UsesTwoHands => gripMode == CombatGripMode.TwoHanded;

    public bool HasOffHandWeapon => offHand != null;

    public bool HasShield =>
        (leftHand && leftHand.category == WeaponCategory.Shield)
        || (offHand && offHand.category == WeaponCategory.Shield);
}

/// <summary>
/// Derives combat grip from explicit left/right equip slots (no preference system).
/// </summary>
public static class CombatLoadoutResolver
{
    public static ResolvedCombatLoadout Resolve(SyntyWeaponItemData right, SyntyWeaponItemData left)
    {
        var loadout = new ResolvedCombatLoadout
        {
            rightHand = right,
            leftHand = left,
        };

        if (!right && !left)
            return loadout;

        // Two-hand / staff on right occupies both.
        if (right && WeaponProficiencyMapper.OccupiesBothHands(right.category)
            && !WeaponProficiencyMapper.IsLeftHandPrimary(right.category))
        {
            loadout.primaryHand = right;
            loadout.offHand = null;
            loadout.gripMode = CombatGripMode.TwoHanded;
            return loadout;
        }

        // Bow on left (or misplaced on right) — left holds bow, right draws string.
        var bow = (left && left.category == WeaponCategory.Bow) ? left
            : (right && right.category == WeaponCategory.Bow) ? right
            : null;
        if (bow)
        {
            loadout.primaryHand = bow;
            loadout.offHand = null;
            loadout.rightHand = null;
            loadout.leftHand = bow;
            loadout.gripMode = CombatGripMode.TwoHanded;
            return loadout;
        }

        if (right && left)
        {
            if (left.category == WeaponCategory.Shield
                || left.category == WeaponCategory.ShortGun)
            {
                loadout.primaryHand = right;
                loadout.offHand = left;
                loadout.gripMode = CombatGripMode.OneHandPlusOffHand;
                return loadout;
            }

            // Dual wield two one-hand weapons.
            loadout.primaryHand = right;
            loadout.offHand = left;
            loadout.gripMode = CombatGripMode.DualWield;
            return loadout;
        }

        if (right)
        {
            loadout.primaryHand = right;
            loadout.gripMode = CombatGripMode.OneHanded;
            return loadout;
        }

        // Left only (shield or sidearm alone).
        loadout.primaryHand = left;
        loadout.gripMode = CombatGripMode.OneHanded;
        return loadout;
    }

    /// <summary>
    /// Fallback when no explicit hand slots: pick from weapon grid with default rules
    /// (shield→left, else fill right then left). Used only to seed empty equip state.
    /// </summary>
    public static void SuggestDefaultEquip(
        IReadOnlyList<WeaponGridEntry> entries,
        out SyntyWeaponItemData right,
        out SyntyWeaponItemData left)
    {
        right = null;
        left = null;
        if (entries == null || entries.Count == 0)
            return;

        var sorted = new List<WeaponGridEntry>(entries);
        sorted.Sort((a, b) => a.GridOrder.CompareTo(b.GridOrder));

        foreach (var entry in sorted)
        {
            var w = entry.WeaponData;
            if (!w)
                continue;

            if (w.category == WeaponCategory.Shield || w.category == WeaponCategory.Bow)
            {
                left ??= w;
                if (w.category == WeaponCategory.Bow)
                    return;
                continue;
            }

            if (WeaponProficiencyMapper.OccupiesBothHands(w.category))
            {
                if (right == null && left == null)
                {
                    right = w;
                    return;
                }

                continue;
            }

            if (right == null)
                right = w;
            else if (left == null && WeaponProficiencyMapper.CanEquipToHand(w.category, WeaponHand.Left))
                left = w;
        }
    }

    [Obsolete("Use Resolve(right, left) with explicit hand slots.")]
    public static ResolvedCombatLoadout Resolve(IReadOnlyList<WeaponGridEntry> entries)
    {
        SuggestDefaultEquip(entries, out var right, out var left);
        var loadout = Resolve(right, left);
        if (entries == null)
            return loadout;

        foreach (var entry in entries)
        {
            var w = entry.WeaponData;
            if (!w || w == right || w == left)
                continue;
            loadout.backWeapons.Add(w);
        }

        return loadout;
    }
}
