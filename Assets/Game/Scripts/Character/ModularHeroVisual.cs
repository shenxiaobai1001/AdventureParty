using System.Collections.Generic;
using PsychoticLab;
using UnityEngine;

/// <summary>
/// Runtime modular part switching for Synty Fantasy Hero characters.
/// Strips demo/random logic from CharacterRandomizer; activates parts by GameObject name.
/// </summary>
public class ModularHeroVisual : MonoBehaviour
{
    public Material heroMaterial;

    readonly List<GameObject> _enabledParts = new List<GameObject>();
    readonly Dictionary<string, GameObject> _partsByName = new Dictionary<string, GameObject>();
    readonly Dictionary<string, List<GameObject>> _listByPartName = new Dictionary<string, List<GameObject>>();

    CharacterObjectGroups _male = new CharacterObjectGroups();
    CharacterObjectGroups _female = new CharacterObjectGroups();
    CharacterObjectListsAllGender _allGender = new CharacterObjectListsAllGender();

    Transform _bodyScaleBone;
    bool _initialized;

    public bool IsInitialized => _initialized;

    public void Initialize()
    {
        if (_initialized && _partsByName.Count > 0)
            return;

        _initialized = false;
        InitializeLists();
        BuildLists();
        CachePartLookup();
        _bodyScaleBone = FindChildTransform("Root") ?? transform;

        if (!heroMaterial)
        {
            foreach (var renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (renderer.sharedMaterial)
                {
                    heroMaterial = renderer.sharedMaterial;
                    break;
                }
            }
        }

        _initialized = true;
    }

    public void ApplyAppearance(HeroAppearanceProfile profile, bool applyColors = true)
    {
        if (!profile)
            return;

        Initialize();

        ClearAllEnabledParts();

        var body = profile.gender == Gender.Female ? _female : _male;

        ActivatePart(profile.head);
        ActivatePart(profile.eyebrow);
        ActivatePart(profile.facialHair);
        ActivatePart(profile.hair);
        ActivatePart(profile.headCovering);

        ActivatePart(profile.torso);
        ActivatePart(profile.armUpperRight);
        ActivatePart(profile.armUpperLeft);
        ActivatePart(profile.armLowerRight);
        ActivatePart(profile.armLowerLeft);
        ActivatePart(profile.handRight);
        ActivatePart(profile.handLeft);
        ActivatePart(profile.hips);
        ActivatePart(profile.legRight);
        ActivatePart(profile.legLeft);

        if (applyColors)
            ApplyColors(profile);

        if (_bodyScaleBone)
            _bodyScaleBone.localScale = profile.bodyScale;
    }

    public void ApplyEquipment(HeroEquipmentProfile equipment, HeroAppearanceProfile appearance)
    {
        if (!equipment || !appearance)
            return;

        Initialize();

        ClearList(_allGender.shoulder_Attachment_Right);
        ClearList(_allGender.shoulder_Attachment_Left);
        ClearList(_allGender.back_Attachment);
        ClearList(_allGender.hips_Attachment);
        ClearList(_allGender.knee_Attachement_Right);
        ClearList(_allGender.knee_Attachement_Left);
        ClearList(_allGender.all_Head_Attachment);
        ClearList(_allGender.headCoverings_Base_Hair);
        ClearList(_allGender.headCoverings_No_FacialHair);
        ClearList(_allGender.headCoverings_No_Hair);

        ApplySlotParts(equipment.body, appearance);
        ApplySlotParts(equipment.forearm, appearance);
        ApplySlotParts(equipment.hips, appearance);
        ApplySlotParts(equipment.leg, appearance);
        ApplySlotParts(equipment.shoulder, appearance);
        ApplySlotParts(equipment.back, appearance);

        ApplyHeadEquipment(equipment, appearance);
    }

    void ApplySlotParts(string combinedParts, HeroAppearanceProfile appearance)
    {
        foreach (var partName in EquipmentPartParser.Split(combinedParts))
            ApplyEquipmentPart(partName.Trim(), appearance);
    }

    void ApplyEquipmentPart(string partName, HeroAppearanceProfile appearance)
    {
        if (string.IsNullOrEmpty(partName))
            return;

        partName = EquipmentPartGenderResolver.Resolve(partName.Trim(), appearance.gender);

        if (SyntyEquipmentPartClassifier.IsAttachmentPart(partName))
        {
            ActivateOptionalPart(partName);
            return;
        }

        SetBodyPartOrBase(partName, GetAppearanceFallback(partName, appearance));
    }

