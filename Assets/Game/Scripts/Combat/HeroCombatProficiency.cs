using UnityEngine;

/// <summary>
/// Runtime bridge between CharacterEntry combat proficiency data and the hero instance.
/// </summary>
public class HeroCombatProficiency : MonoBehaviour
{
    [SerializeField] CharacterEntry characterEntry;
    [Tooltip("Log XP awards from CombatSimXp.")]
    public bool debugXp = true;

    public CharacterEntry CharacterEntry
    {
        get => characterEntry;
        set
        {
            characterEntry = value;
            characterEntry?.EnsureCombatDefaults();
        }
    }

    public CombatProficiencyProfile Profile
    {
        get
        {
            EnsureProfile();
            return characterEntry != null ? characterEntry.combatProficiency : null;
        }
    }

    void Awake()
    {
        EnsureProfile();
    }

    public CombatProficiencyProfile EnsureProfile()
    {
        if (characterEntry == null)
            characterEntry = new CharacterEntry { displayName = gameObject.name };

        characterEntry.EnsureCombatDefaults();
        return characterEntry.combatProficiency;
    }

    public void BindCharacterEntry(CharacterEntry entry)
    {
        CharacterEntry = entry;
    }

    public float GetWeaponLevel(WeaponProficiencyType type)
    {
        return Profile != null ? Profile.GetWeaponLevel(type) : 1f;
    }

    public float GetAttributeLevel(BodyAttributeType type)
    {
        return Profile != null ? Profile.GetAttributeLevel(type) : 1f;
    }

    public float GetFightAttributeLevel(FightAttributeType type)
    {
        return Profile != null ? Profile.GetFightAttributeLevel(type) : 1f;
    }

    public WeaponProficiencyType GetEquippedWeaponType()
    {
        var visual = GetComponent<HeroWeaponVisual>();
        if (visual && visual.equippedRight)
            return visual.equippedRight.proficiencyType;
        if (visual && visual.equippedLeft
            && visual.equippedLeft.category != WeaponCategory.Shield)
            return visual.equippedLeft.proficiencyType;
        return WeaponProficiencyType.MartialArts;
    }
}
