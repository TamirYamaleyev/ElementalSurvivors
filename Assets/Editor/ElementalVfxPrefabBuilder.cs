#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Regenerates elemental status VFX prefabs with valid Unity serialization (use after YAML edits or to recover corrupt prefabs).
/// </summary>
public static class ElementalVfxPrefabBuilder
{
    private const string OutputDir = "Assets/Prefabs/VFX/Elements";

    [MenuItem("Tools/Elemental Survivors/Regenerate Element Status VFX Prefabs")]
    public static void RegenerateAll()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/VFX");
        EnsureFolder(OutputDir);

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Spritres/White Pixel.png");
        if (sprite == null)
            Debug.LogWarning("[ElementalVfxPrefabBuilder] White Pixel sprite not found at Assets/Spritres/White Pixel.png");

        SaveOne($"{OutputDir}/VFX_Status_Fire.prefab", "VFX_Status_Fire", ElementalParticleBootstrap.PresetKind.Fire, sprite);
        SaveOne($"{OutputDir}/VFX_Status_Water.prefab", "VFX_Status_Water", ElementalParticleBootstrap.PresetKind.Water, sprite);
        SaveOne($"{OutputDir}/VFX_Status_Wind.prefab", "VFX_Status_Wind", ElementalParticleBootstrap.PresetKind.Wind, sprite);
        SaveOne($"{OutputDir}/VFX_Status_Earth.prefab", "VFX_Status_Earth", ElementalParticleBootstrap.PresetKind.Earth, sprite);
        SaveOne($"{OutputDir}/VFX_Status_Lightning.prefab", "VFX_Status_Lightning", ElementalParticleBootstrap.PresetKind.Lightning, sprite);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ElementalVfxPrefabBuilder] Regenerated 5 elemental VFX prefabs.");
    }

    /// <summary>Unity -batchmode -executeMethod ElementalVfxPrefabBuilder.RegenerateFromCli</summary>
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

    private static void SaveOne(string assetPath, string objectName, ElementalParticleBootstrap.PresetKind kind, Sprite sprite)
    {
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(assetPath);

        var go = new GameObject(objectName);
        try
        {
            var boot = go.AddComponent<ElementalParticleBootstrap>();
            var so = new SerializedObject(boot);
            so.FindProperty("kind").enumValueIndex = (int)kind;
            so.FindProperty("particleSprite").objectReferenceValue = sprite;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(go, assetPath);
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
