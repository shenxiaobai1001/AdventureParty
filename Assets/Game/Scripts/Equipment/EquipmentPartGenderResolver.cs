using PsychoticLab;

/// <summary>
/// Maps Synty part names to the hero appearance gender at runtime.
/// </summary>
public static class EquipmentPartGenderResolver
{
    public static string Resolve(string partName, Gender appearanceGender)
    {
        if (string.IsNullOrEmpty(partName))
            return partName;

        if (appearanceGender == Gender.Male && partName.Contains("_Female_"))
            return partName.Replace("_Female_", "_Male_");

        if (appearanceGender == Gender.Female && partName.Contains("_Male_"))
            return partName.Replace("_Male_", "_Female_");

        return partName;
    }
}
