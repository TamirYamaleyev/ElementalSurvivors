#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Idempotent wiring of release gameplay systems into the dev SampleScene after a bad YAML merge.
/// Run: Unity -batchmode -quit -executeMethod DevMergeSceneSetup.WireReleaseSystemsFromCli
/// </summary>
public static class DevMergeSceneSetup
{
    const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    const string DamageNumberPrefabPath = "Assets/Prefabs/UI/DamageNumberView.prefab";
    const string ReactionVfxCatalogPath = "Assets/Data/ReactionVfxCatalog.asset";
    const string BranchPrefabPath = "Assets/Prefabs/Environment/PF_Environment_Branch.prefab";
    const string BushPrefabPath = "Assets/Prefabs/Environment/PF_Environment_Bush.prefab";
    const string DamageNumbersChildName = "DamageNumbers";
    const string CollectTriggerName = "CollectTrigger";
    const string ObstacleGeneratorName = "EnvironmentObstacleGenerator";
    const string PendingWireFlagPath = "Temp/devmerge-wire-pending";

    [InitializeOnLoadMethod]
    static void SchedulePendingWire()
    {
        EditorApplication.delayCall += TryRunPendingWire;
    }

    static void TryRunPendingWire()
    {
        if (!File.Exists(PendingWireFlagPath))
            return;

        File.Delete(PendingWireFlagPath);
        WireReleaseSystems(logToConsole: true);
    }

    /// <summary>Creates a flag file so the next domain reload wires SampleScene (use when batchmode is blocked).</summary>
    public static void QueuePendingWire()
    {
        Directory.CreateDirectory("Temp");
        File.WriteAllText(PendingWireFlagPath, "pending");
    }

    const string PlayerControllerPath = "Assets/Animation/Controllers/AC_Player.controller";
    const string VisualChildName = "Visual";
    const float VisualDisplayScale = 0.24f;

    [MenuItem("Tools/Dev Merge/Bind Enemy Art Animations")]
    public static void BindEnemyArtAnimationsMenu()
    {
        CharacterAnimationPipeline.BindDevTierEnemySprites();
    }

    [MenuItem("Tools/Dev Merge/Bind Dev Tier Enemy Sprites")]
    public static void BindDevTierEnemySpritesMenu()
    {
        CharacterAnimationPipeline.BindDevTierEnemySprites();
    }

    /// <summary>Unity -batchmode -quit -executeMethod DevMergeSceneSetup.RunFullPipelineFromCli</summary>
    public static void RunFullPipelineFromCli()
    {
        try
        {
            CharacterAnimationPipeline.BindArtFolderPlayerSprites();
            AddPlayerAnimatorVisual(logToConsole: false);
            CharacterAnimationPipeline.BindDevTierEnemySprites();
            WireReleaseSystems(logToConsole: false);
            FixLevelUpUi(FindScene(), new List<string>());
            EditorSceneManager.SaveScene(FindScene());
            VerifySampleScene();
            Debug.Log("[DevMergeSceneSetup] Full pipeline completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DevMergeSceneSetup] Full pipeline failed: " + ex);
            EditorApplication.Exit(1);
        }
    }

    static Scene FindScene()
    {
        return EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
    }

