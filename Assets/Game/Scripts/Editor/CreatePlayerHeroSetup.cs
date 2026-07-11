#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreatePlayerHeroSetup
{
    const string SourcePrefabPath =
        "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/FixedScale/ModularCharacter_01.prefab";

    const string OutputPrefabPath = "Assets/Game/Prefabs/Characters/PlayerHero_Base.prefab";
    const string DataFolder = "Assets/Game/Data/Character";
    const string AppearanceAssetPath = DataFolder + "/HeroAppearance_DefaultMale.asset";
    const string EquipAAssetPath = DataFolder + "/HeroEquip_ClothBase.asset";
    const string EquipBAssetPath = DataFolder + "/HeroEquip_LeatherWarrior.asset";

    const string MatPath = "Assets/Synty/PolygonFantasyHeroCharacters/Materials/FantasyHero.mat";
    const string SwordPath = "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Weapons/SM_Wep_Sword_01.prefab";
    const string ShieldPath = "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Weapons/SM_Wep_Shield_Buckler_01.prefab";
    [MenuItem("Game/Character/Create Player Hero Base + 2 Equipment Sets")]
    public static void CreateAll()
    {
        EnsureFolder("Assets/Game/Prefabs/Characters");
        EnsureFolder(DataFolder);

        var appearance = LoadOrCreateAppearance();
        var equipA = LoadOrCreateEquipA();
        var equipB = LoadOrCreateEquipB();
        CreatePlayerPrefab(appearance, equipA, equipB);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Player Hero Setup",
            "Created:\n" +
            $"- {OutputPrefabPath}\n" +
            $"- {AppearanceAssetPath}\n" +
            $"- {EquipAAssetPath} (Cloth Base)\n" +
            $"- {EquipBAssetPath} (Leather Warrior)\n\n" +
            "Drag PlayerHero_Base into a scene and press Play.\n" +
            "Assign weapons on HeroWeaponVisual (back mount prefabs or children under mount sockets).\n" +
            "Menu: Game/Character/Ensure Weapon Mount Sockets On Selected\n" +
            "Use [1] to cycle equipment sets from EquipmentSets.csv.\n\n" +
            "Controls: LMB select, RMB move, E stance (standing), R walk/run (casual), MMB orbit, WASD pan, scroll zoom.\n" +
            "Menu: Game/Camera/Add Kenshi Camera Controller To Main Camera",
            "OK");
    }

    static HeroAppearanceProfile LoadOrCreateAppearance()
    {
        var asset = AssetDatabase.LoadAssetAtPath<HeroAppearanceProfile>(AppearanceAssetPath);
        if (asset)
            return asset;

        asset = ScriptableObject.CreateInstance<HeroAppearanceProfile>();
        AssetDatabase.CreateAsset(asset, AppearanceAssetPath);
        return asset;
    }

    static HeroEquipmentProfile LoadOrCreateEquipA()
    {
        var asset = AssetDatabase.LoadAssetAtPath<HeroEquipmentProfile>(EquipAAssetPath);
        if (!asset)
        {
            asset = ScriptableObject.CreateInstance<HeroEquipmentProfile>();
            AssetDatabase.CreateAsset(asset, EquipAAssetPath);
        }

        asset.setName = "Cloth Base";
        asset.setIndex = 0;
        asset.head = string.Empty;
        asset.body = string.Empty;
        asset.shoulder = string.Empty;
        asset.forearm = string.Empty;
        asset.hips = string.Empty;
        asset.leg = string.Empty;
        asset.back = string.Empty;
        EditorUtility.SetDirty(asset);
        return asset;
    }

    static HeroEquipmentProfile LoadOrCreateEquipB()
    {
        var asset = AssetDatabase.LoadAssetAtPath<HeroEquipmentProfile>(EquipBAssetPath);
        if (!asset)
        {
            asset = ScriptableObject.CreateInstance<HeroEquipmentProfile>();
            AssetDatabase.CreateAsset(asset, EquipBAssetPath);
        }

        asset.setName = "Leather Warrior";
        asset.setIndex = 1;
        asset.body = EquipmentPartParser.MergeCombined(
            "Chr_Torso_Male_06;Chr_ArmUpperRight_Male_02;Chr_ArmUpperLeft_Male_02",
            "Chr_ShoulderAttachRight_04;Chr_ShoulderAttachLeft_04");
        asset.shoulder = string.Empty;
        asset.forearm = "Chr_ArmLowerRight_Male_03;Chr_ArmLowerLeft_Male_03;Chr_HandRight_Male_02;Chr_HandLeft_Male_02";
        asset.hips = "Chr_Hips_Male_07";
        asset.leg = "Chr_LegRight_Male_03;Chr_LegLeft_Male_03";
        asset.back = "Chr_BackAttachment_01";
        EditorUtility.SetDirty(asset);
        return asset;
    }

    static void CreatePlayerPrefab(
        HeroAppearanceProfile appearance,
        HeroEquipmentProfile equipA,
        HeroEquipmentProfile equipB)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
        if (!source)
        {
            EditorUtility.DisplayDialog("Error", $"Source prefab not found:\n{SourcePrefabPath}", "OK");
            return;
        }

        var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        if (!instance)
            return;

        instance.name = "PlayerHero_Base";

        var randomizer = instance.GetComponent<PsychoticLab.CharacterRandomizer>();
        if (randomizer)
            Object.DestroyImmediate(randomizer);

        var visual = instance.GetComponent<ModularHeroVisual>();
        if (!visual)
            visual = instance.AddComponent<ModularHeroVisual>();

        visual.heroMaterial = AssetDatabase.LoadAssetAtPath<Material>(MatPath);

        var hero = instance.GetComponent<PlayerHeroEntity>();
        if (!hero)
            hero = instance.AddComponent<PlayerHeroEntity>();

        hero.visual = visual;
        hero.weaponVisual = instance.GetComponent<HeroWeaponVisual>();
        hero.appearance = appearance;
        hero.equipmentSetA = equipA;
        hero.equipmentSetB = equipB;
        hero.activeEquipment = equipA;

        var debug = instance.GetComponent<HeroEquipmentDebug>();
        if (!debug)
            debug = instance.AddComponent<HeroEquipmentDebug>();
        debug.hero = hero;

        EnsurePlayerControlComponents(instance);
        EnsureWeaponMountSockets(instance);

        var weaponVisual = instance.GetComponent<HeroWeaponVisual>();
        if (weaponVisual)
        {
            var sword = AssetDatabase.LoadAssetAtPath<GameObject>(SwordPath);
            if (sword && !weaponVisual.mainHand.weaponPrefab)
                weaponVisual.mainHand.weaponPrefab = sword;
        }

        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(OutputPrefabPath);
        if (existing)
            PrefabUtility.SaveAsPrefabAsset(instance, OutputPrefabPath);
        else
            PrefabUtility.SaveAsPrefabAsset(instance, OutputPrefabPath);

        Object.DestroyImmediate(instance);

        Debug.Log($"[CreatePlayerHeroSetup] Saved {OutputPrefabPath}");
    }

    [MenuItem("Game/Character/Add Player Control To Selected")]
    static void AddPlayerControlToSelected()
    {
        var go = Selection.activeGameObject;
        if (!go)
        {
            EditorUtility.DisplayDialog("Player Control", "Select a hero GameObject or prefab root in the hierarchy.", "OK");
            return;
        }

        EnsurePlayerControlComponents(go);
        EnsureWeaponMountSockets(go);
        EditorUtility.SetDirty(go);
        Debug.Log("[CreatePlayerHeroSetup] Added Player control components to " + go.name);
    }

    [MenuItem("Game/Character/Ensure Weapon Mount Sockets On Selected")]
    static void EnsureWeaponMountSocketsOnSelected()
    {
        var selected = Selection.activeGameObject;
        if (!selected)
        {
            EditorUtility.DisplayDialog("Weapon Mounts", "Select a hero GameObject in the hierarchy.", "OK");
            return;
        }

        var instance = ResolveHeroRoot(selected);
        if (!instance)
        {
            EditorUtility.DisplayDialog("Weapon Mounts", "Could not find hero root on selection.", "OK");
            return;
        }

        var weaponVisual = instance.GetComponent<HeroWeaponVisual>();
        if (!weaponVisual)
            weaponVisual = instance.AddComponent<HeroWeaponVisual>();

        EnsureWeaponMountSockets(instance);
        EditorUtility.SetDirty(instance);
        EditorUtility.SetDirty(weaponVisual);
        Debug.Log("[CreatePlayerHeroSetup] Ensured weapon mount sockets on " + instance.name);
    }

    static GameObject ResolveHeroRoot(GameObject selected)
    {
        var hero = selected.GetComponentInParent<PlayerHeroEntity>();
        if (hero)
            return hero.gameObject;

        var weaponVisual = selected.GetComponentInParent<HeroWeaponVisual>();
        if (weaponVisual)
            return weaponVisual.gameObject;

        if (selected.GetComponent<HeroWeaponVisual>() || selected.GetComponent<PlayerController>())
            return selected;

        return null;
    }

    public static void EnsureWeaponMountSockets(GameObject instance)
    {
        var weaponVisual = instance.GetComponent<HeroWeaponVisual>();
        if (!weaponVisual)
            weaponVisual = instance.AddComponent<HeroWeaponVisual>();

        var backBone = FindChildTransform(instance.transform, "Back_Attachment")
            ?? FindChildTransform(instance.transform, "Spine_02")
            ?? FindChildTransform(instance.transform, "Spine_03");

        if (!backBone)
            Debug.LogWarning("[CreatePlayerHeroSetup] No Back_Attachment / Spine bone found for back weapon mounts.", instance);

        var handRight = FindChildTransform(instance.transform, "Hand_R");
        var handLeft = FindChildTransform(instance.transform, "Hand_L");

        if (!handRight)
            Debug.LogWarning("[CreatePlayerHeroSetup] Hand_R bone not found.", instance);
        if (!handLeft)
            Debug.LogWarning("[CreatePlayerHeroSetup] Hand_L bone not found.", instance);

        weaponVisual.mainHand.handSocket = EnsureHandMountSocket(
            instance.transform,
            handRight,
            weaponVisual.mainHand.handSocket,
            "Socket_WeaponHand_Main",
            weaponVisual.mainHandLocalPosition,
            weaponVisual.mainHandLocalEuler);

        weaponVisual.offHand.handSocket = EnsureHandMountSocket(
            instance.transform,
            handLeft,
            weaponVisual.offHand.handSocket,
            "Socket_WeaponHand_Off",
            weaponVisual.offHandLocalPosition,
            weaponVisual.offHandLocalEuler);

        if (backBone)
        {
            weaponVisual.mainHand.backMountSocket = weaponVisual.mainHand.backMountSocket
                ?? FindOrCreateSocket(backBone, "Socket_WeaponMount_Main",
                    weaponVisual.mainBackLocalPosition, weaponVisual.mainBackLocalEuler);
            weaponVisual.offHand.backMountSocket = weaponVisual.offHand.backMountSocket
                ?? FindOrCreateSocket(backBone, "Socket_WeaponMount_Off",
                    weaponVisual.offBackLocalPosition, weaponVisual.offBackLocalEuler);
        }

        EditorUtility.SetDirty(weaponVisual);
    }

    static Transform EnsureHandMountSocket(
        Transform heroRoot,
        Transform handBone,
        Transform currentSocket,
        string socketName,
        Vector3 localPos,
        Vector3 localEuler)
    {
        var existing = FindChildTransform(heroRoot, socketName);
        if (existing)
            return existing;

        if (currentSocket && currentSocket.name == socketName)
            return currentSocket;

        if (!handBone)
            return currentSocket;

        if (!currentSocket || IsBareHandBone(currentSocket))
            return FindOrCreateSocket(handBone, socketName, localPos, localEuler);

        return currentSocket;
    }

    static bool IsBareHandBone(Transform t)
    {
        return t && (t.name == "Hand_R" || t.name == "Hand_L");
    }

    static Transform FindOrCreateSocket(Transform parent, string name, Vector3 localPos, Vector3 localEuler)
    {
        var existing = parent.Find(name);
        if (existing)
            return existing;

        var socketGo = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(socketGo, "Create Weapon Mount Socket");
        var socket = socketGo.transform;
        socket.SetParent(parent, false);
        socket.localPosition = localPos;
        socket.localEulerAngles = localEuler;
        return socket;
    }

    static Transform FindChildTransform(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == name)
                return t;
        }

        return null;
    }

    public static void EnsurePlayerControlComponents(GameObject instance)
    {
        if (!instance.GetComponent<PlayerController>())
            instance.AddComponent<PlayerController>();

        if (!instance.GetComponent<PlayerStanceController>())
            instance.AddComponent<PlayerStanceController>();

        if (!instance.GetComponent<HeroWeaponVisual>())
            instance.AddComponent<HeroWeaponVisual>();

        var cc = instance.GetComponent<CharacterController>();
        if (!cc)
            cc = instance.AddComponent<CharacterController>();

        cc.height = 1.75f;
        cc.radius = 0.35f;
        cc.center = new Vector3(0f, 0.88f, 0f);
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        if (!instance.CompareTag("Player"))
            instance.tag = "Player";
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
