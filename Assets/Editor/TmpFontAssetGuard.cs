#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Self-heal TextMeshPro when Font Asset is missing (fixes editor errors like
/// "Can't Generate Mesh, No Font Asset has been assigned" during HandleUtility.BeginHandles).
/// </summary>
[InitializeOnLoad]
internal static class TmpFontAssetGuard
{
    private const string DefaultFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

    static TmpFontAssetGuard()
    {
        EditorApplication.delayCall += TryPatchOnceAfterLoad;
    }

    private static void TryPatchOnceAfterLoad()
    {
        EnsureTmpSettingsDefaultFont();
        var fallback = TMP_Settings.defaultFontAsset;
        if (fallback == null)
            fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetPath);
        if (fallback == null)
        {
            Debug.LogWarning(
                "[TMP] No TMP font asset available. Run Window > TextMeshPro > Import TMP Essential Resources, " +
                $"or ensure the font exists at '{DefaultFontAssetPath}'.");
            return;
        }

        var patched = 0;
        foreach (var tmp in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (tmp.font != null)
                continue;
            Undo.RecordObject(tmp, "Assign TMP default font");
            tmp.font = fallback;
            EditorUtility.SetDirty(tmp);
            patched++;
        }

        if (patched > 0)
            Debug.LogWarning($"[TMP] Assigned default font to {patched} TextMeshPro component(s) with missing Font Asset. Save scenes/prefabs to persist.");
    }

    private static void EnsureTmpSettingsDefaultFont()
    {
        var settings = Resources.Load<TMP_Settings>("TMP Settings");
        if (settings == null)
            return;

        if (TMP_Settings.defaultFontAsset != null)
            return;

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(DefaultFontAssetPath);
        if (font == null)
            return;

        var so = new SerializedObject(settings);
        var prop = so.FindProperty("m_defaultFontAsset");
        if (prop == null)
            return;
        prop.objectReferenceValue = font;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.LogWarning("[TMP] Restored missing TMP Settings default font asset reference.");
    }
}
#endif