    /// <summary>Unity -batchmode -quit -executeMethod DevMergeSceneSetup.FixLevelUpUiFromCli</summary>
    public static void FixLevelUpUiFromCli()
    {
        try
        {
            var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            var checklist = new List<string>();
            FixLevelUpUi(scene, checklist);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[DevMergeSceneSetup] Level-up UI wired: " + string.Join(", ", checklist));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DevMergeSceneSetup] FixLevelUpUi: " + ex);
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("Tools/Dev Merge/Fix Level-Up UI Wiring")]
    public static void FixLevelUpUiMenu()
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var checklist = new List<string>();
        FixLevelUpUi(scene, checklist);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DevMergeSceneSetup] Level-up UI wired: " + string.Join(", ", checklist));
    }

    static void FixLevelUpUi(Scene scene, List<string> checklist)
    {
        var levelUpUi = Object.FindFirstObjectByType<LevelUpUI>();
        if (levelUpUi == null)
        {
            Debug.LogWarning("[DevMergeSceneSetup] LevelUpUI not found.");
            return;
        }

        var player = FindRootObject(scene, "Player");
        if (player == null)
            throw new System.InvalidOperationException("Player not found in SampleScene.");

        var spearWeapon = EnsurePlayerDefaultWeapon(player, checklist);
        var orbitWeapon = player.GetComponentInChildren<OrbitWeapon>(true);
        var boomerangWeapon = player.GetComponentInChildren<BoomerangController>(true);

        var uiSo = new SerializedObject(levelUpUi);
        if (spearWeapon != null && uiSo.FindProperty("spearWeapon").objectReferenceValue == null)
        {
            uiSo.FindProperty("spearWeapon").objectReferenceValue = spearWeapon;
            checklist.Add("LevelUpUI.spearWeapon");
        }

        if (orbitWeapon != null && uiSo.FindProperty("orbitWeapon").objectReferenceValue == null)
        {
            uiSo.FindProperty("orbitWeapon").objectReferenceValue = orbitWeapon;
            checklist.Add("LevelUpUI.orbitWeapon");
        }

        if (boomerangWeapon != null && uiSo.FindProperty("boomerangWeapon").objectReferenceValue == null)
        {
            uiSo.FindProperty("boomerangWeapon").objectReferenceValue = boomerangWeapon;
            checklist.Add("LevelUpUI.boomerangWeapon");
        }

        var option1 = FindSceneButton(scene, "Option1");
        var option2 = FindSceneButton(scene, "Option2");
        var option3 = FindSceneButton(scene, "Option3");

        if (option1 != null && uiSo.FindProperty("option1Button").objectReferenceValue == null)
        {
            uiSo.FindProperty("option1Button").objectReferenceValue = option1;
            checklist.Add("LevelUpUI.option1Button");
        }

        if (option2 != null && uiSo.FindProperty("option2Button").objectReferenceValue == null)
        {
            uiSo.FindProperty("option2Button").objectReferenceValue = option2;
            checklist.Add("LevelUpUI.option2Button");
        }

        if (option3 != null && uiSo.FindProperty("option3Button").objectReferenceValue == null)
        {
            uiSo.FindProperty("option3Button").objectReferenceValue = option3;
            checklist.Add("LevelUpUI.option3Button");
        }

        uiSo.ApplyModifiedPropertiesWithoutUndo();

        if (spearWeapon != null && option1 != null)
            WireButtonLevelUpTarget(option1, spearWeapon, levelUpUi, checklist, "Option1→PlayerDefaultWeapon");
        if (orbitWeapon != null && option2 != null)
            WireButtonLevelUpTarget(option2, orbitWeapon, levelUpUi, checklist, "Option2→OrbitWeapon");
        if (boomerangWeapon != null && option3 != null)
            WireButtonLevelUpTarget(option3, boomerangWeapon, levelUpUi, checklist, "Option3→BoomerangController");
    }

    static PlayerDefaultWeapon EnsurePlayerDefaultWeapon(GameObject player, List<string> checklist)
    {
        var spearWeapon = player.GetComponent<PlayerDefaultWeapon>();
        if (spearWeapon == null)
        {
            spearWeapon = player.AddComponent<PlayerDefaultWeapon>();
            checklist.Add("PlayerDefaultWeapon component");
        }

        var spearTransform = player.transform.Find("SpearHitbox");
        if (spearTransform == null)
        {
            var spearGo = new GameObject("SpearHitbox");
            spearGo.transform.SetParent(player.transform, false);
            spearGo.SetActive(false);
            spearTransform = spearGo.transform;
            checklist.Add("Player/SpearHitbox child");
        }

        var cam = Camera.main ?? Object.FindFirstObjectByType<Camera>();
        var weaponSo = new SerializedObject(spearWeapon);
        if (weaponSo.FindProperty("spear").objectReferenceValue == null)
            weaponSo.FindProperty("spear").objectReferenceValue = spearTransform;
        if (cam != null && weaponSo.FindProperty("cam").objectReferenceValue == null)
            weaponSo.FindProperty("cam").objectReferenceValue = cam;

        var characterAnimation = player.GetComponent<PlayerCharacterAnimation>();
        if (characterAnimation != null &&
            weaponSo.FindProperty("characterAnimation").objectReferenceValue == null)
            weaponSo.FindProperty("characterAnimation").objectReferenceValue = characterAnimation;

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null && weaponSo.FindProperty("statsProviderBehaviour").objectReferenceValue == null)
            weaponSo.FindProperty("statsProviderBehaviour").objectReferenceValue = stats;

        weaponSo.ApplyModifiedPropertiesWithoutUndo();
        return spearWeapon;
    }

    static Button FindSceneButton(Scene scene, string objectName)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.name == objectName)
                    return button;
            }
        }

        return null;
    }

    static void WireButtonLevelUpTarget(
        Button button,
        MonoBehaviour weaponTarget,
        LevelUpUI levelUpUi,
        List<string> checklist,
        string label)
    {
        var buttonSo = new SerializedObject(button);
        var calls = buttonSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        var changed = false;

        for (var i = 0; i < calls.arraySize; i++)
        {
            var call = calls.GetArrayElementAtIndex(i);
            var methodName = call.FindPropertyRelative("m_MethodName").stringValue;
            if (methodName != "LevelUp")
                continue;

            var targetProp = call.FindPropertyRelative("m_Target");
            if (targetProp.objectReferenceValue == weaponTarget)
                continue;

            targetProp.objectReferenceValue = weaponTarget;
            changed = true;
        }

        if (changed)
        {
            buttonSo.ApplyModifiedPropertiesWithoutUndo();
            checklist.Add(label);
        }

        WireChoiceSelectedTarget(button, levelUpUi, checklist);
    }

    static void WireChoiceSelectedTarget(Button button, LevelUpUI levelUpUi, List<string> checklist)
    {
        var buttonSo = new SerializedObject(button);
        var calls = buttonSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        var changed = false;

        for (var i = 0; i < calls.arraySize; i++)
        {
            var call = calls.GetArrayElementAtIndex(i);
            if (call.FindPropertyRelative("m_MethodName").stringValue != "ChoiceSelected")
                continue;

            var targetProp = call.FindPropertyRelative("m_Target");
            if (targetProp.objectReferenceValue == levelUpUi)
                continue;

            targetProp.objectReferenceValue = levelUpUi;
            changed = true;
        }

        if (changed)
        {
            buttonSo.ApplyModifiedPropertiesWithoutUndo();
            checklist.Add(button.name + "→LevelUpUI.ChoiceSelected");
        }
    }

    [MenuItem("Tools/Dev Merge/Add Player Animator Visual")]
    public static void AddPlayerAnimatorVisualMenu()
    {
        AddPlayerAnimatorVisual(logToConsole: true);
    }

    /// <summary>Unity -batchmode -quit -executeMethod DevMergeSceneSetup.AddPlayerAnimatorVisualFromCli</summary>
    public static void AddPlayerAnimatorVisualFromCli()
    {
        try
        {
            AddPlayerAnimatorVisual(logToConsole: true);
            Debug.Log("[DevMergeSceneSetup] Player animator CLI run completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DevMergeSceneSetup] " + ex);
            EditorApplication.Exit(1);
        }
    }

    static void AddPlayerAnimatorVisual(bool logToConsole)
    {
        EnsurePlayerAnimationAssets();

        if (!File.Exists("Assets/Art/Characters/Player/player_walk.png"))
            CharacterAnimationPipeline.SetupPlayerAnimatorInSampleScene();

        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var player = FindRootObject(scene, "Player");
        if (player == null)
            throw new System.InvalidOperationException("Player not found in SampleScene.");

        var checklist = new List<string>();
        if (player.GetComponent<PlayerCharacterAnimation>() != null)
            checklist.Add("PlayerCharacterAnimation on Player");
        if (player.transform.Find(VisualChildName) != null)
            checklist.Add("Player/Visual child");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var message = checklist.Count > 0
            ? "[DevMergeSceneSetup] Player animator visual ready:\n  - " + string.Join("\n  - ", checklist)
            : "[DevMergeSceneSetup] Player animator setup finished.";

        if (logToConsole)
            Debug.Log(message);
    }

    static void EnsurePlayerAnimationAssets()
    {
        if (File.Exists("Assets/Art/Characters/Player/player_walk.png") &&
            File.Exists("Assets/Art/Characters/Player/Player_attak.png"))
        {
            CharacterAnimationPipeline.BindArtFolderPlayerSprites();
            return;
        }

        if (File.Exists("Assets/Art/Characters/Player/Player_idle.png"))
        {
            CharacterAnimationPipeline.BindProductionCharacterSprites();
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/IMG_0600.png") != null)
        {
            CharacterAnimationPipeline.EnsureDevPlayerAnimationAssets();
            CharacterAnimationPipeline.SetupPlayerAnimatorInSampleScene();
            return;
        }

        CharacterAnimationPipeline.EnsurePlaceholderAnimationAssets();
        CharacterAnimationPipeline.SetupPlayerAnimatorInSampleScene();
    }

    [MenuItem("Tools/Dev Merge/Wire Release Systems Into SampleScene")]
    public static void WireReleaseSystemsMenu()
    {
        WireReleaseSystems(logToConsole: true);
    }

    /// <summary>Unity -batchmode -quit -executeMethod DevMergeSceneSetup.WireReleaseSystemsFromCli</summary>
    public static void WireReleaseSystemsFromCli()
    {
        try
        {
            WireReleaseSystems(logToConsole: true);
            Debug.Log("[DevMergeSceneSetup] CLI run completed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DevMergeSceneSetup] " + ex);
            EditorApplication.Exit(1);
        }
    }

    static void WireReleaseSystems(bool logToConsole)
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var checklist = new List<string>();

        WireDamageNumbers(scene, checklist);
        WireGameInstaller(scene, checklist);
        WireEnvironmentObstacleGenerator(scene, checklist);
        WirePlayerPickup(scene, checklist);
        FixLevelUpUi(scene, checklist);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var summary = string.Join("\n  - ", checklist);
        var message = checklist.Count > 0
            ? "[DevMergeSceneSetup] Wired release systems:\n  - " + summary
            : "[DevMergeSceneSetup] Scene already wired; no changes needed.";

        if (logToConsole)
            Debug.Log(message);
    }

    static void WireDamageNumbers(Scene scene, List<string> checklist)
    {
        var combatManager = FindRootObject(scene, "CombatManager");
        if (combatManager == null)
        {
            Debug.LogWarning("[DevMergeSceneSetup] CombatManager not found; skipping damage numbers.");
            return;
        }

        var hud = FindRootObject(scene, "HUD");
        if (hud == null)
        {
            Debug.LogWarning("[DevMergeSceneSetup] HUD not found; skipping damage numbers.");
            return;
        }

        var container = EnsureDamageNumbersContainer(hud.transform);
        if (container != null)
            checklist.Add("HUD/DamageNumbers container");

        var prefab = AssetDatabase.LoadAssetAtPath<DamageNumberView>(DamageNumberPrefabPath);
        if (prefab == null)
            Debug.LogWarning("[DevMergeSceneSetup] Missing prefab: " + DamageNumberPrefabPath);

        var display = combatManager.GetComponent<DamageNumberDisplay>();
        if (display == null)
        {
            display = combatManager.AddComponent<DamageNumberDisplay>();
            checklist.Add("DamageNumberDisplay on CombatManager");
        }

        var so = new SerializedObject(display);
        if (prefab != null && so.FindProperty("prefab").objectReferenceValue == null)
        {
            so.FindProperty("prefab").objectReferenceValue = prefab;
            checklist.Add("DamageNumberDisplay.prefab");
        }

        if (container != null && so.FindProperty("container").objectReferenceValue == null)
        {
            so.FindProperty("container").objectReferenceValue = container;
            checklist.Add("DamageNumberDisplay.container");
        }

        var mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        if (mainCamera != null && so.FindProperty("worldCamera").objectReferenceValue == null)
        {
            so.FindProperty("worldCamera").objectReferenceValue = mainCamera;
            checklist.Add("DamageNumberDisplay.worldCamera");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static RectTransform EnsureDamageNumbersContainer(Transform hud)
    {
        var existing = hud.Find(DamageNumbersChildName);
        if (existing != null)
            return existing as RectTransform;

        var go = new GameObject(DamageNumbersChildName, typeof(RectTransform));
        go.transform.SetParent(hud, false);
        go.layer = hud.gameObject.layer;

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.localScale = Vector3.one;
        rect.localPosition = Vector3.zero;

        return rect;
    }

    static void WireGameInstaller(Scene scene, List<string> checklist)
    {
        var combatManager = FindRootObject(scene, "CombatManager");
        if (combatManager == null)
            return;

        var installer = combatManager.GetComponent<GameInstaller>();
        if (installer == null)
            return;

        var catalog = AssetDatabase.LoadAssetAtPath<ReactionVfxCatalogSO>(ReactionVfxCatalogPath);
        if (catalog == null)
        {
            Debug.LogWarning("[DevMergeSceneSetup] Missing asset: " + ReactionVfxCatalogPath);
            return;
        }

        var so = new SerializedObject(installer);
        var catalogProp = so.FindProperty("reactionVfxCatalog");
        if (catalogProp.objectReferenceValue == catalog)
            return;

        catalogProp.objectReferenceValue = catalog;
        so.ApplyModifiedPropertiesWithoutUndo();
        checklist.Add("GameInstaller.reactionVfxCatalog");
    }

    static void WireEnvironmentObstacleGenerator(Scene scene, List<string> checklist)
    {
        var floor = FindRootObject(scene, "Floor");
        var floorRenderer = floor != null ? floor.GetComponent<SpriteRenderer>() : null;
        if (floorRenderer == null)
            Debug.LogWarning("[DevMergeSceneSetup] Floor SpriteRenderer not found.");

        var branch = AssetDatabase.LoadAssetAtPath<GameObject>(BranchPrefabPath);
        var bush = AssetDatabase.LoadAssetAtPath<GameObject>(BushPrefabPath);
        if (branch == null || bush == null)
            Debug.LogWarning("[DevMergeSceneSetup] Missing obstacle prefabs under Assets/Prefabs/Environment/.");

        var generatorGo = FindRootObject(scene, ObstacleGeneratorName);
        if (generatorGo == null)
        {
            generatorGo = new GameObject(ObstacleGeneratorName);
            SceneManager.MoveGameObjectToScene(generatorGo, scene);
            checklist.Add(ObstacleGeneratorName + " root");
        }

        var generator = generatorGo.GetComponent<EnvironmentObstacleGenerator>();
        if (generator == null)
        {
            generator = generatorGo.AddComponent<EnvironmentObstacleGenerator>();
            checklist.Add("EnvironmentObstacleGenerator component");
        }

        var so = new SerializedObject(generator);

        if (floorRenderer != null && so.FindProperty("floorRenderer").objectReferenceValue == null)
        {
            so.FindProperty("floorRenderer").objectReferenceValue = floorRenderer;
            checklist.Add("EnvironmentObstacleGenerator.floorRenderer");
        }

        var prefabsProp = so.FindProperty("obstaclePrefabs");
        if (prefabsProp.arraySize == 0 && branch != null && bush != null)
        {
            prefabsProp.arraySize = 2;
            prefabsProp.GetArrayElementAtIndex(0).objectReferenceValue = branch;
            prefabsProp.GetArrayElementAtIndex(1).objectReferenceValue = bush;
            checklist.Add("EnvironmentObstacleGenerator.obstaclePrefabs");
        }

        if (so.FindProperty("spawnCount").intValue != 55)
        {
            so.FindProperty("spawnCount").intValue = 55;
            checklist.Add("EnvironmentObstacleGenerator.spawnCount=55");
        }

        if (so.FindProperty("edgePadding").floatValue != 4f)
        {
            so.FindProperty("edgePadding").floatValue = 4f;
            checklist.Add("EnvironmentObstacleGenerator.edgePadding=4");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WirePlayerPickup(Scene scene, List<string> checklist)
    {
        var player = FindRootObject(scene, "Player");
        if (player == null)
        {
            Debug.LogWarning("[DevMergeSceneSetup] Player not found; skipping pickup wiring.");
            return;
        }

        var pickupFacade = player.GetComponent<PlayerPickupFacade>();
        if (pickupFacade == null)
        {
            pickupFacade = player.AddComponent<PlayerPickupFacade>();
            checklist.Add("PlayerPickupFacade on Player");
        }

        var pickupSo = new SerializedObject(pickupFacade);
        var playerExp = player.GetComponent<PlayerEXP>();
        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerExp != null && pickupSo.FindProperty("playerExp").objectReferenceValue == null)
        {
            pickupSo.FindProperty("playerExp").objectReferenceValue = playerExp;
            checklist.Add("PlayerPickupFacade.playerExp");
        }

        if (playerHealth != null && pickupSo.FindProperty("playerHealth").objectReferenceValue == null)
        {
            pickupSo.FindProperty("playerHealth").objectReferenceValue = playerHealth;
            checklist.Add("PlayerPickupFacade.playerHealth");
        }

        pickupSo.ApplyModifiedPropertiesWithoutUndo();

        var collectTrigger = player.transform.Find(CollectTriggerName);
        GameObject collectGo;
        if (collectTrigger == null)
        {
            collectGo = new GameObject(CollectTriggerName);
            collectGo.transform.SetParent(player.transform, false);
            collectGo.transform.localPosition = Vector3.zero;
            collectGo.layer = player.layer;
            collectGo.AddComponent<CircleCollider2D>();
            collectGo.AddComponent<CollectRadiusController>();
            checklist.Add("Player/CollectTrigger");
        }
        else
        {
            collectGo = collectTrigger.gameObject;
            if (collectGo.GetComponent<CircleCollider2D>() == null)
                collectGo.AddComponent<CircleCollider2D>();
            if (collectGo.GetComponent<CollectRadiusController>() == null)
                collectGo.AddComponent<CollectRadiusController>();
        }

        var collider = collectGo.GetComponent<CircleCollider2D>();
        collider.isTrigger = true;

        var collectController = collectGo.GetComponent<CollectRadiusController>();
        var collectSo = new SerializedObject(collectController);

        if (collectSo.FindProperty("pickupFacade").objectReferenceValue == null)
        {
            collectSo.FindProperty("pickupFacade").objectReferenceValue = pickupFacade;
            checklist.Add("CollectRadiusController.pickupFacade");
        }

        var playerStats = player.GetComponent<PlayerStats>();
        if (playerStats != null && collectSo.FindProperty("statsProviderBehaviour").objectReferenceValue == null)
        {
            collectSo.FindProperty("statsProviderBehaviour").objectReferenceValue = playerStats;
            checklist.Add("CollectRadiusController.statsProviderBehaviour");
        }

        collectSo.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Unity -batchmode -quit -executeMethod DevMergeSceneSetup.VerifySampleSceneFromCli</summary>
    public static void VerifySampleSceneFromCli()
    {
        try
        {
            VerifySampleScene();
            Debug.Log("[DevMergeSceneSetup] Verification passed.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DevMergeSceneSetup] Verification failed: " + ex);
            EditorApplication.Exit(1);
        }
    }

    static void VerifySampleScene()
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
            throw new System.InvalidOperationException("SampleScene did not open.");

        Require(FindRootObject(scene, "Floor"), "Floor");
        Require(FindRootObject(scene, "Player"), "Player");
        Require(FindRootObject(scene, "HUD"), "HUD");
        Require(FindRootObject(scene, "CombatManager"), "CombatManager");
        Require(FindRootObject(scene, "EnvironmentObstacleGenerator"), "EnvironmentObstacleGenerator");

        var player = FindRootObject(scene, "Player");
        if (player.GetComponent<PlayerPickupFacade>() == null)
            throw new System.InvalidOperationException("Player missing PlayerPickupFacade.");
        if (player.transform.Find(CollectTriggerName) == null)
            throw new System.InvalidOperationException("Player missing CollectTrigger child.");

        var combatManager = FindRootObject(scene, "CombatManager");
        if (combatManager.GetComponent<DamageNumberDisplay>() == null)
            throw new System.InvalidOperationException("CombatManager missing DamageNumberDisplay.");

        var installer = combatManager.GetComponent<GameInstaller>();
        if (installer == null)
            throw new System.InvalidOperationException("CombatManager missing GameInstaller.");

        var installerSo = new SerializedObject(installer);
        if (installerSo.FindProperty("reactionVfxCatalog").objectReferenceValue == null)
            throw new System.InvalidOperationException("GameInstaller.reactionVfxCatalog not assigned.");

        var hud = FindRootObject(scene, "HUD");
        if (hud.transform.Find(DamageNumbersChildName) == null)
            throw new System.InvalidOperationException("HUD missing DamageNumbers container.");

        if (player.transform.Find("BlobShadow") == null && !ContainsChildNamed(player.transform, "BlobShadow"))
            Debug.LogWarning("[DevMergeSceneSetup] BlobShadow child not found on Player (dev prefab may use different name).");

        if (FindRootObject(scene, "showcase") == null)
            Debug.LogWarning("[DevMergeSceneSetup] showcase root not found.");

        if (player.GetComponent<PlayerCharacterAnimation>() == null)
            Debug.LogWarning("[DevMergeSceneSetup] PlayerCharacterAnimation missing — run Tools/Dev Merge/Add Player Animator Visual.");
        else if (player.transform.Find(VisualChildName) == null)
            Debug.LogWarning("[DevMergeSceneSetup] Player Visual child missing.");
    }

    static void Require(Object obj, string label)
    {
        if (obj == null)
            throw new System.InvalidOperationException("Missing scene object: " + label);
    }

    static bool ContainsChildNamed(Transform root, string name)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return true;
        }

        return false;
    }

    static GameObject FindRootObject(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }
}
#endif
