using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AudioManagerPrefabBuilder
{
    const string PrefabPath = "Assets/Prefabs/Audio/PF_AudioManager.prefab";
    const string ScenePath = "Assets/Scenes/SampleScene.unity";
    const string BgmFolder = "Assets/Audio/BGM";

    [MenuItem("Tools/Audio/Build AudioManager Prefab And Wire Scene")]
    public static void BuildFromMenu()
    {
        Build();
        Debug.Log("[AudioManagerPrefabBuilder] Done.");
    }

    public static void BuildFromCli()
    {
        Build();
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[AudioManagerPrefabBuilder] CLI build complete.");
        EditorApplication.Exit(0);
    }

    static void Build()
    {
        EnsureFolder("Assets/Prefabs/Audio");
        EnsureFolder(BgmFolder);

        var root = new GameObject("PF_AudioManager");
        try
        {
            var sfxGo = new GameObject("SfxPlayer");
            sfxGo.transform.SetParent(root.transform, false);
            sfxGo.AddComponent<AudioSource>();
            var sfxSource = sfxGo.GetComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            var sfx = sfxGo.AddComponent<SfxPlayer>();

            var bgmGo = new GameObject("BgmPlayer");
            bgmGo.transform.SetParent(root.transform, false);
            var bgmSource = bgmGo.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
            bgmSource.spatialBlend = 0f;
            var bgm = bgmGo.AddComponent<BgmPlayer>();

            var manager = root.AddComponent<AudioManager>();
            var so = new SerializedObject(manager);
            so.FindProperty("sfxPlayer").objectReferenceValue = sfx;
            so.FindProperty("bgmPlayer").objectReferenceValue = bgm;

            var defaultBgm = FindFirstBgmClip();
            if (defaultBgm != null)
            {
                so.FindProperty("defaultBgm").objectReferenceValue = defaultBgm;
                var bgmSo = new SerializedObject(bgm);
                bgmSo.FindProperty("defaultClip").objectReferenceValue = defaultBgm;
                bgmSo.ApplyModifiedPropertiesWithoutUndo();
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            WireSceneInstance();
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    static AudioClip FindFirstBgmClip()
    {
        var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { BgmFolder });
        if (guids.Length == 0)
            return null;

        return AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    static void WireSceneInstance()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var existing = GameObject.Find("AudioManager");
        if (existing != null)
            Object.DestroyImmediate(existing);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[AudioManagerPrefabBuilder] Prefab missing at " + PrefabPath);
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = "AudioManager";
        SceneManager.MoveGameObjectToScene(instance, scene);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    static void EnsureFolder(string path)
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