    static string GetAppearanceFallback(string partName, HeroAppearanceProfile appearance)
    {
        if (partName.StartsWith("Chr_Torso_")) return appearance.torso;
        if (partName.StartsWith("Chr_ArmUpperRight_")) return appearance.armUpperRight;
        if (partName.StartsWith("Chr_ArmUpperLeft_")) return appearance.armUpperLeft;
        if (partName.StartsWith("Chr_ArmLowerRight_")) return appearance.armLowerRight;
        if (partName.StartsWith("Chr_ArmLowerLeft_")) return appearance.armLowerLeft;
        if (partName.StartsWith("Chr_HandRight_")) return appearance.handRight;
        if (partName.StartsWith("Chr_HandLeft_")) return appearance.handLeft;
        if (partName.StartsWith("Chr_Hips_")) return appearance.hips;
        if (partName.StartsWith("Chr_LegRight_")) return appearance.legRight;
        if (partName.StartsWith("Chr_LegLeft_")) return appearance.legLeft;
        return string.Empty;
    }

    void ApplyHeadEquipment(HeroEquipmentProfile equipment, HeroAppearanceProfile appearance)
    {
        var hasHeadGear = !string.IsNullOrEmpty(equipment.head);

        if (hasHeadGear)
        {
            if (equipment.hideHairWhenHeadEquipped)
                ClearList(_allGender.all_Hair);
            else
                ActivatePart(appearance.hair);

            ApplySlotParts(equipment.head, appearance);
        }
        else
        {
            ActivatePart(appearance.hair);
            ActivatePart(appearance.headCovering);
        }
    }

    void ApplyColors(HeroAppearanceProfile profile)
    {
        if (!heroMaterial)
            return;

        heroMaterial.SetColor("_Color_Skin", profile.skinColor);
        heroMaterial.SetColor("_Color_Hair", profile.hairColor);
        heroMaterial.SetColor("_Color_Stubble", profile.stubbleColor);
        heroMaterial.SetColor("_Color_Scar", profile.scarColor);
        heroMaterial.SetColor("_Color_Primary", profile.primaryColor);
        heroMaterial.SetColor("_Color_Secondary", profile.secondaryColor);
        heroMaterial.SetColor("_Color_Metal_Primary", profile.metalPrimaryColor);
        heroMaterial.SetColor("_Color_Metal_Secondary", profile.metalSecondaryColor);
        heroMaterial.SetColor("_Color_Leather_Primary", profile.leatherPrimaryColor);
        heroMaterial.SetColor("_Color_Leather_Secondary", profile.leatherSecondaryColor);
    }

    void SetBodyPartOrBase(string equipmentPart, string basePart)
    {
        if (string.IsNullOrEmpty(equipmentPart))
        {
            ActivatePart(basePart);
            return;
        }

        if (!string.IsNullOrEmpty(basePart))
            DeactivatePart(basePart);

        ActivatePart(equipmentPart);
    }

    void DeactivatePart(string partName)
    {
        if (string.IsNullOrEmpty(partName))
            return;

        if (!_partsByName.TryGetValue(partName, out var part))
            return;

        if (!part.activeSelf)
            return;

        part.SetActive(false);
        _enabledParts.Remove(part);
    }

    void ActivateOptionalPart(string partName)
    {
        if (!string.IsNullOrEmpty(partName))
            ActivatePart(partName);
    }

    public void ActivatePart(string partName)
    {
        if (string.IsNullOrEmpty(partName))
            return;

        if (!_listByPartName.TryGetValue(partName, out var list))
        {
            var fallback = FindChildTransform(partName);
            if (fallback && fallback.GetComponent<SkinnedMeshRenderer>())
            {
                EnablePart(fallback.gameObject);
                return;
            }

            Debug.LogWarning($"[ModularHeroVisual] Part not found: {partName}", this);
            return;
        }

        ClearList(list);

        if (_partsByName.TryGetValue(partName, out var part))
            EnablePart(part);
    }

    void EnablePart(GameObject part)
    {
        part.SetActive(true);
        _enabledParts.Add(part);
    }

    void ClearList(List<GameObject> list)
    {
        if (list == null)
            return;

        foreach (var part in list)
        {
            if (part.activeSelf)
            {
                part.SetActive(false);
                _enabledParts.Remove(part);
            }
        }
    }

