using UnityEngine;

/// <summary>
/// Runtime bridge between CharacterEntry combat proficiency data and the hero instance.
/// </summary>
public class HeroCombatProficiency : MonoBehaviour
{
    [SerializeField] CharacterEntry characterEntry;

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
            characterEntry?.EnsureCombatDefaults();
            return characterEntry != null ? characterEntry.combatProficiency : null;
        }
    }

    void Awake()
    {
        characterEntry?.EnsureCombatDefaults();
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
}
