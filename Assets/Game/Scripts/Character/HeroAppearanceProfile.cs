using PsychoticLab;
using UnityEngine;

[CreateAssetMenu(fileName = "HeroAppearance", menuName = "AdventureParty/Hero Appearance Profile")]
public class HeroAppearanceProfile : ScriptableObject
{
    [Header("Identity")]
    public Gender gender = Gender.Male;

    [Header("Face (locked at creation)")]
    public string head = "Chr_Head_Male_01";
    public string eyebrow = "Chr_Eyebrow_Male_01";
    public string facialHair = "Chr_FacialHair_Male_02";
    public string hair = "Chr_Hair_02";
    public string headCovering = "Chr_HeadCoverings_Base_Hair_02";

    [Header("Base body (_00 cloth, restored when gear removed)")]
    public string torso = "Chr_Torso_Male_00";
    public string armUpperRight = "Chr_ArmUpperRight_Male_00";
    public string armUpperLeft = "Chr_ArmUpperLeft_Male_00";
    public string armLowerRight = "Chr_ArmLowerRight_Male_00";
    public string armLowerLeft = "Chr_ArmLowerLeft_Male_00";
    public string handRight = "Chr_HandRight_Male_00";
    public string handLeft = "Chr_HandLeft_Male_00";
    public string hips = "Chr_Hips_Male_00";
    public string legRight = "Chr_LegRight_Male_00";
    public string legLeft = "Chr_LegLeft_Male_00";

    [Header("Colors")]
    public Color skinColor = new Color(1f, 0.8f, 0.682f);
    public Color hairColor = new Color(0.31f, 0.25f, 0.18f);
    public Color stubbleColor = new Color(0.8f, 0.7f, 0.63f);
    public Color scarColor = new Color(0.93f, 0.69f, 0.59f);
    public Color primaryColor = new Color(0.29f, 0.4f, 0.49f);
    public Color secondaryColor = new Color(0.7f, 0.62f, 0.47f);
    public Color metalPrimaryColor = new Color(0.67f, 0.67f, 0.67f);
    public Color metalSecondaryColor = new Color(0.39f, 0.4f, 0.41f);
    public Color leatherPrimaryColor = new Color(0.48f, 0.35f, 0.24f);
    public Color leatherSecondaryColor = new Color(0.33f, 0.24f, 0.16f);

    [Header("Body type")]
    public Vector3 bodyScale = Vector3.one;
}