    void ClearAllEnabledParts()
    {
        for (var i = _enabledParts.Count - 1; i >= 0; i--)
            _enabledParts[i].SetActive(false);
        _enabledParts.Clear();
    }

    void CachePartLookup()
    {
        _partsByName.Clear();
        _listByPartName.Clear();

        RegisterLists(GetAllLists());
    }

    void RegisterLists(IEnumerable<List<GameObject>> lists)
    {
        foreach (var list in lists)
        {
            if (list == null)
                continue;

            foreach (var part in list)
            {
                if (!part)
                    continue;

                _partsByName[part.name] = part;
                _listByPartName[part.name] = list;
            }
        }
    }

    IEnumerable<List<GameObject>> GetAllLists()
    {
        yield return _male.headAllElements;
        yield return _male.headNoElements;
        yield return _male.eyebrow;
        yield return _male.facialHair;
        yield return _male.torso;
        yield return _male.arm_Upper_Right;
        yield return _male.arm_Upper_Left;
        yield return _male.arm_Lower_Right;
        yield return _male.arm_Lower_Left;
        yield return _male.hand_Right;
        yield return _male.hand_Left;
        yield return _male.hips;
        yield return _male.leg_Right;
        yield return _male.leg_Left;

        yield return _female.headAllElements;
        yield return _female.headNoElements;
        yield return _female.eyebrow;
        yield return _female.facialHair;
        yield return _female.torso;
        yield return _female.arm_Upper_Right;
        yield return _female.arm_Upper_Left;
        yield return _female.arm_Lower_Right;
        yield return _female.arm_Lower_Left;
        yield return _female.hand_Right;
        yield return _female.hand_Left;
        yield return _female.hips;
        yield return _female.leg_Right;
        yield return _female.leg_Left;

        yield return _allGender.all_Hair;
        yield return _allGender.all_Head_Attachment;
        yield return _allGender.headCoverings_Base_Hair;
        yield return _allGender.headCoverings_No_FacialHair;
        yield return _allGender.headCoverings_No_Hair;
        yield return _allGender.chest_Attachment;
        yield return _allGender.back_Attachment;
        yield return _allGender.shoulder_Attachment_Right;
        yield return _allGender.shoulder_Attachment_Left;
        yield return _allGender.elbow_Attachment_Right;
        yield return _allGender.elbow_Attachment_Left;
        yield return _allGender.hips_Attachment;
        yield return _allGender.knee_Attachement_Right;
        yield return _allGender.knee_Attachement_Left;
        yield return _allGender.elf_Ear;
    }

    void InitializeLists()
    {
        InitGroup(_male);
        InitGroup(_female);

        _allGender.all_Hair = new List<GameObject>();
        _allGender.all_Head_Attachment = new List<GameObject>();
        _allGender.headCoverings_Base_Hair = new List<GameObject>();
        _allGender.headCoverings_No_FacialHair = new List<GameObject>();
        _allGender.headCoverings_No_Hair = new List<GameObject>();
        _allGender.chest_Attachment = new List<GameObject>();
        _allGender.back_Attachment = new List<GameObject>();
        _allGender.shoulder_Attachment_Right = new List<GameObject>();
        _allGender.shoulder_Attachment_Left = new List<GameObject>();
        _allGender.elbow_Attachment_Right = new List<GameObject>();
        _allGender.elbow_Attachment_Left = new List<GameObject>();
        _allGender.hips_Attachment = new List<GameObject>();
        _allGender.knee_Attachement_Right = new List<GameObject>();
        _allGender.knee_Attachement_Left = new List<GameObject>();
        _allGender.all_12_Extra = new List<GameObject>();
        _allGender.elf_Ear = new List<GameObject>();
    }

    static void InitGroup(CharacterObjectGroups group)
    {
        group.headAllElements = new List<GameObject>();
        group.headNoElements = new List<GameObject>();
        group.eyebrow = new List<GameObject>();
        group.facialHair = new List<GameObject>();
        group.torso = new List<GameObject>();
        group.arm_Upper_Right = new List<GameObject>();
        group.arm_Upper_Left = new List<GameObject>();
        group.arm_Lower_Right = new List<GameObject>();
        group.arm_Lower_Left = new List<GameObject>();
        group.hand_Right = new List<GameObject>();
        group.hand_Left = new List<GameObject>();
        group.hips = new List<GameObject>();
        group.leg_Right = new List<GameObject>();
        group.leg_Left = new List<GameObject>();
    }

