using System;

[Serializable]
public struct ProficiencyValue
{
    public float level;
    public float xp;

    public ProficiencyValue(float level, float xp)
    {
        this.level = level;
        this.xp = xp;
    }

    public static ProficiencyValue Default => new ProficiencyValue(1f, 0f);
}
