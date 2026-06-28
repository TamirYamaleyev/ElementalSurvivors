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
    private const string BossSpritePath = "Assets/Sprites/final binal/Untitled_Artwork.png";
    private const string LightSwordSpritePath = "Assets/Sprites/LightSword.png";
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

        var rb = root.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = root.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

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
            ConfigureBossVisual(visual);

            var fp = visual.Find("FirePoint");
            if (fp == null)
            {
                var fpGo = new GameObject("FirePoint");
                fpGo.transform.SetParent(visual, false);
                fpGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                firePoint = fpGo.transform;
            }
            else
            {
                fp.localPosition = new Vector3(0f, 0.55f, 0f);
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

        var singleLine = attackSo.FindProperty("singleLine");
        singleLine.FindPropertyRelative("count").intValue = 11;
        singleLine.FindPropertyRelative("delayBetweenShots").floatValue = 0.05f;

        var rotatingArc = attackSo.FindProperty("rotatingArc");
        rotatingArc.FindPropertyRelative("segmentCount").intValue = 5;
        rotatingArc.FindPropertyRelative("segmentArcDegrees").floatValue = 72f;
        rotatingArc.FindPropertyRelative("projectilesPerRow").intValue = 9;
        rotatingArc.FindPropertyRelative("radialRows").intValue = 6;
        rotatingArc.FindPropertyRelative("delayBetweenSegments").floatValue = 0.5f;
        rotatingArc.FindPropertyRelative("startFromAim").boolValue = true;
        attackSo.ApplyModifiedPropertiesWithoutUndo();

        EnsureBossHealthBar(instance, health);

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

    private static void ConfigureBossVisual(Transform visual)
    {
        var bossSprite = LoadSprite(BossSpritePath, "Untitled_Artwork_0");
        var swordSprite = LoadSprite(LightSwordSpritePath);

        var spriteRenderer = visual.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && bossSprite != null)
            spriteRenderer.sprite = bossSprite;

        var animator = visual.GetComponent<Animator>();
        if (animator != null)
            animator.enabled = false;

        EnsureShadowSword(visual, swordSprite);
    }

    private static Sprite LoadSprite(string assetPath, string spriteName = null)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        foreach (var asset in assets)
        {
            if (asset is not Sprite sprite)
                continue;

            if (string.IsNullOrEmpty(spriteName) || sprite.name == spriteName)
                return sprite;
        }

        return null;
    }

    private static void EnsureShadowSword(Transform visual, Sprite swordSprite)
    {
        var shadowRoot = visual.Find("ShadowSword");
        if (shadowRoot == null)
        {
            var shadowGo = new GameObject("ShadowSword");
            shadowGo.transform.SetParent(visual, false);
            shadowRoot = shadowGo.transform;
        }

        shadowRoot.localPosition = new Vector3(-0.65f, 0f, 0f);
        shadowRoot.localRotation = Quaternion.Euler(0f, 180f, 0f);
        shadowRoot.localScale = Vector3.one * (2f / 3f);

        var glowRenderer = shadowRoot.GetComponent<SpriteRenderer>() ?? shadowRoot.gameObject.AddComponent<SpriteRenderer>();
        if (swordSprite != null)
            glowRenderer.sprite = swordSprite;
        glowRenderer.color = new Color(0.195f, 0f, 0.632f, 0.87f);
        glowRenderer.sortingOrder = 1;
        glowRenderer.spriteSortPoint = SpriteSortPoint.Pivot;

        var swordTransform = shadowRoot.Find("Sword");
        if (swordTransform == null)
        {
            var swordGo = new GameObject("Sword");
            swordGo.transform.SetParent(shadowRoot, false);
            swordTransform = swordGo.transform;
        }

        swordTransform.localPosition = Vector3.zero;
        swordTransform.localRotation = Quaternion.identity;
        swordTransform.localScale = Vector3.one;

        var swordRenderer = swordTransform.GetComponent<SpriteRenderer>() ?? swordTransform.gameObject.AddComponent<SpriteRenderer>();
        if (swordSprite != null)
            swordRenderer.sprite = swordSprite;
        swordRenderer.color = new Color(0.14f, 0.029f, 0.415f, 0.835f);
        swordRenderer.sortingOrder = -1;
        swordRenderer.spriteSortPoint = SpriteSortPoint.Pivot;
    }

    private static void EnsureBossHealthBar(GameObject bossRoot, EnemyHealth health)
    {
        var existing = bossRoot.GetComponentInChildren<EnemyWorldHealthBar>(true);
        if (existing != null)
            return;

        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        var barRoot = new GameObject("HealthBar");
        barRoot.transform.SetParent(bossRoot.transform, false);
        barRoot.transform.localPosition = new Vector3(0f, 2f, 0f);

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(barRoot.transform, false);
        bgGo.transform.localScale = new Vector3(1.4f, 0.12f, 1f);
        var bgSr = bgGo.AddComponent<SpriteRenderer>();
        bgSr.sprite = sprite;
        bgSr.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        bgSr.sortingOrder = 50;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(barRoot.transform, false);
        fillGo.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        fillGo.transform.localScale = new Vector3(1.4f, 0.1f, 1f);
        var fillSr = fillGo.AddComponent<SpriteRenderer>();
        fillSr.sprite = sprite;
        fillSr.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        fillSr.sortingOrder = 51;

        var bar = barRoot.AddComponent<EnemyWorldHealthBar>();
        var barSo = new SerializedObject(bar);
        barSo.FindProperty("health").objectReferenceValue = health;
        barSo.FindProperty("fillTransform").objectReferenceValue = fillGo.transform;
        barSo.FindProperty("barWidth").floatValue = 1.4f;
        barSo.FindProperty("localOffset").vector3Value = Vector3.zero;
        barSo.ApplyModifiedPropertiesWithoutUndo();
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