    void BuildLists()
    {
        BuildList(_male.headAllElements, "Male_Head_All_Elements");
        BuildList(_male.headNoElements, "Male_Head_No_Elements");
        BuildList(_male.eyebrow, "Male_01_Eyebrows");
        BuildList(_male.facialHair, "Male_02_FacialHair");
        BuildList(_male.torso, "Male_03_Torso");
        BuildList(_male.arm_Upper_Right, "Male_04_Arm_Upper_Right");
        BuildList(_male.arm_Upper_Left, "Male_05_Arm_Upper_Left");
        BuildList(_male.arm_Lower_Right, "Male_06_Arm_Lower_Right");
        BuildList(_male.arm_Lower_Left, "Male_07_Arm_Lower_Left");
        BuildList(_male.hand_Right, "Male_08_Hand_Right");
        BuildList(_male.hand_Left, "Male_09_Hand_Left");
        BuildList(_male.hips, "Male_10_Hips");
        BuildList(_male.leg_Right, "Male_11_Leg_Right");
        BuildList(_male.leg_Left, "Male_12_Leg_Left");

        BuildList(_female.headAllElements, "Female_Head_All_Elements");
        BuildList(_female.headNoElements, "Female_Head_No_Elements");
        BuildList(_female.eyebrow, "Female_01_Eyebrows");
        BuildList(_female.facialHair, "Female_02_FacialHair");
        BuildList(_female.torso, "Female_03_Torso");
        BuildList(_female.arm_Upper_Right, "Female_04_Arm_Upper_Right");
        BuildList(_female.arm_Upper_Left, "Female_05_Arm_Upper_Left");
        BuildList(_female.arm_Lower_Right, "Female_06_Arm_Lower_Right");
        BuildList(_female.arm_Lower_Left, "Female_07_Arm_Lower_Left");
        BuildList(_female.hand_Right, "Female_08_Hand_Right");
        BuildList(_female.hand_Left, "Female_09_Hand_Left");
        BuildList(_female.hips, "Female_10_Hips");
        BuildList(_female.leg_Right, "Female_11_Leg_Right");
        BuildList(_female.leg_Left, "Female_12_Leg_Left");

        BuildList(_allGender.all_Hair, "All_01_Hair");
        BuildList(_allGender.all_Head_Attachment, "All_02_Head_Attachment");
        BuildList(_allGender.headCoverings_Base_Hair, "HeadCoverings_Base_Hair");
        BuildList(_allGender.headCoverings_No_FacialHair, "HeadCoverings_No_FacialHair");
        BuildList(_allGender.headCoverings_No_Hair, "HeadCoverings_No_Hair");
        BuildList(_allGender.chest_Attachment, "All_03_Chest_Attachment");
        BuildList(_allGender.back_Attachment, "All_04_Back_Attachment");
        BuildList(_allGender.shoulder_Attachment_Right, "All_05_Shoulder_Attachment_Right");
        BuildList(_allGender.shoulder_Attachment_Left, "All_06_Shoulder_Attachment_Left");
        BuildList(_allGender.elbow_Attachment_Right, "All_07_Elbow_Attachment_Right");
        BuildList(_allGender.elbow_Attachment_Left, "All_08_Elbow_Attachment_Left");
        BuildList(_allGender.hips_Attachment, "All_09_Hips_Attachment");
        BuildList(_allGender.knee_Attachement_Right, "All_10_Knee_Attachement_Right");
        BuildList(_allGender.knee_Attachement_Left, "All_11_Knee_Attachement_Left");
        BuildList(_allGender.elf_Ear, "Elf_Ear");
    }

    void BuildList(List<GameObject> targetList, string folderName)
    {
        var folder = FindChildTransform(folderName);
        if (!folder)
        {
            Debug.LogWarning($"[ModularHeroVisual] Folder not found: {folderName}", this);
            return;
        }

        targetList.Clear();
        CollectMeshParts(folder, targetList);
    }

    static void CollectMeshParts(Transform root, List<GameObject> targetList)
    {
        for (var i = 0; i < root.childCount; i++)
        {
            var child = root.GetChild(i);
            if (child.GetComponent<SkinnedMeshRenderer>())
            {
                var part = child.gameObject;
                part.SetActive(false);
                targetList.Add(part);
                continue;
            }

            CollectMeshParts(child, targetList);
        }
    }

    Transform FindChildTransform(string childName)
    {
        foreach (var t in GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName)
                return t;
        }

        return null;
    }
}
