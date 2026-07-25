using System;
using System.Text.RegularExpressions;

/// <summary>How a resolved move-slot clip should be fired on the RPG animator.</summary>
public enum CombatMovePlaybackKind
{
    None,
    Attack,
    AttackDual,
    AttackKick,
    AttackRanged,
    Special,
    BlockStart,
    BlockEnd,
    BlockHit,
    BlockBreak,
    Dodge,
    Roll,
    GetHit,
    Reload,
    UiOnly,
}

/// <summary>Parsed playback parameters for one clip or slot.</summary>
public struct CombatMovePlaybackCommand
{
    public CombatMovePlaybackKind kind;
    public int action;
    public int side;
    public string source;
    public string error;
    public bool ok => kind != CombatMovePlaybackKind.None && string.IsNullOrEmpty(error);
}

/// <summary>
/// Maps RPG pack clip names / slot ids to animator trigger numbers + Action/Side.
/// </summary>
public static class CombatMoveClipMapper
{
    static readonly Regex AttackSide = new Regex(
        @"Attack-(Dual|L|R)(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static readonly Regex AttackPlain = new Regex(
        @"Attack(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static readonly Regex KickSide = new Regex(
        @"Kick-(L|R)(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static readonly Regex GetHitDir = new Regex(
        @"GetHit-([FBLR])(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static readonly Regex BlockGetHit = new Regex(
        @"Block(?:-.*)?-GetHit(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    static readonly Regex BlockBreak = new Regex(
        @"Block(?:-.*)?-Break(\d+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static CombatMovePlaybackCommand FromSlotHint(string slotId, string animAsset)
    {
        if (string.Equals(animAsset, "UI_ONLY", StringComparison.OrdinalIgnoreCase)
            || string.Equals(animAsset, "—", StringComparison.Ordinal)
            || string.Equals(animAsset, "-", StringComparison.Ordinal))
        {
            return new CombatMovePlaybackCommand
            {
                kind = CombatMovePlaybackKind.UiOnly,
                source = animAsset,
            };
        }

        if (!string.IsNullOrEmpty(slotId))
        {
            if (slotId == "melee.block")
                return new CombatMovePlaybackCommand { kind = CombatMovePlaybackKind.BlockStart, source = slotId };

            if (slotId == "react.block_break" || slotId == "react.block_break_dual")
                return new CombatMovePlaybackCommand { kind = CombatMovePlaybackKind.BlockBreak, source = slotId };

            if (slotId == "react.block_hit" || slotId == "react.block_hit_dual")
            {
                return new CombatMovePlaybackCommand
                {
                    kind = CombatMovePlaybackKind.BlockHit,
                    action = RpgAnimParams.HitForward1,
                    source = slotId,
                };
            }

            if (slotId.StartsWith("melee.dodge.", StringComparison.Ordinal)
                || slotId.StartsWith("ranged.dodge.", StringComparison.Ordinal))
            {
                return FromDodgeSlot(slotId);
            }

            if (slotId.StartsWith("melee.roll.", StringComparison.Ordinal))
                return FromRollSlot(slotId);

            if (slotId.StartsWith("react.hit.", StringComparison.Ordinal))
                return FromReactHitSlot(slotId);

            if (slotId == "ranged.reload")
                return new CombatMovePlaybackCommand { kind = CombatMovePlaybackKind.Reload, action = 1, source = slotId };

            if (slotId == "ranged.fire"
                || slotId == "melee.dash_attack"
                || slotId == "melee.attack_1"
                || slotId == "melee.attack_2"
                || slotId == "melee.attack_3")
                return FromClipName(animAsset);
        }

        return FromClipName(animAsset);
    }

    public static CombatMovePlaybackCommand FromClipName(string animAsset)
    {
        var cmd = new CombatMovePlaybackCommand { source = animAsset };
        if (string.IsNullOrWhiteSpace(animAsset))
        {
            cmd.error = "empty animAsset";
            return cmd;
        }

        if (animAsset.StartsWith("SHARED_", StringComparison.OrdinalIgnoreCase)
            || animAsset.StartsWith("slot:", StringComparison.OrdinalIgnoreCase)
            || animAsset.StartsWith("COMBO", StringComparison.OrdinalIgnoreCase))
        {
            cmd.error = "unexpanded ref: " + animAsset;
            return cmd;
        }

        var name = animAsset;
        var at = name.LastIndexOf('@');
        if (at >= 0 && at + 1 < name.Length)
            name = name.Substring(at + 1);

        if (BlockBreak.IsMatch(name))
        {
            cmd.kind = CombatMovePlaybackKind.BlockBreak;
            return cmd;
        }

        var blockHit = BlockGetHit.Match(name);
        if (blockHit.Success)
        {
            cmd.kind = CombatMovePlaybackKind.BlockHit;
            cmd.action = ParseInt(blockHit.Groups[1].Value, RpgAnimParams.HitForward1);
            return cmd;
        }

        if (name.IndexOf("Block", StringComparison.OrdinalIgnoreCase) >= 0
            && name.IndexOf("GetHit", StringComparison.OrdinalIgnoreCase) < 0
            && name.IndexOf("Break", StringComparison.OrdinalIgnoreCase) < 0
            && name.IndexOf("Attack", StringComparison.OrdinalIgnoreCase) < 0)
        {
            cmd.kind = CombatMovePlaybackKind.BlockStart;
            return cmd;
        }

        var kick = KickSide.Match(name);
        if (kick.Success || name.IndexOf("Kick", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            cmd.kind = CombatMovePlaybackKind.AttackKick;
            // Pack AttackKick uses Action = side-ish: 1 left, 2 right.
            if (kick.Success)
                cmd.action = string.Equals(kick.Groups[1].Value, "L", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
            else
                cmd.action = 2;
            return cmd;
        }

        if (name.IndexOf("Reload", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            cmd.kind = CombatMovePlaybackKind.Reload;
            cmd.action = 1;
            return cmd;
        }

        if (name.IndexOf("Dodge-", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            cmd.kind = CombatMovePlaybackKind.Dodge;
            if (name.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0)
                cmd.action = RpgAnimParams.DodgeLeft;
            else if (name.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0)
                cmd.action = RpgAnimParams.DodgeRight;
            else
                cmd.action = RpgAnimParams.DodgeBack;
            return cmd;
        }

        if (name.IndexOf("Roll-", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            cmd.kind = CombatMovePlaybackKind.Roll;
            if (name.IndexOf("Forward", StringComparison.OrdinalIgnoreCase) >= 0)
                cmd.action = RpgAnimParams.RollForward;
            else if (name.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0)
                cmd.action = RpgAnimParams.RollRight;
            else if (name.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0)
                cmd.action = RpgAnimParams.RollLeft;
            else
                cmd.action = RpgAnimParams.RollBack;
            return cmd;
        }

        var hit = GetHitDir.Match(name);
        if (hit.Success)
        {
            cmd.kind = CombatMovePlaybackKind.GetHit;
            cmd.action = HitActionFromDir(hit.Groups[1].Value);
            return cmd;
        }

        // Warrior ranged (must be before plain Attack\d+ — "RangeAttack1" contains "Attack1").
        var rangeAtk = Regex.Match(name, @"RangeAttack(\d+)", RegexOptions.IgnoreCase);
        if (rangeAtk.Success)
        {
            cmd.kind = CombatMovePlaybackKind.AttackRanged;
            cmd.action = ParseInt(rangeAtk.Groups[1].Value, 1);
            return cmd;
        }

        // Warrior MoveAttack* → treat as Attack for RPG bridge; Warrior controller path TBD.
        var moveAtk = Regex.Match(name, @"MoveAttack(\d+)", RegexOptions.IgnoreCase);
        if (moveAtk.Success)
        {
            cmd.kind = CombatMovePlaybackKind.Attack;
            cmd.action = ParseInt(moveAtk.Groups[1].Value, 1);
            return cmd;
        }

        // 2Hand-Shooting-Attack* / Fire → RPG Attack while Weapon=Rifle.
        if (name.IndexOf("Shooting-Fire", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("2Hand-Shooting-Attack", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            var shootAtk = Regex.Match(name, @"Attack(\d+)", RegexOptions.IgnoreCase);
            cmd.kind = CombatMovePlaybackKind.Attack;
            cmd.action = shootAtk.Success ? ParseInt(shootAtk.Groups[1].Value, 1) : 1;
            cmd.side = RpgAnimParams.SideNone;
            return cmd;
        }

        var sideAttack = AttackSide.Match(name);
        if (sideAttack.Success)
        {
            var sideToken = sideAttack.Groups[1].Value;
            var index = ParseInt(sideAttack.Groups[2].Value, 1);
            if (string.Equals(sideToken, "Dual", StringComparison.OrdinalIgnoreCase))
            {
                cmd.kind = CombatMovePlaybackKind.AttackDual;
                cmd.side = RpgAnimParams.SideDual;
                cmd.action = index;
            }
            else if (string.Equals(sideToken, "L", StringComparison.OrdinalIgnoreCase))
            {
                cmd.kind = CombatMovePlaybackKind.Attack;
                cmd.side = RpgAnimParams.SideLeft;
                cmd.action = index;
            }
            else
            {
                // RPG pack: Sword RightAttack1=8 (+7), Mace RightAttack1=4 (+3).
                cmd.kind = CombatMovePlaybackKind.Attack;
                cmd.side = RpgAnimParams.SideRight;
                cmd.action = MapRpgRightAttackAction(name, index);
            }

            return cmd;
        }

        var plain = AttackPlain.Match(name);
        if (plain.Success)
        {
            cmd.kind = CombatMovePlaybackKind.Attack;
            cmd.action = ParseInt(plain.Groups[1].Value, 1);
            cmd.side = RpgAnimParams.SideNone;
            return cmd;
        }

        if (name.IndexOf("Air-Attack", StringComparison.OrdinalIgnoreCase) >= 0
            || name.IndexOf("Special", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            cmd.kind = CombatMovePlaybackKind.Special;
            cmd.action = 1;
            return cmd;
        }

        cmd.error = "unparsed clip: " + animAsset;
        return cmd;
    }

    static CombatMovePlaybackCommand FromDodgeSlot(string slotId)
    {
        var cmd = new CombatMovePlaybackCommand { kind = CombatMovePlaybackKind.Dodge, source = slotId };
        if (slotId.EndsWith(".left", StringComparison.Ordinal))
            cmd.action = RpgAnimParams.DodgeLeft;
        else if (slotId.EndsWith(".right", StringComparison.Ordinal))
            cmd.action = RpgAnimParams.DodgeRight;
        else
            cmd.action = RpgAnimParams.DodgeBack;
        return cmd;
    }

    static CombatMovePlaybackCommand FromRollSlot(string slotId)
    {
        var cmd = new CombatMovePlaybackCommand { kind = CombatMovePlaybackKind.Roll, source = slotId };
        if (slotId.EndsWith(".forward", StringComparison.Ordinal))
            cmd.action = RpgAnimParams.RollForward;
        else if (slotId.EndsWith(".right", StringComparison.Ordinal))
            cmd.action = RpgAnimParams.RollRight;
        else if (slotId.EndsWith(".left", StringComparison.Ordinal))
            cmd.action = RpgAnimParams.RollLeft;
        else
            cmd.action = RpgAnimParams.RollBack;
        return cmd;
    }

    static CombatMovePlaybackCommand FromReactHitSlot(string slotId)
    {
        var cmd = new CombatMovePlaybackCommand { kind = CombatMovePlaybackKind.GetHit, source = slotId };
        if (slotId.EndsWith(".front", StringComparison.Ordinal))
            cmd.action = 1;
        else if (slotId.EndsWith(".back", StringComparison.Ordinal))
            cmd.action = 3;
        else if (slotId.EndsWith(".left", StringComparison.Ordinal))
            cmd.action = 4;
        else if (slotId.EndsWith(".right", StringComparison.Ordinal))
            cmd.action = 5;
        else
            cmd.action = 1;
        return cmd;
    }

    static int HitActionFromDir(string dir)
    {
        switch (char.ToUpperInvariant(dir[0]))
        {
            case 'F': return 1;
            case 'B': return 3;
            case 'L': return 4;
            case 'R': return 5;
            default: return 1;
        }
    }

    /// <summary>
    /// RPG Character pack Action indices for right-hand attacks differ by weapon family.
    /// Sword: RightAttack1=8; Mace: RightAttack1=4; left-hand uses 1..N as-is.
    /// </summary>
    static int MapRpgRightAttackAction(string clipName, int index)
    {
        if (index <= 0)
            index = 1;

        if (clipName.IndexOf("Mace", StringComparison.OrdinalIgnoreCase) >= 0
            || clipName.IndexOf("Pistol", StringComparison.OrdinalIgnoreCase) >= 0)
            return index + 3;

        // Sword / default Armed right attacks
        return index + 7;
    }

    static int ParseInt(string text, int fallback)
        => int.TryParse(text, out var value) ? value : fallback;
}
