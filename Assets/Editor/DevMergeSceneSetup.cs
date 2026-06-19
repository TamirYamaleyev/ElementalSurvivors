#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

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
