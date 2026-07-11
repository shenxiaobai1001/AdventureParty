using System.Collections.Generic;
using PsychoticLab;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Dresses a mannequin for slot display. Shared by Icon Studio and world pickup prefab generation.
/// </summary>
public static class EquipmentDisplayAssembler
{
    public const string MannequinPrefabPath =
        "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/FixedScale/ModularCharacter_01.prefab";

    public const string DefaultAppearancePath =
        "Assets/Game/Data/Character/HeroAppearance_DefaultMale.asset";

    public struct AssemblyRequest
    {
        public int setId;
        public string setName;
        public SyntyEquipmentSlot slot;
        public string[] parts;
        public Material materialOverride;
    }

    public static AssemblyRequest FromEntry(IconRenderEntry entry, Material materialOverride = null)
    {
        return new AssemblyRequest
        {
            setId = entry.setId,
            setName = entry.setName,
            slot = entry.slot,
            parts = entry.parts,
            materialOverride = materialOverride,
        };
    }

#if UNITY_EDITOR
    public static GameObject Assemble(AssemblyRequest request, Transform parent)
    {
        if (request.parts == null || request.parts.Length == 0 || !parent)
            return null;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MannequinPrefabPath);
        if (!prefab)
        {
            Debug.LogError($"[EquipmentDisplay] Mannequin prefab not found: {MannequinPrefabPath}");
            return null;
        }

        var mannequin = Object.Instantiate(prefab, parent);
        mannequin.name = "EquipmentDisplay";
        mannequin.transform.localPosition = Vector3.zero;
        mannequin.transform.localRotation = Quaternion.identity;
        mannequin.transform.localScale = Vector3.one;

        DisableAnimator(mannequin);

        var gender = DetectGender(request.parts);
        SetGenderPartRoots(mannequin.transform, gender);

        var visual = mannequin.GetComponent<ModularHeroVisual>();
        if (!visual)
            visual = mannequin.AddComponent<ModularHeroVisual>();

        if (request.materialOverride)
            visual.heroMaterial = request.materialOverride;

        var appearance = CreateAppearanceProfile(gender);
        var equipment = CreateEquipmentProfile(request);

        visual.Initialize();
        visual.ApplyAppearance(appearance, applyColors: false);
        DressParts(visual, request, equipment, appearance);
        EquipmentSlotPose.Apply(mannequin.transform, request.slot);
        HideEverythingExcept(mannequin.transform, request.parts);
        IconStudioSkinnedBounds.PrepareSkinnedMeshes(mannequin.transform);
        CenterVisibleRenderers(mannequin.transform, parent);

        if (appearance && appearance != LoadDefaultAppearanceAsset())
            Object.DestroyImmediate(appearance);

        if (equipment)
            Object.DestroyImmediate(equipment);

