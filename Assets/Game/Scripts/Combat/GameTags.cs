/// <summary>
/// Unity tag names used for faction / context-menu targeting.
/// </summary>
public static class GameTags
{
    public const string Teammate = "Teammate";
    public const string Npc = "NPC";
    public const string Enemy = "Enemy";
    /// <summary>Legacy; prefer <see cref="Teammate"/> for player-squad heroes.</summary>
    public const string Player = "Player";

    public static bool IsTeammate(UnityEngine.GameObject go)
    {
        if (!go)
            return false;
        return go.CompareTag(Teammate) || go.CompareTag(Player);
    }

    public static bool IsHostileOrNpc(UnityEngine.GameObject go)
    {
        if (!go)
            return false;
        return go.CompareTag(Npc) || go.CompareTag(Enemy);
    }

    public static bool IsContextTarget(UnityEngine.GameObject go)
        => IsTeammate(go) || IsHostileOrNpc(go);
}
