using System;
using UnityEngine;

[Serializable]
public class CharacterEntry
{
    public string displayName;
    public float hp;
    public float armorDurability;
    public float energy;
    public float hunger;
    public GameObject heroObject;
    public CharacterInventoryData inventory = new CharacterInventoryData();

    public CharacterEntry Clone()
    {
        return new CharacterEntry
        {
            displayName = displayName,
            hp = hp,
            armorDurability = armorDurability,
            energy = energy,
            hunger = hunger,
            heroObject = heroObject,
            inventory = inventory
        };
    }
}