        return mannequin;
    }

    public static GameObject BakeToStaticVisual(GameObject dressedMannequin, Transform parent)
    {
        if (!dressedMannequin)
            return null;

        var visualRoot = new GameObject("VisualRoot");
        visualRoot.transform.SetParent(parent, false);

        var skinnedParts = new List<GameObject>();
        foreach (var renderer in dressedMannequin.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            skinnedParts.Add(renderer.gameObject);
        }

        IconStudioSkinnedMeshBaker.BakeAndDisableSources(skinnedParts, visualRoot.transform);

        Object.DestroyImmediate(dressedMannequin);
        return visualRoot;
    }

    static void DisableAnimator(GameObject mannequin)
    {
        foreach (var animator in mannequin.GetComponentsInChildren<Animator>(true))
        {
            if (!animator)
                continue;

            animator.applyRootMotion = false;
            animator.enabled = false;
        }
    }

    static void SetGenderPartRoots(Transform root, Gender gender)
    {
        var maleParts = FindChild(root, "Male_Parts");
        var femaleParts = FindChild(root, "Female_Parts");

        if (maleParts)
            maleParts.gameObject.SetActive(gender == Gender.Male);

        if (femaleParts)
            femaleParts.gameObject.SetActive(gender == Gender.Female);
    }

    static Transform FindChild(Transform root, string childName)
    {
        foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (transform.name == childName)
                return transform;
        }

        return null;
    }

    static void DressParts(
        ModularHeroVisual visual,
        AssemblyRequest request,
        HeroEquipmentProfile equipment,
        HeroAppearanceProfile appearance)
    {
        if (request.slot == SyntyEquipmentSlot.Head)
        {
            visual.ApplyEquipment(equipment, appearance);
            return;
        }

        foreach (var partName in request.parts)
        {
            if (string.IsNullOrWhiteSpace(partName))
                continue;

            visual.ActivatePart(partName.Trim());
        }
    }

    static HeroAppearanceProfile LoadDefaultAppearanceAsset()
    {
        return AssetDatabase.LoadAssetAtPath<HeroAppearanceProfile>(DefaultAppearancePath);
    }

    static HeroAppearanceProfile CreateAppearanceProfile(Gender gender)
    {
        var asset = LoadDefaultAppearanceAsset();
        if (!asset)
            return CreateFallbackAppearance(gender);

        if (gender == Gender.Male)
            return asset;

        var profile = Object.Instantiate(asset);
        profile.gender = Gender.Female;
        profile.head = "Chr_Head_Female_01";
        profile.eyebrow = "Chr_Eyebrow_Female_01";
        profile.facialHair = string.Empty;
        profile.hair = "Chr_Hair_02";
        profile.headCovering = "Chr_HeadCoverings_Base_Hair_02";
        profile.torso = "Chr_Torso_Female_00";
        profile.armUpperRight = "Chr_ArmUpperRight_Female_00";
        profile.armUpperLeft = "Chr_ArmUpperLeft_Female_00";
        profile.armLowerRight = "Chr_ArmLowerRight_Female_00";
        profile.armLowerLeft = "Chr_ArmLowerLeft_Female_00";
        profile.handRight = "Chr_HandRight_Female_00";
        profile.handLeft = "Chr_HandLeft_Female_00";
        profile.hips = "Chr_Hips_Female_00";
        profile.legRight = "Chr_LegRight_Female_00";
        profile.legLeft = "Chr_LegLeft_Female_00";
        return profile;
    }

    static HeroAppearanceProfile CreateFallbackAppearance(Gender gender)
    {
        var profile = ScriptableObject.CreateInstance<HeroAppearanceProfile>();
        profile.gender = gender;
        if (gender == Gender.Female)
        {
            profile.head = "Chr_Head_Female_01";
            profile.torso = "Chr_Torso_Female_00";
            profile.armUpperRight = "Chr_ArmUpperRight_Female_00";
            profile.armUpperLeft = "Chr_ArmUpperLeft_Female_00";
            profile.armLowerRight = "Chr_ArmLowerRight_Female_00";
            profile.armLowerLeft = "Chr_ArmLowerLeft_Female_00";
            profile.handRight = "Chr_HandRight_Female_00";
            profile.handLeft = "Chr_HandLeft_Female_00";
            profile.hips = "Chr_Hips_Female_00";
            profile.legRight = "Chr_LegRight_Female_00";
            profile.legLeft = "Chr_LegLeft_Female_00";
        }

        return profile;
    }

    static HeroEquipmentProfile CreateEquipmentProfile(AssemblyRequest request)
    {
        var profile = ScriptableObject.CreateInstance<HeroEquipmentProfile>();
        profile.setIndex = request.setId;
        profile.setName = request.setName;
        profile.hideHairWhenHeadEquipped = request.slot == SyntyEquipmentSlot.Head;

        var joined = EquipmentPartParser.Join(request.parts);
        switch (request.slot)
        {
            case SyntyEquipmentSlot.Head: profile.head = joined; break;
            case SyntyEquipmentSlot.Body: profile.body = joined; break;
            case SyntyEquipmentSlot.Shoulder: profile.shoulder = joined; break;
            case SyntyEquipmentSlot.Forearm: profile.forearm = joined; break;
            case SyntyEquipmentSlot.Hips: profile.hips = joined; break;
            case SyntyEquipmentSlot.Leg: profile.leg = joined; break;
            case SyntyEquipmentSlot.Back: profile.back = joined; break;
        }

        return profile;
    }

    static Gender DetectGender(IEnumerable<string> parts)
    {
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            if (part.Contains("_Female_"))
                return Gender.Female;
        }

        return Gender.Male;
    }

    static void HideEverythingExcept(Transform root, string[] visiblePartNames)
    {
        var visible = new HashSet<string>();
        if (visiblePartNames != null)
        {
            foreach (var partName in visiblePartNames)
            {
                if (!string.IsNullOrWhiteSpace(partName))
                    visible.Add(partName.Trim());
            }
        }

        foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!renderer)
                continue;

            var keep = visible.Contains(renderer.gameObject.name);
            renderer.gameObject.SetActive(keep);
            renderer.enabled = keep;
        }
    }

    static void CenterVisibleRenderers(Transform mannequinRoot, Transform previewStage)
    {
        if (!TryGetActiveRendererBounds(mannequinRoot, out var bounds))
            return;

        var localCenter = previewStage.InverseTransformPoint(bounds.center);
        mannequinRoot.localPosition -= localCenter;
    }

    static bool TryGetActiveRendererBounds(Transform root, out Bounds bounds)
    {
        bounds = default;
        var hasBounds = false;

        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (!renderer || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }
#endif
}
