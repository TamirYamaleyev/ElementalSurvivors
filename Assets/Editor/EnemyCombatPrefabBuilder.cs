#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class EnemyCombatPrefabBuilder
{
    private const string DarkFlamePath = "Assets/Prefabs/Enemies/EnemyDarkFlameProjectile.prefab";
    private const string RangedEnemyPath = "Assets/Prefabs/Enemies/RangedEnemy.prefab";
    private const string BossEnemyPath = "Assets/Prefabs/Enemies/BossEnemy.prefab";
    private const string TierSetPath = "Assets/Scripts/Data/EnemyTierSet.asset";
    private const string BossVfxPath = "Assets/Prefabs/VFX/VFX_Boss_ElementalCone.prefab";
    private const string FireballPath = "Assets/Scripts/Weapons/SO/testWep/FireProjectilePrefab.prefab";
    private const string Level2Path = "Assets/Prefabs/Enemies/Level2Enemy.prefab";
    private const string Level3Path = "Assets/Prefabs/Enemies/Level3Enemy.prefab";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BossTestScenePath = "Assets/Scenes/BossCombatTest.unity";

    [MenuItem("Tools/Boss/Build Enemy Combat Prefabs And Test Scene")]
    public static void BuildAll()
    {
        EnsureFolder("Assets/Prefabs/Enemies");

        var darkFlame = BuildDarkFlameProjectile();
        var ranged = BuildRangedEnemy(darkFlame);
        var boss = BuildBossEnemy(darkFlame);
        WireTierSet(ranged, boss);
        BuildBossTestScene(boss);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[EnemyCombatPrefabBuilder] Built dark flame, ranged enemy, boss enemy, tier set, and BossCombatTest scene.");
    }

    /// <summary>Unity -batchmode -executeMethod EnemyCombatPrefabBuilder.BuildAllFromCli</summary>
    public static void BuildAllFromCli()
    {
        try
        {
            BuildAll();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex);
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }

    private static EnemyProjectile BuildDarkFlameProjectile()
    {
        var fireball = AssetDatabase.LoadAssetAtPath<GameObject>(FireballPath);
        if (fireball == null)
            throw new System.InvalidOperationException("Missing fireball prefab at " + FireballPath);

        var root = Object.Instantiate(fireball);
        root.name = "EnemyDarkFlameProjectile";

        Object.DestroyImmediate(root.GetComponent<Projectile>());

        var projectile = root.AddComponent<EnemyProjectile>();
        var sr = root.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(0.08f, 0.02f, 0.12f, 1f);

        var outlineGo = new GameObject("Outline");
        outlineGo.transform.SetParent(root.transform, false);
        outlineGo.transform.localScale = Vector3.one * 1.08f;
        var outlineSr = outlineGo.AddComponent<SpriteRenderer>();
        if (sr != null)
        {
            outlineSr.sprite = sr.sprite;
            outlineSr.sortingLayerID = sr.sortingLayerID;
            outlineSr.sortingOrder = sr.sortingOrder - 1;
        }
        outlineSr.color = new Color(0.9f, 0.15f, 0.1f, 1f);

        var saved = PrefabUtility.SaveAsPrefabAsset(root, DarkFlamePath);
        Object.DestroyImmediate(root);
        return saved.GetComponent<EnemyProjectile>();
    }

    private static Enemy BuildRangedEnemy(EnemyProjectile projectile)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Level2Path);
        if (source == null)
            throw new System.InvalidOperationException("Missing Level2Enemy prefab.");

        var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        instance.name = "RangedEnemy";

        var health = instance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            var so = new SerializedObject(health);
            so.FindProperty("contactDamage").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var visual = instance.transform.Find("Visual");
        Transform firePoint = null;
        if (visual != null)
        {
            var fp = visual.Find("FirePoint");
            if (fp == null)
            {
                var fpGo = new GameObject("FirePoint");
                fpGo.transform.SetParent(visual, false);
                fpGo.transform.localPosition = new Vector3(0.4f, 0f, 0f);
                firePoint = fpGo.transform;
            }
            else
            {
                firePoint = fp;
            }
        }

        var ranged = instance.GetComponent<EnemyRangedAttack>();
        if (ranged == null)
            ranged = instance.AddComponent<EnemyRangedAttack>();

        var rangedSo = new SerializedObject(ranged);
        rangedSo.FindProperty("projectilePrefab").objectReferenceValue = projectile;
        if (firePoint != null)
            rangedSo.FindProperty("firePoint").objectReferenceValue = firePoint;
        rangedSo.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(instance, RangedEnemyPath);
        Object.DestroyImmediate(instance);
        return saved.GetComponent<Enemy>();
    }

    private static Enemy BuildBossEnemy(EnemyProjectile projectile)
    {
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Level3Path);
        if (source == null)
            throw new System.InvalidOperationException("Missing Level3Enemy prefab.");

        var bossVfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossVfxPath);

        var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        instance.name = "BossEnemy";

        var health = instance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            var so = new SerializedObject(health);
            so.FindProperty("maxHealth").floatValue = 500f;
            so.FindProperty("contactDamage").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var visual = instance.transform.Find("Visual");
        Transform firePoint = null;
        GameObject telegraphInstance = null;

        if (visual != null)
        {
            var fp = visual.Find("FirePoint");
            if (fp == null)
            {
                var fpGo = new GameObject("FirePoint");
                fpGo.transform.SetParent(visual, false);
                fpGo.transform.localPosition = new Vector3(0f, 0.35f, 0f);
                firePoint = fpGo.transform;
            }
            else
            {
                firePoint = fp;
            }

            if (bossVfxPrefab != null)
            {
                telegraphInstance = PrefabUtility.InstantiatePrefab(bossVfxPrefab, visual) as GameObject;
                if (telegraphInstance != null)
                {
                    telegraphInstance.name = "TelegraphVfx";
                    telegraphInstance.transform.localPosition = new Vector3(0f, -0.15f, 0f);
                    telegraphInstance.transform.localRotation = Quaternion.identity;
                    telegraphInstance.SetActive(false);
                }
            }
        }

        if (instance.GetComponent<BossAttackController>() == null)
            instance.AddComponent<BossAttackController>();
        if (instance.GetComponent<BossAI>() == null)
            instance.AddComponent<BossAI>();

        var telegraph = instance.GetComponent<BossAttackTelegraphVfx>();
        if (telegraph == null)
            telegraph = instance.AddComponent<BossAttackTelegraphVfx>();

        if (telegraphInstance != null)
        {
            var teleSo = new SerializedObject(telegraph);
            teleSo.FindProperty("vfxRoot").objectReferenceValue = telegraphInstance;
            teleSo.ApplyModifiedPropertiesWithoutUndo();
        }

        var attack = instance.GetComponent<BossAttackController>();
        var attackSo = new SerializedObject(attack);
        attackSo.FindProperty("projectilePrefab").objectReferenceValue = projectile;
        attackSo.FindProperty("telegraphVfx").objectReferenceValue = telegraph;
        if (firePoint != null)
            attackSo.FindProperty("firePoint").objectReferenceValue = firePoint;
        attackSo.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(instance, BossEnemyPath);
        Object.DestroyImmediate(instance);
        return saved.GetComponent<Enemy>();
    }

    private static void WireTierSet(Enemy ranged, Enemy boss)
    {
        var tierSet = AssetDatabase.LoadAssetAtPath<EnemyTierSetSO>(TierSetPath);
        if (tierSet == null)
            throw new System.InvalidOperationException("Missing EnemyTierSet.asset");

        var so = new SerializedObject(tierSet);
        var tiers = so.FindProperty("tiers");
        if (tiers.arraySize > 1)
            tiers.GetArrayElementAtIndex(1).FindPropertyRelative("prototype").objectReferenceValue = ranged;

        so.FindProperty("bossPrototype").objectReferenceValue = boss;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildBossTestScene(Enemy bossPrefab)
    {
        if (!File.Exists(BossTestScenePath))
            AssetDatabase.CopyAsset(SampleScenePath, BossTestScenePath);

        var scene = EditorSceneManager.OpenScene(BossTestScenePath, OpenSceneMode.Single);
        if (!scene.IsValid())
            throw new System.InvalidOperationException("Could not open BossCombatTest scene.");

        var spawner = Object.FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.gameObject.SetActive(false);

        var bootstrapGo = GameObject.Find("BossCombatTestBootstrap");
        if (bootstrapGo == null)
            bootstrapGo = new GameObject("BossCombatTestBootstrap");

        var bootstrap = bootstrapGo.GetComponent<BossCombatTestBootstrap>();
        if (bootstrap == null)
            bootstrap = bootstrapGo.AddComponent<BossCombatTestBootstrap>();

        var bootstrapSo = new SerializedObject(bootstrap);
        bootstrapSo.FindProperty("bossPrefab").objectReferenceValue = bossPrefab;
        bootstrapSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AddSceneToBuildSettings(BossTestScenePath);
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        foreach (var entry in scenes)
        {
            if (entry.path == scenePath)
                return;
        }

        var updated = new EditorBuildSettingsScene[scenes.Length + 1];
        for (var i = 0; i < scenes.Length; i++)
            updated[i] = scenes[i];
        updated[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updated;
    }

    private static void EnsureFolder(string path)
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
