#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Keeps scene TextMeshPro objects patched when Font Asset references are missing.
/// </summary>
[InitializeOnLoad]
internal static class TmpFontAssetGuard
{
    private static double nextScenePatchTime;

    static TmpFontAssetGuard()
    {
        SceneView.duringSceneGui += OnSceneGui;
        EditorApplication.hierarchyChanged += SchedulePatch;
        EditorApplication.playModeStateChanged += _ => SchedulePatch();
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += (_, _) => SchedulePatch();
        UnityEditor.SceneManagement.PrefabStage.prefabStageOpened += _ => SchedulePatch();
        EditorApplication.delayCall += PatchMissingFonts;
    }

    private static void OnSceneGui(SceneView _)
    {
        if (EditorApplication.timeSinceStartup < nextScenePatchTime)
            return;

        nextScenePatchTime = EditorApplication.timeSinceStartup + 0.35;
        PatchMissingFonts();
    }

    private static void SchedulePatch()
    {
        EditorApplication.delayCall += PatchMissingFonts;
    }

    private static void PatchMissingFonts()
    {
        if (Application.isBatchMode)
            return;

        TmpFontUtility.EnsureAllInScene();
    }
}
#endif
