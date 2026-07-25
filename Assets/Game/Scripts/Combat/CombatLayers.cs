using UnityEngine;

/// <summary>
/// Physics layers used by melee sweep / hurt detection.
/// </summary>
public static class CombatLayers
{
    public const string HurtName = "CombatHurt";
    public const string HitName = "CombatHit";

    public static int Hurt => LayerMask.NameToLayer(HurtName);
    public static int Hit => LayerMask.NameToLayer(HitName);

    public static LayerMask HurtMask
    {
        get
        {
            var layer = Hurt;
            return layer >= 0 ? 1 << layer : ~0;
        }
    }
}
