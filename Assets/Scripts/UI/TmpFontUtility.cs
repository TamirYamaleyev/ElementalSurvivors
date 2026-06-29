using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Resolves the project TMP default font and assigns it when references are missing.
/// </summary>
public static class TmpFontUtility
{
    private const string LiberationSansResourcePath = "Fonts & Materials/LiberationSans SDF";
    private const string LiberationSansFallbackResourcePath = "Fonts & Materials/LiberationSans SDF - Fallback";
#if UNITY_EDITOR
    private const string LiberationSansAssetPath =
        "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
#endif

    private static TMP_FontAsset cachedFont;

    public static TMP_FontAsset DefaultFont
    {
        get
        {
            if (cachedFont != null)
                return cachedFont;

            _ = TMP_Settings.instance;
            cachedFont = TMP_Settings.defaultFontAsset;
            if (cachedFont != null)
                return cachedFont;

            cachedFont = Resources.Load<TMP_FontAsset>(LiberationSansResourcePath);
            if (cachedFont != null)
                return cachedFont;

            cachedFont = Resources.Load<TMP_FontAsset>(LiberationSansFallbackResourcePath);
            if (cachedFont != null)
                return cachedFont;

#if UNITY_EDITOR
            cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LiberationSansAssetPath);
            if (cachedFont == null)
            {
                cachedFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                    "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset");
            }
#endif

            return cachedFont;
        }
    }

    public static void EnsureAssigned(TMP_Text text, bool preserveSharedMaterial = false)
    {
        if (text == null)
            return;

        if (text.font == null)
        {
            var font = DefaultFont;
            if (font == null)
                return;

            text.font = font;
            if (!preserveSharedMaterial && font.material != null)
                text.fontSharedMaterial = font.material;
        }

        text.ForceMeshUpdate(true);
    }

    public static void EnsureAllInScene()
    {
        var font = DefaultFont;
        if (font == null)
            return;

        var texts = Object.FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (var text in texts)
            EnsureAssigned(text, preserveSharedMaterial: text.fontSharedMaterial != null);
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void RegisterEditorSceneFix()
    {
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (_, _) =>
        {
            EditorApplication.delayCall += EnsureAllInScene;
        };

        EditorApplication.delayCall += EnsureAllInScene;
    }
#endif
}
