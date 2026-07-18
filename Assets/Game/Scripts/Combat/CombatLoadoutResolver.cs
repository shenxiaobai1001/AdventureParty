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
/// Resolved hand assignment from the weapon grid for visuals and animation.
/// </summary>
public sealed class ResolvedCombatLoadout
{
    public static readonly ResolvedCombatLoadout Empty = new ResolvedCombatLoadout();

    public CombatGripMode gripMode = CombatGripMode.Unarmed;
    public SyntyWeaponItemData primaryHand;
    public SyntyWeaponItemData offHand;
    public List<SyntyWeaponItemData> backWeapons = new List<SyntyWeaponItemData>();

    public bool HasDrawableWeapon =>
        gripMode != CombatGripMode.Unarmed && primaryHand != null;

    public bool UsesTwoHands => gripMode == CombatGripMode.TwoHanded;

    public bool HasOffHandWeapon => offHand != null;
}

/// <summary>
/// Derives main/off-hand weapon pairing from grid contents.
/// </summary>
public static class CombatLoadoutResolver
{
    public static ResolvedCombatLoadout Resolve(IReadOnlyList<WeaponGridEntry> entries)
    {
        var loadout = new ResolvedCombatLoadout();
        if (entries == null || entries.Count == 0)
            return loadout;

        var weapons = new List<SyntyWeaponItemData>();
        foreach (var entry in entries)
        {
            if (entry.WeaponData)
                weapons.Add(entry.WeaponData);
        }

        if (weapons.Count == 0)
            return loadout;

        SyntyWeaponItemData primary = null;
        SyntyWeaponItemData off = null;
        SyntyWeaponItemData shield = null;
        SyntyWeaponItemData pistol = null;
        var oneHandCandidates = new List<SyntyWeaponItemData>();
        var twoHandCandidates = new List<SyntyWeaponItemData>();

        foreach (var weapon in weapons)
        {
            switch (weapon.category)
            {
                case WeaponCategory.Shield:
                    shield ??= weapon;
                    break;
                case WeaponCategory.FirearmPistol:
                    pistol ??= weapon;
                    break;
                default:
                    if (WeaponProficiencyMapper.OccupiesBothHands(weapon.category))
                        twoHandCandidates.Add(weapon);
                    else
                        oneHandCandidates.Add(weapon);
                    break;
            }
        }

        if (twoHandCandidates.Count > 0)
        {
            primary = twoHandCandidates[0];
            loadout.gripMode = CombatGripMode.TwoHanded;
        }
        else if (oneHandCandidates.Count >= 2
            && WeaponProficiencyMapper.CanDualWield(oneHandCandidates[0], oneHandCandidates[1]))
        {
            primary = oneHandCandidates[0];
            off = oneHandCandidates[1];
            loadout.gripMode = CombatGripMode.DualWield;
        }
        else if (oneHandCandidates.Count > 0)
        {
            primary = oneHandCandidates[0];
            if (pistol != null && WeaponProficiencyMapper.CanPairOffHand(primary, pistol))
            {
                off = pistol;
                loadout.gripMode = CombatGripMode.OneHandPlusOffHand;
            }
            else if (shield != null && WeaponProficiencyMapper.CanPairOffHand(primary, shield))
            {
                off = shield;
                loadout.gripMode = CombatGripMode.OneHandPlusOffHand;
            }
            else
            {
                loadout.gripMode = CombatGripMode.OneHanded;
            }
        }
        else if (pistol != null)
        {
            primary = pistol;
            loadout.gripMode = CombatGripMode.OneHanded;
        }
        else if (shield != null)
        {
            primary = shield;
            loadout.gripMode = CombatGripMode.OneHanded;
        }

        foreach (var weapon in weapons)
        {
            if (weapon == primary || weapon == off)
                continue;

            loadout.backWeapons.Add(weapon);
        }

        loadout.primaryHand = primary;
        loadout.offHand = off;
        return loadout;
    }
}
