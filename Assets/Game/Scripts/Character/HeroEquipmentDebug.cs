using UnityEngine;

/// <summary>
/// Press [1] to cycle equipment sets from EquipmentSets.csv (Synty Preset_1..120).
/// </summary>
public class HeroEquipmentDebug : MonoBehaviour
{
    public PlayerHeroEntity hero;

    int _currentSetIndex;

    void Awake()
    {
        if (!hero)
            hero = GetComponent<PlayerHeroEntity>();
    }

    void Start()
    {
        if (!EquipmentData.Instance.EnsureLoaded())
            Debug.LogWarning("[HeroEquipmentDebug] EquipmentSets.csv not loaded yet. Press [1] to retry.", this);

        if (hero && hero.activeEquipment)
            _currentSetIndex = hero.activeEquipment.setIndex;
    }

    void Update()
    {
        if (!hero || !Input.GetKeyDown(KeyCode.Alpha1))
            return;

        if (!EquipmentData.Instance.EnsureLoaded())
        {
            Debug.LogWarning("[HeroEquipmentDebug] EquipmentSets.csv not loaded.", this);
            return;
        }

        if (!EquipmentData.Instance.TryGetNextSetIndex(_currentSetIndex, out var nextSetIndex))
            return;

        _currentSetIndex = nextSetIndex;
        EquipmentData.Instance.ApplySetToHero(hero, _currentSetIndex);
    }

    void OnGUI()
    {
        if (!hero)
            return;

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            normal = { textColor = Color.white }
        };

        var row = EquipmentData.Instance.GetSet(_currentSetIndex);
        var setName = row != null ? row.name : "(none)";
        var torso = row != null ? row.GetBodyTorsoName() : string.Empty;
        var total = EquipmentData.Instance.SetCount;

        GUI.Label(new Rect(12, 12, 900, 24),
            $"Equipment #{_currentSetIndex} {setName}  |  {torso}  |  {total} sets  |  [1] Next",
            style);
    }
}
