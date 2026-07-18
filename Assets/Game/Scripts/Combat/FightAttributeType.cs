/// <summary>
/// Fight attributes (战斗属性): decision / reaction layer for auto combat.
/// Offense = initiating attacks / trading;
/// Defense = block / parry / endure;
/// Awareness = reading foes, evade/kite intent, multi-target decisions.
/// (Dodge is NOT a fight attribute — evade timing uses Awareness + Agility.)
/// </summary>
public enum FightAttributeType
{
    Offense,
    Defense,
    Awareness,
}
