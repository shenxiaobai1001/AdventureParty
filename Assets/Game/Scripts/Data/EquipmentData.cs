using UnityEngine;

public class EquipmentData : Singleton<EquipmentData>
{
    public ConfigTable<EquipmentSetRow> sets = new ConfigTable<EquipmentSetRow>();

    bool _loaded;

    public bool Init()
    {
        _loaded = sets.Load("EquipmentSets.csv");
        return _loaded;
    }

    public bool EnsureLoaded()
    {
        if (_loaded && sets.GetListInfo().Count > 0)
            return true;

        return Init() && sets.GetListInfo().Count > 0;
    }

    public int SetCount => sets.GetListInfo().Count;

    public EquipmentSetRow GetSetByListOrder(int listIndex)
    {
        var list = sets.GetListInfo();
        if (listIndex < 0 || listIndex >= list.Count)
            return null;

        return list[listIndex];
    }

    public bool TryGetNextSetIndex(int currentSetIndex, out int nextSetIndex)
    {
        nextSetIndex = 0;
        if (!EnsureLoaded())
            return false;

        var list = sets.GetListInfo();
        if (list.Count == 0)
            return false;

        var currentListIndex = -1;
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].id != currentSetIndex)
                continue;

            currentListIndex = i;
            break;
        }

        var nextListIndex = currentListIndex < 0 ? 0 : (currentListIndex + 1) % list.Count;
        nextSetIndex = list[nextListIndex].id;
        return true;
    }

    public EquipmentSetRow GetSet(int setIndex)
    {
        return sets.GetInfo(setIndex);
    }

    public bool TryGetSet(int setIndex, out EquipmentSetRow row)
    {
        return sets.TryGetInfo(setIndex, out row);
    }

    public string GetSlotCombined(int setIndex, SyntyEquipmentSlot slot)
    {
        var row = GetSet(setIndex);
        return row != null ? row.GetSlotCombined(slot) : string.Empty;
    }

    public string[] GetSlotParts(int setIndex, SyntyEquipmentSlot slot)
    {
        return EquipmentPartParser.Split(GetSlotCombined(setIndex, slot));
    }

    public HeroEquipmentProfile CreateProfile(int setIndex)
    {
        var row = GetSet(setIndex);
        if (row == null)
            return null;

        var profile = ScriptableObject.CreateInstance<HeroEquipmentProfile>();
        row.CopyTo(profile);
        return profile;
    }

    public void ApplySetToHero(PlayerHeroEntity hero, int setIndex)
    {
        if (!hero)
            return;

        var row = GetSet(setIndex);
        if (row == null)
        {
            Debug.LogWarning($"[EquipmentData] Set not found: {setIndex}");
            return;
        }

        var profile = ScriptableObject.CreateInstance<HeroEquipmentProfile>();
        row.CopyTo(profile);
        hero.RefreshFull(profile);
    }
}

public class EquipmentSetRow : NamedData
{
    public string head;
    public string body;
    public string shoulder;
    public string forearm;
    public string hips;
    public string leg;
    public string back;

    public override string GetName()
    {
        return name;
    }

    public string GetSlotCombined(SyntyEquipmentSlot slot)
    {
        switch (EquipmentSlotUtility.NormalizeSlot(slot))
        {
            case SyntyEquipmentSlot.Head: return head ?? string.Empty;
            case SyntyEquipmentSlot.Body: return EquipmentPartParser.MergeCombined(body, shoulder);
            case SyntyEquipmentSlot.Forearm: return forearm ?? string.Empty;
            case SyntyEquipmentSlot.Hips: return hips ?? string.Empty;
            case SyntyEquipmentSlot.Leg: return leg ?? string.Empty;
            case SyntyEquipmentSlot.Back: return back ?? string.Empty;
            default: return string.Empty;
        }
    }

    public string[] GetSlotParts(SyntyEquipmentSlot slot)
    {
        return EquipmentPartParser.Split(GetSlotCombined(slot));
    }

    public string GetBodyTorsoName()
    {
        foreach (var part in EquipmentPartParser.Split(body))
        {
            if (part.StartsWith("Chr_Torso_"))
                return part;
        }

        return string.Empty;
    }

    public void CopyTo(HeroEquipmentProfile profile)
    {
        if (!profile)
            return;

        profile.setIndex = id;
        profile.setName = name;
        profile.head = head;
        profile.body = EquipmentPartParser.MergeCombined(body, shoulder);
        profile.shoulder = string.Empty;
        profile.forearm = forearm;
        profile.hips = hips;
        profile.leg = leg;
        profile.back = back;
    }

    public static EquipmentSetRow FromEntry(EquipmentSetEntry entry)
    {
        if (entry == null)
            return null;

        return new EquipmentSetRow
        {
            id = entry.setIndex,
            name = entry.setName,
            head = entry.head,
            body = entry.body,
            shoulder = entry.shoulder,
            forearm = entry.forearm,
            hips = entry.hips,
            leg = entry.leg,
            back = entry.back,
        };
    }
}
