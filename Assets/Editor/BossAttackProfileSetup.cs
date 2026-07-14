#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class BossAttackProfileSetup
{
    const string FolderPath = "Assets/Data/Boss";
    const string ProfilePath = FolderPath + "/BossAttackProfile_Default.asset";
    const string BossEnemyPath = "Assets/Prefabs/Enemies/BossEnemy.prefab";

    [MenuItem("Tools/Elemental Survivors/Create Default Boss Attack Profile")]
    public static void CreateDefaultProfileMenu()
    {
        CreateAndWire();
        Debug.Log("[BossAttackProfileSetup] Created/wired " + ProfilePath);
    }

    public static void CreateAndWireFromCli()
    {
        try
        {
            CreateAndWire();
            Debug.Log("[BossAttackProfileSetup] CLI completed.");
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[BossAttackProfileSetup] CLI failed: " + ex);
            EditorApplication.Exit(1);
        }
    }

    static void CreateAndWire()
    {
        EnsureFolder(FolderPath);

        var profile = AssetDatabase.LoadAssetAtPath<BossAttackProfileSO>(ProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<BossAttackProfileSO>();
            AssetDatabase.CreateAsset(profile, ProfilePath);
        }

        // Match current BossEnemy prefab tuning.
        profile.windUpDuration = 1f;
        profile.delayBetweenVolleys = 2f;
        profile.initialDelay = 1.5f;
        profile.projectileSpeed = 5f;
        profile.projectileDamage = 12f;
        profile.projectileLifetime = 5f;
        profile.patternCycle = new[]
        {
            BossAttackPatternKind.TriangleCone,
            BossAttackPatternKind.SingleLine,
            BossAttackPatternKind.RotatingArc
        };
        profile.triangleCone = new BossTriangleConeConfig
        {
            rows = 7,
            coneHalfAngle = 35f,
            rowSpacing = 0.55f,
            delayBetweenRows = 0.2f
        };
        profile.singleLine = new BossSingleLineConfig
        {
            count = 11,
            delayBetweenShots = 0.05f
        };
        profile.rotatingArc = new BossRotatingArcConfig
        {
            segmentCount = 5,
            segmentArcDegrees = 72f,
            projectilesPerRow = 9,
            radialRows = 6,
            rowSpacing = 0.5f,
            delayBetweenSegments = 0.5f,
            rotationStepDegrees = 45f,
            startFromAim = true
        };

        EditorUtility.SetDirty(profile);

        var bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossEnemyPath);
        if (bossPrefab == null)
            throw new System.InvalidOperationException("Missing " + BossEnemyPath);

        var attack = bossPrefab.GetComponent<BossAttackController>();
        if (attack == null)
            throw new System.InvalidOperationException("BossEnemy missing BossAttackController");

        var so = new SerializedObject(attack);
        so.FindProperty("attackProfile").objectReferenceValue = profile;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(bossPrefab);

        AssetDatabase.SaveAssets();
    }

    static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        var parts = folderPath.Split('/');
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
