#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Builds reaction burst VFX prefabs and wires <see cref="ReactionVfxCatalogSO"/>.
/// </summary>
public static class ReactionVfxPrefabBuilder
{
    private const string OutputDir = "Assets/Prefabs/VFX/Reactions";
    private const string CatalogPath = "Assets/Data/ReactionVfxCatalog.asset";

    [MenuItem("Tools/Elemental Survivors/Regenerate Reaction Burst VFX Prefabs")]
    public static void RegenerateAll()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/VFX");
        EnsureFolder(OutputDir);
        EnsureFolder("Assets/Data");

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/White Pixel.png")
            ?? AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spritres/White Pixel.png");
        if (sprite == null)
            Debug.LogWarning("[ReactionVfxPrefabBuilder] White Pixel sprite not found under Assets/Sprites or Assets/Spritres.");

        var vaporize = SaveVaporize($"{OutputDir}/VFX_Reaction_Vaporize.prefab", "VFX_Reaction_Vaporize", sprite);
        var crystallize = SaveOne($"{OutputDir}/VFX_Reaction_Crystallize.prefab", "VFX_Reaction_Crystallize", ReactionBurstParticleBootstrap.ReactionBurstKind.Crystallize, sprite);
        var scorchingWind = SaveScorchingWind($"{OutputDir}/VFX_Reaction_ScorchingWind.prefab", "VFX_Reaction_ScorchingWind", sprite);
        var explosion = SaveOne($"{OutputDir}/VFX_Reaction_Explosion.prefab", "VFX_Reaction_Explosion", ReactionBurstParticleBootstrap.ReactionBurstKind.Explosion, sprite);
        var growth = SaveOne($"{OutputDir}/VFX_Reaction_Growth.prefab", "VFX_Reaction_Growth", ReactionBurstParticleBootstrap.ReactionBurstKind.Growth, sprite);
        var hail = SaveHail($"{OutputDir}/VFX_Reaction_Hail.prefab", "VFX_Reaction_Hail", sprite);
        var electrowetting = SaveElectrowetting($"{OutputDir}/VFX_Reaction_Electrowetting.prefab", "VFX_Reaction_Electrowetting", sprite);
        var dustSandStorm = SaveDustSandStorm($"{OutputDir}/VFX_Reaction_DustSandStorm.prefab", "VFX_Reaction_DustSandStorm", sprite);
        var magnetism = SaveMagnetism($"{OutputDir}/VFX_Reaction_Magnetism.prefab", "VFX_Reaction_Magnetism", sprite);
        var staticCharge = SaveStaticCharge($"{OutputDir}/VFX_Reaction_StaticCharge.prefab", "VFX_Reaction_StaticCharge", sprite);

        var catalog = AssetDatabase.LoadAssetAtPath<ReactionVfxCatalogSO>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<ReactionVfxCatalogSO>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        var catSo = new SerializedObject(catalog);
        catSo.FindProperty("vaporize").objectReferenceValue = vaporize;
        catSo.FindProperty("crystallize").objectReferenceValue = crystallize;
        catSo.FindProperty("scorchingWind").objectReferenceValue = scorchingWind;
        catSo.FindProperty("explosion").objectReferenceValue = explosion;
        catSo.FindProperty("growth").objectReferenceValue = growth;
        catSo.FindProperty("hail").objectReferenceValue = hail;
        catSo.FindProperty("electrowetting").objectReferenceValue = electrowetting;
        catSo.FindProperty("dustSandStorm").objectReferenceValue = dustSandStorm;
        catSo.FindProperty("magnetism").objectReferenceValue = magnetism;
        catSo.FindProperty("staticCharge").objectReferenceValue = staticCharge;
        catSo.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ReactionVfxPrefabBuilder] Regenerated 10 reaction prefabs + catalog.");
    }

    /// <summary>Unity -batchmode -executeMethod ReactionVfxPrefabBuilder.RegenerateFromCli</summary>
    public static void RegenerateFromCli()
    {
        try
        {
            RegenerateAll();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            EditorApplication.Exit(1);
            return;
        }

        EditorApplication.Exit(0);
    }

    private static GameObject SaveVaporize(string assetPath, string objectName, Sprite sprite)
    {
        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Vaporize,
            sprite,
            go => go.AddComponent<ReactionVaporizeVisual>());
    }

    private static GameObject SaveScorchingWind(string assetPath, string objectName, Sprite sprite)
    {
        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.ScorchingWind,
            sprite,
            go => go.AddComponent<ReactionScorchingRaysVisual>());
    }

    private static GameObject SaveHail(string assetPath, string objectName, Sprite sprite)
    {
        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Hail,
            sprite,
            go => go.AddComponent<ReactionHailVisual>());
    }

    private static GameObject SaveDustSandStorm(string assetPath, string objectName, Sprite sprite)
    {
        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.DustSandStorm,
            sprite,
            go => go.AddComponent<ReactionDustSandStormVisual>());
    }

    private static GameObject SaveStaticCharge(string assetPath, string objectName, Sprite sprite)
    {
        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.StaticCharge,
            sprite,
            go => go.AddComponent<ReactionStaticChargeParalysisVisual>());
    }

    private static GameObject SaveMagnetism(string assetPath, string objectName, Sprite sprite)
    {
        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Magnetism,
            sprite,
            go => go.AddComponent<ReactionMagneticFieldShrink>());
    }

    private static GameObject SaveElectrowetting(string assetPath, string objectName, Sprite sprite)
    {
        var lightningPrefab = AssetDatabase.LoadAssetAtPath<ChaingLightningVisual>(
            "Assets/Scripts/Weapons/ChainLightning/LightningVisualPrefab.prefab");

        return SaveWithExtraComponents(
            assetPath,
            objectName,
            ReactionBurstParticleBootstrap.ReactionBurstKind.Electrowetting,
            sprite,
            go =>
            {
                go.AddComponent<ReactionMagneticFieldShrink>();
                var bolts = go.AddComponent<ReactionElectrowettingBolts>();
                if (lightningPrefab != null)
                {
                    var boltsSo = new SerializedObject(bolts);
                    boltsSo.FindProperty("lightningVisualPrefab").objectReferenceValue = lightningPrefab;
                    boltsSo.ApplyModifiedPropertiesWithoutUndo();
                }
            });
    }

    private static GameObject SaveWithExtraComponents(
        string assetPath,
        string objectName,
        ReactionBurstParticleBootstrap.ReactionBurstKind kind,
        Sprite sprite,
        System.Action<GameObject> addExtra)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(assetPath);

        var go = new GameObject(objectName);
        try
        {
            var boot = go.AddComponent<ReactionBurstParticleBootstrap>();
            var so = new SerializedObject(boot);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("particleSprite").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            addExtra?.Invoke(go);

            return PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
    }

    private static GameObject SaveOne(string assetPath, string objectName, ReactionBurstParticleBootstrap.ReactionBurstKind kind, Sprite sprite)
    {
        return SaveWithExtraComponents(assetPath, objectName, kind, sprite, null);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        var parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        var leaf = Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(leaf))
            throw new System.InvalidOperationException("Bad folder path: " + path);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, leaf);
    }
}
#endif
