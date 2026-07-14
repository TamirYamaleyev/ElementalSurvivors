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
    const string ReactionGameplayCatalogPath = "Assets/Data/ReactionGameplayCatalog.asset";
    const string ElementalStatusGameplayCatalogPath = "Assets/Data/ElementalStatusGameplayCatalog.asset";
    const string BranchPrefabPath = "Assets/Prefabs/Environment/PF_Environment_Branch.prefab";
    const string BushPrefabPath = "Assets/Prefabs/Environment/PF_Environment_Bush.prefab";
    const string DamageNumbersWorldName = "DamageNumbersWorld";
    const string DamageNumbersChildName = "DamageNumbers";
    const float DamageNumbersWorldCanvasScale = 0.01f;
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
            ValidateLevelUpUi(FindScene(), new List<string>());
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
            ValidateLevelUpUi(scene, checklist);
            Debug.Log("[DevMergeSceneSetup] Level-up UI validation: " + string.Join(", ", checklist));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[DevMergeSceneSetup] FixLevelUpUi: " + ex);
            EditorApplication.Exit(1);
        }
    }

    [MenuItem("Tools/Dev Merge/Validate Level-Up UI")]
    public static void FixLevelUpUiMenu()
    {
        var scene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        var checklist = new List<string>();
        ValidateLevelUpUi(scene, checklist);
        Debug.Log("[DevMergeSceneSetup] Level-up UI validation: " + string.Join(", ", checklist));
    }

    static void ValidateLevelUpUi(Scene scene, List<string> checklist)
    {
        var levelUpUi = Object.FindFirstObjectByType<LevelUpUI>();
        if (levelUpUi == null)
        {
            Debug.LogWarning("[DevMergeSceneSetup] LevelUpUI not found.");
            return;
        }

        var uiSo = new SerializedObject(levelUpUi);
        var weaponSystem = uiSo.FindProperty("weaponSystem").objectReferenceValue;
        var options = uiSo.FindProperty("levelUpOptions");
        var buttons = uiSo.FindProperty("optionButtons");

        if (weaponSystem == null)
            Debug.LogWarning("[DevMergeSceneSetup] LevelUpUI.weaponSystem is not assigned.");
        else
            checklist.Add("weaponSystem ok");

        if (options == null || options.arraySize < 3)
            Debug.LogWarning("[DevMergeSceneSetup] LevelUpUI.levelUpOptions needs 3 WeaponDefinitions.");
        else
        {
            for (int i = 0; i < options.arraySize; i++)
            {
                if (options.GetArrayElementAtIndex(i).objectReferenceValue == null)
                    Debug.LogWarning($"[DevMergeSceneSetup] LevelUpUI.levelUpOptions[{i}] is null.");
            }
            checklist.Add($"levelUpOptions[{options.arraySize}]");
        }

        if (buttons == null || buttons.arraySize < 3)
            Debug.LogWarning("[DevMergeSceneSetup] LevelUpUI.optionButtons needs 3 Buttons.");
        else
        {
            for (int i = 0; i < buttons.arraySize; i++)
            {
                var button = buttons.GetArrayElementAtIndex(i).objectReferenceValue as Button;
                if (button == null)
                {
                    Debug.LogWarning($"[DevMergeSceneSetup] LevelUpUI.optionButtons[{i}] is null.");
                    continue;
                }

                if (button.onClick.GetPersistentEventCount() == 0)
                    Debug.LogWarning($"[DevMergeSceneSetup] {button.name} has no persistent OnClick (expected Choose).");
            }
            checklist.Add($"optionButtons[{buttons.arraySize}]");
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
        ValidateLevelUpUi(scene, checklist);

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

        var container = EnsureWorldDamageNumbersContainer(scene, checklist);

        var prefab = AssetDatabase.LoadAssetAtPath<DamageNumberView>(DamageNumberPrefabPath);
        if (prefab == null)
            Debug.LogWarning("[DevMergeSceneSetup] Missing prefab: " + DamageNumberPrefabPath);

        var display = combatManager.GetComponent<DamageNumberDisplay>();
        if (display == null)
        {
            display = combatManager.AddComponent<DamageNumberDisplay>();
            checklist.Add("DamageNumberDisplay on CombatManager");
        }

        var mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        var so = new SerializedObject(display);
        if (prefab != null)
        {
            so.FindProperty("prefab").objectReferenceValue = prefab;
            checklist.Add("DamageNumberDisplay.prefab");
        }

        if (container != null)
        {
            so.FindProperty("container").objectReferenceValue = container;
            checklist.Add("DamageNumberDisplay.container (world space)");
        }

        if (mainCamera != null)
        {
            so.FindProperty("worldCamera").objectReferenceValue = mainCamera;
            checklist.Add("DamageNumberDisplay.worldCamera");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static RectTransform EnsureWorldDamageNumbersContainer(Scene scene, List<string> checklist)
    {
        var worldRoot = FindRootObject(scene, DamageNumbersWorldName);
        GameObject go;
        if (worldRoot == null)
        {
            go = new GameObject(DamageNumbersWorldName);
            SceneManager.MoveGameObjectToScene(go, scene);
            checklist.Add(DamageNumbersWorldName + " root");
        }
        else
        {
            go = worldRoot;
        }

        var rect = go.GetComponent<RectTransform>();
        if (rect == null)
            rect = go.AddComponent<RectTransform>();

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(100f, 100f);
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        var canvas = go.GetComponent<Canvas>();
        if (canvas == null)
            canvas = go.AddComponent<Canvas>();

        var mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = Object.FindFirstObjectByType<Camera>();

        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            checklist.Add(DamageNumbersWorldName + " canvas world space");
        }

        if (mainCamera != null && canvas.worldCamera != mainCamera)
        {
            canvas.worldCamera = mainCamera;
            checklist.Add(DamageNumbersWorldName + " canvas camera");
        }

        go.transform.localScale = Vector3.one * DamageNumbersWorldCanvasScale;

        if (go.GetComponent<UnityEngine.UI.CanvasScaler>() == null)
            go.AddComponent<UnityEngine.UI.CanvasScaler>();

        if (go.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        if (canvas.sortingOrder < 100)
        {
            canvas.sortingOrder = 100;
            checklist.Add(DamageNumbersWorldName + " canvas sorting order");
        }

        return rect;
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

        var vfxCatalog = AssetDatabase.LoadAssetAtPath<ReactionVfxCatalogSO>(ReactionVfxCatalogPath);
        var gameplayCatalog = AssetDatabase.LoadAssetAtPath<ReactionGameplayCatalogSO>(ReactionGameplayCatalogPath);
        var elementalCatalog = AssetDatabase.LoadAssetAtPath<ElementalStatusGameplayCatalogSO>(ElementalStatusGameplayCatalogPath);

        if (vfxCatalog == null)
            Debug.LogWarning("[DevMergeSceneSetup] Missing asset: " + ReactionVfxCatalogPath);
        if (gameplayCatalog == null)
            Debug.LogWarning("[DevMergeSceneSetup] Missing asset: " + ReactionGameplayCatalogPath);
        if (elementalCatalog == null)
            Debug.LogWarning("[DevMergeSceneSetup] Missing asset: " + ElementalStatusGameplayCatalogPath);

        var so = new SerializedObject(installer);
        var changed = false;

        if (vfxCatalog != null)
            changed |= AssignIfDifferent(so, "reactionVfxCatalog", vfxCatalog, checklist, "GameInstaller.reactionVfxCatalog");

        if (gameplayCatalog != null)
            changed |= AssignIfDifferent(so, "reactionGameplayCatalog", gameplayCatalog, checklist, "GameInstaller.reactionGameplayCatalog");

        if (elementalCatalog != null)
            changed |= AssignIfDifferent(so, "elementalStatusGameplayCatalog", elementalCatalog, checklist, "GameInstaller.elementalStatusGameplayCatalog");

        if (changed)
            so.ApplyModifiedPropertiesWithoutUndo();
    }

    static bool AssignIfDifferent(SerializedObject so, string propertyName, Object value, List<string> checklist, string label)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null || prop.objectReferenceValue == value)
            return false;

        prop.objectReferenceValue = value;
        checklist.Add(label);
        return true;
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

        SetVector2IfDifferent(so, "activeHalfExtents", new Vector2(24f, 14f), checklist, "EnvironmentObstacleGenerator.activeHalfExtents");
        SetVector2IfDifferent(so, "despawnHalfExtents", new Vector2(32f, 20f), checklist, "EnvironmentObstacleGenerator.despawnHalfExtents");
        SetFloatIfDifferent(so, "cellSize", 16f, checklist, "EnvironmentObstacleGenerator.cellSize=16");
        SetIntIfDifferent(so, "obstaclesPerCell", 4, checklist, "EnvironmentObstacleGenerator.obstaclesPerCell=4");
        SetIntIfDifferent(so, "poolPrewarmPerPrefab", 8, checklist, "EnvironmentObstacleGenerator.poolPrewarmPerPrefab=8");

        if (so.FindProperty("edgePadding").floatValue != 4f)
        {
            so.FindProperty("edgePadding").floatValue = 4f;
            checklist.Add("EnvironmentObstacleGenerator.edgePadding=4");
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void SetBoolIfDifferent(SerializedObject so, string propertyName, bool value, List<string> checklist, string label)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null || prop.boolValue == value)
            return;
        prop.boolValue = value;
        checklist.Add(label);
    }

    static void SetIntIfDifferent(SerializedObject so, string propertyName, int value, List<string> checklist, string label)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null || prop.intValue == value)
            return;
        prop.intValue = value;
        checklist.Add(label);
    }

    static void SetFloatIfDifferent(SerializedObject so, string propertyName, float value, List<string> checklist, string label)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null || Mathf.Approximately(prop.floatValue, value))
            return;
        prop.floatValue = value;
        checklist.Add(label);
    }

    static void SetVector2IfDifferent(SerializedObject so, string propertyName, Vector2 value, List<string> checklist, string label)
    {
        var prop = so.FindProperty(propertyName);
        if (prop == null || prop.vector2Value == value)
            return;
        prop.vector2Value = value;
        checklist.Add(label);
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
        if (installerSo.FindProperty("reactionGameplayCatalog").objectReferenceValue == null)
            throw new System.InvalidOperationException("GameInstaller.reactionGameplayCatalog not assigned.");
        if (installerSo.FindProperty("elementalStatusGameplayCatalog").objectReferenceValue == null)
            throw new System.InvalidOperationException("GameInstaller.elementalStatusGameplayCatalog not assigned.");

        var worldDamageNumbers = FindRootObject(scene, DamageNumbersWorldName);
        if (worldDamageNumbers == null)
            throw new System.InvalidOperationException("Missing " + DamageNumbersWorldName + " world canvas.");

        var worldCanvas = worldDamageNumbers.GetComponent<Canvas>();
        if (worldCanvas == null || worldCanvas.renderMode != RenderMode.WorldSpace)
            throw new System.InvalidOperationException(DamageNumbersWorldName + " must use a World Space Canvas.");

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
