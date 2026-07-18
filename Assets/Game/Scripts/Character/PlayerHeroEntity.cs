using UnityEngine;

/// <summary>
/// Player hero root: appearance + equipment refresh. Weapons are configured on HeroWeaponVisual.
/// </summary>
public class PlayerHeroEntity : MonoBehaviour
{
    [Header("Profiles")]
    public HeroAppearanceProfile appearance;
    public HeroEquipmentProfile equipmentSetA;
    public HeroEquipmentProfile equipmentSetB;

    [Header("Components")]
    public ModularHeroVisual visual;
    public HeroWeaponVisual weaponVisual;

    [Header("Runtime")]
    public HeroEquipmentProfile activeEquipment;

    void Awake()
    {
        if (!visual)
            visual = GetComponent<ModularHeroVisual>();

        if (!weaponVisual)
            weaponVisual = GetComponent<HeroWeaponVisual>();

        if (Application.isPlaying
            && GetComponent<CharacterController>()
            && !GetComponent<EquipmentPickupInteractor>())
        {
            gameObject.AddComponent<EquipmentPickupInteractor>();
        }
    }

    void Start()
    {
        RefreshFull(activeEquipment ? activeEquipment : equipmentSetA);
        EnsureCombatProficiencyComponent();
    }

    void EnsureCombatProficiencyComponent()
    {
        if (!GetComponent<HeroCombatProficiency>())
            gameObject.AddComponent<HeroCombatProficiency>();
    }

    public void EquipSetA()
    {
        RefreshFull(equipmentSetA);
    }

    public void EquipSetB()
    {
        RefreshFull(equipmentSetB);
    }

    public void RefreshFull(HeroEquipmentProfile equipment)
    {
        if (!visual || !appearance)
        {
            Debug.LogWarning("[PlayerHeroEntity] Missing visual or appearance profile.", this);
            return;
        }

        activeEquipment = equipment;
        visual.ApplyAppearance(appearance);

        if (equipment)
            visual.ApplyEquipment(equipment, appearance);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!visual)
            visual = GetComponent<ModularHeroVisual>();

        if (!weaponVisual)
            weaponVisual = GetComponent<HeroWeaponVisual>();
    }
#endif
}
