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

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spritres/White Pixel.png");
        if (sprite == null)
            Debug.LogWarning("[ReactionVfxPrefabBuilder] White Pixel sprite not found at Assets/Spritres/White Pixel.png");

        var vaporize = SaveOne($"{OutputDir}/VFX_Reaction_Vaporize.prefab", "VFX_Reaction_Vaporize", ReactionBurstParticleBootstrap.ReactionBurstKind.Vaporize, sprite);
        var crystallize = SaveOne($"{OutputDir}/VFX_Reaction_Crystallize.prefab", "VFX_Reaction_Crystallize", ReactionBurstParticleBootstrap.ReactionBurstKind.Crystallize, sprite);
        var scorchingWind = SaveOne($"{OutputDir}/VFX_Reaction_ScorchingWind.prefab", "VFX_Reaction_ScorchingWind", ReactionBurstParticleBootstrap.ReactionBurstKind.ScorchingWind, sprite);
        var explosion = SaveOne($"{OutputDir}/VFX_Reaction_Explosion.prefab", "VFX_Reaction_Explosion", ReactionBurstParticleBootstrap.ReactionBurstKind.Explosion, sprite);
        var growth = SaveOne($"{OutputDir}/VFX_Reaction_Growth.prefab", "VFX_Reaction_Growth", ReactionBurstParticleBootstrap.ReactionBurstKind.Growth, sprite);
        var hail = SaveOne($"{OutputDir}/VFX_Reaction_Hail.prefab", "VFX_Reaction_Hail", ReactionBurstParticleBootstrap.ReactionBurstKind.Hail, sprite);
        var electrowetting = SaveOne($"{OutputDir}/VFX_Reaction_Electrowetting.prefab", "VFX_Reaction_Electrowetting", ReactionBurstParticleBootstrap.ReactionBurstKind.Electrowetting, sprite);
        var dustSandStorm = SaveOne($"{OutputDir}/VFX_Reaction_DustSandStorm.prefab", "VFX_Reaction_DustSandStorm", ReactionBurstParticleBootstrap.ReactionBurstKind.DustSandStorm, sprite);
        var magnetism = SaveOne($"{OutputDir}/VFX_Reaction_Magnetism.prefab", "VFX_Reaction_Magnetism", ReactionBurstParticleBootstrap.ReactionBurstKind.Magnetism, sprite);
        var staticCharge = SaveOne($"{OutputDir}/VFX_Reaction_StaticCharge.prefab", "VFX_Reaction_StaticCharge", ReactionBurstParticleBootstrap.ReactionBurstKind.StaticCharge, sprite);

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

    private static GameObject SaveOne(string assetPath, string objectName, ReactionBurstParticleBootstrap.ReactionBurstKind kind, Sprite sprite)
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

            return PrefabUtility.SaveAsPrefabAsset(go, assetPath);
        }
        finally
        {
            Object.DestroyImmediate(go);
        }
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
