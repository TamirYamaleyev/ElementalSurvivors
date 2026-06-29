#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class EnemyCombatPrefabBuilder
{
    private const string DarkFlamePath = "Assets/Prefabs/Enemies/EnemyDarkFlameProjectile.prefab";
    private const string RangedEnemyPath = "Assets/Prefabs/Enemies/RangedEnemy.prefab";
    private const string TierSetPath = "Assets/Scripts/Data/EnemyTierSet.asset";
    private const string FireballPath = "Assets/Scripts/Weapons/SO/testWep/FireProjectilePrefab.prefab";
    private const string Level1Path = "Assets/Prefabs/Enemies/Level1Enemy.prefab";
    private const string RangedEnemySpritePath = "Assets/Art/Characters/Enemy/Enemy2_Walk.png";
    private const string RangedEnemySpriteName = "Enemy2_0";
    private const string RangedControllerPath = "Assets/Animation/Controllers/AC_Enemy_Ranged.controller";

    [MenuItem("Tools/Enemy/Build Ranged Enemy Prefabs")]
    public static void BuildAll()
    {
        EnsureFolder("Assets/Prefabs/Enemies");

        var darkFlame = BuildDarkFlameProjectile();
        var ranged = BuildRangedEnemy(darkFlame);
        CharacterAnimationPipeline.BuildRangedEnemyAnimation();
        WireTierSet(ranged);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[EnemyCombatPrefabBuilder] Built dark flame projectile, ranged enemy, and tier set wiring.");
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
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Level1Path);
        if (source == null)
            throw new System.InvalidOperationException("Missing Level1Enemy prefab.");

        var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
        PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        instance.name = "RangedEnemy";

        var health = instance.GetComponent<EnemyHealth>();
        if (health != null)
        {
            var so = new SerializedObject(health);
            so.FindProperty("maxHealth").floatValue = 12f;
            so.FindProperty("contactDamage").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        var visual = instance.transform.Find("Visual");
        Transform firePoint = null;
        if (visual != null)
        {
            var archerSprite = LoadSpriteFromSheet(RangedEnemySpritePath, RangedEnemySpriteName);
            var visualSr = visual.GetComponent<SpriteRenderer>();
            if (archerSprite != null && visualSr != null)
                visualSr.sprite = archerSprite;

            var animator = visual.GetComponent<Animator>();
            var rangedController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(RangedControllerPath);
            if (animator != null)
            {
                animator.runtimeAnimatorController = rangedController;
                animator.enabled = rangedController != null;
            }

            var fpGo = new GameObject("FirePoint");
            fpGo.transform.SetParent(visual, false);
            fpGo.transform.localPosition = new Vector3(0.4f, 0f, 0f);
            firePoint = fpGo.transform;
        }

        var ranged = instance.GetComponent<EnemyRangedAttack>();
        if (ranged == null)
            ranged = instance.AddComponent<EnemyRangedAttack>();

        var rangedSo = new SerializedObject(ranged);
        rangedSo.FindProperty("projectilePrefab").objectReferenceValue = projectile;
        rangedSo.FindProperty("windUpDuration").floatValue = 0.75f;
        if (firePoint != null)
            rangedSo.FindProperty("firePoint").objectReferenceValue = firePoint;
        rangedSo.ApplyModifiedPropertiesWithoutUndo();

        var saved = PrefabUtility.SaveAsPrefabAsset(instance, RangedEnemyPath);
        Object.DestroyImmediate(instance);
        return saved.GetComponent<Enemy>();
    }

    private static void WireTierSet(Enemy ranged)
    {
        var tierSet = AssetDatabase.LoadAssetAtPath<EnemyTierSetSO>(TierSetPath);
        if (tierSet == null)
            throw new System.InvalidOperationException("Missing EnemyTierSet.asset");

        var so = new SerializedObject(tierSet);
        var tiers = so.FindProperty("tiers");
        if (tiers.arraySize > 1)
            tiers.GetArrayElementAtIndex(1).FindPropertyRelative("prototype").objectReferenceValue = ranged;

        so.FindProperty("rangedTierIndex").intValue = 1;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite LoadSpriteFromSheet(string sheetPath, string spriteName)
    {
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(sheetPath))
        {
            if (asset is Sprite sprite && sprite.name == spriteName)
                return sprite;
        }

        return null;
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
