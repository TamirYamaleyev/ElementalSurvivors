#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MenuSceneSetup
{
    const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
    const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    const string BossCombatTestPath = "Assets/Scenes/BossCombatTest.unity";
    const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
    const string AudioManagerPrefabPath = "Assets/Prefabs/Audio/PF_AudioManager.prefab";

    const string MenuPrefabsFolder = "Assets/Prefabs/UI/Menus";
    const string SettingsPrefabPath = MenuPrefabsFolder + "/PF_SettingsPanel.prefab";
    const string MainMenuPrefabPath = MenuPrefabsFolder + "/PF_MainMenu.prefab";
    const string PauseMenuPrefabPath = MenuPrefabsFolder + "/PF_PauseMenu.prefab";
    const string RunResultPrefabPath = MenuPrefabsFolder + "/PF_RunResultMenu.prefab";

    [MenuItem("Tools/Menu/Build Menu Prefabs From Layout")]
    public static void BuildMenuPrefabsMenu()
    {
        var checklist = new List<string>();
        BuildAllMenuPrefabs(checklist);
        AssetDatabase.SaveAssets();
        Debug.Log("[MenuSceneSetup] Prefabs built: " + string.Join(", ", checklist));
    }

    [MenuItem("Tools/Menu/Setup Main Menu And Pause Menu")]
    public static void SetupAllMenusMenu()
    {
        var checklist = new List<string>();
        EnsureMenuPrefabsExist(checklist);
        CreateOrUpdateMainMenuScene(checklist);
        SetupPauseMenuInScene(SampleScenePath, checklist);
        SetupPauseMenuInScene(BossCombatTestPath, checklist);
        SetupRunResultMenuInScene(SampleScenePath, checklist);
        SetupRunResultMenuInScene(BossCombatTestPath, checklist);
        SetupRunTimerInScene(SampleScenePath, checklist);
        SetupRunTimerInScene(BossCombatTestPath, checklist);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("[MenuSceneSetup] Completed: " + string.Join(", ", checklist));
    }

    public static void SetupAllFromCli()
    {
        try
        {
            SetupAllMenusMenu();
            Debug.Log("[MenuSceneSetup] CLI setup completed.");
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[MenuSceneSetup] CLI setup failed: " + ex);
            EditorApplication.Exit(1);
        }
    }

    public static void BuildMenuPrefabsFromCli()
    {
        try
        {
            var checklist = new List<string>();
            BuildAllMenuPrefabs(checklist);
            AssetDatabase.SaveAssets();
            Debug.Log("[MenuSceneSetup] Prefabs CLI build completed: " + string.Join(", ", checklist));
            EditorApplication.Exit(0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[MenuSceneSetup] Prefabs CLI build failed: " + ex);
            EditorApplication.Exit(1);
        }
    }

    static void EnsureMenuPrefabsExist(List<string> checklist)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(SettingsPrefabPath) == null
            || AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuPrefabPath) == null
            || AssetDatabase.LoadAssetAtPath<GameObject>(PauseMenuPrefabPath) == null
            || AssetDatabase.LoadAssetAtPath<GameObject>(RunResultPrefabPath) == null)
        {
            BuildAllMenuPrefabs(checklist);
        }
    }

    static void BuildAllMenuPrefabs(List<string> checklist)
    {
        EnsureFolder(MenuPrefabsFolder);

        var settingsPrefab = BuildSettingsPrefab(checklist);
        BuildMainMenuPrefab(settingsPrefab, checklist);
        BuildPauseMenuPrefab(settingsPrefab, checklist);
        BuildRunResultPrefab(checklist);
    }

    static GameObject BuildSettingsPrefab(List<string> checklist)
    {
        var root = CreateOverlayPanel(null, "PF_SettingsPanel");
        root.SetActive(false);

        var settingsMenu = root.AddComponent<SettingsMenuUI>();
        var scratch = new List<string>();

        var title = EnsureTmpText(root.transform, "SettingsTitle", "Settings", 42, scratch);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.82f);
        titleRect.anchorMax = new Vector2(0.5f, 0.82f);
        titleRect.sizeDelta = new Vector2(500f, 70f);
        titleRect.anchoredPosition = Vector2.zero;

        var masterSlider = EnsureVolumeSlider(root.transform, "MasterVolumeSlider", "Master Volume", 0.62f, scratch);
        var musicSlider = EnsureVolumeSlider(root.transform, "MusicVolumeSlider", "Music Volume", 0.5f, scratch);
        var sfxSlider = EnsureVolumeSlider(root.transform, "SfxVolumeSlider", "SFX Volume", 0.38f, scratch);
        var backButton = EnsureMenuButton(root.transform, "BackButton", "Back", new Vector2(0.5f, 0.18f), scratch);

        WireSettingsMenu(settingsMenu, masterSlider, musicSlider, sfxSlider, backButton);
        WireButton(backButton, settingsMenu, nameof(SettingsMenuUI.Close), scratch, "Settings.Back");

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, SettingsPrefabPath);
        Object.DestroyImmediate(root);
        checklist.Add("PF_SettingsPanel");
        return prefab;
    }

    static void BuildMainMenuPrefab(GameObject settingsPrefab, List<string> checklist)
    {
        var scratch = new List<string>();
        var canvas = CreateScreenCanvas("MainMenuCanvas");
        var menuRoot = new GameObject("MenuRoot", typeof(RectTransform));
        menuRoot.transform.SetParent(canvas.transform, false);
        Stretch(menuRoot.GetComponent<RectTransform>());

        var mainMenuUi = menuRoot.AddComponent<MainMenuUI>();
        var mainPanel = EnsureChildPanel(menuRoot.transform, "MainPanel", scratch);

        var title = EnsureTmpText(mainPanel.transform, "TitleText", "Elemental Survivors", 56, scratch);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.75f);
        titleRect.anchorMax = new Vector2(0.5f, 0.75f);
        titleRect.sizeDelta = new Vector2(700f, 90f);
        titleRect.anchoredPosition = Vector2.zero;

        var playButton = EnsureMenuButton(mainPanel.transform, "StartGameButton", "Start Game", new Vector2(0.5f, 0.48f), scratch);
        var settingsButton = EnsureMenuButton(mainPanel.transform, "SettingsButton", "Settings", new Vector2(0.5f, 0.36f), scratch);
        var quitButton = EnsureMenuButton(mainPanel.transform, "QuitButton", "Quit", new Vector2(0.5f, 0.24f), scratch);

        var settingsInstance = (GameObject)PrefabUtility.InstantiatePrefab(settingsPrefab);
        settingsInstance.name = "SettingsPanel";
        settingsInstance.transform.SetParent(menuRoot.transform, false);
        Stretch(settingsInstance.GetComponent<RectTransform>());
        settingsInstance.SetActive(false);

        var settingsMenu = settingsInstance.GetComponent<SettingsMenuUI>();
        WireMainMenuUi(mainMenuUi, mainPanel, settingsInstance, settingsMenu, playButton, settingsButton, quitButton, scratch);

        PrefabUtility.SaveAsPrefabAsset(canvas, MainMenuPrefabPath);
        Object.DestroyImmediate(canvas);
        checklist.Add("PF_MainMenu");
    }

    static void BuildPauseMenuPrefab(GameObject settingsPrefab, List<string> checklist)
    {
        var scratch = new List<string>();
        var root = new GameObject("PauseMenuRoot", typeof(RectTransform));
        Stretch(root.GetComponent<RectTransform>());

        var pauseUi = root.AddComponent<PauseMenuUI>();
        var pausePanel = CreateOverlayPanel(root.transform, "PauseMenuPanel");
        pausePanel.SetActive(false);

        var title = EnsureTmpText(pausePanel.transform, "PauseTitle", "Paused", 42, scratch);
        var titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleRect.sizeDelta = new Vector2(500f, 70f);
        titleRect.anchoredPosition = Vector2.zero;

        var resumeButton = EnsureMenuButton(pausePanel.transform, "ResumeButton", "Resume", new Vector2(0.5f, 0.48f), scratch);
        var settingsButton = EnsureMenuButton(pausePanel.transform, "SettingsButton", "Settings", new Vector2(0.5f, 0.36f), scratch);
        var mainMenuButton = EnsureMenuButton(pausePanel.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.24f), scratch);

        var settingsInstance = (GameObject)PrefabUtility.InstantiatePrefab(settingsPrefab);
        settingsInstance.name = "PauseSettingsPanel";
        settingsInstance.transform.SetParent(root.transform, false);
        Stretch(settingsInstance.GetComponent<RectTransform>());
        settingsInstance.SetActive(false);

        var settingsMenu = settingsInstance.GetComponent<SettingsMenuUI>();
        WirePauseMenuInternal(pauseUi, pausePanel, settingsInstance, settingsMenu, resumeButton, settingsButton, mainMenuButton, scratch);

        PrefabUtility.SaveAsPrefabAsset(root, PauseMenuPrefabPath);
        Object.DestroyImmediate(root);
        checklist.Add("PF_PauseMenu");
    }

    static void BuildRunResultPrefab(List<string> checklist)
    {
        var scratch = new List<string>();
        var root = new GameObject("RunResultMenuRoot", typeof(RectTransform));
        Stretch(root.GetComponent<RectTransform>());

        var runUi = root.AddComponent<RunResultMenuUI>();
        var lossPanel = CreateOverlayPanel(root.transform, "LossPanel");
        lossPanel.SetActive(false);
        var victoryPanel = CreateOverlayPanel(root.transform, "VictoryPanel");
        victoryPanel.SetActive(false);

        var titleLoss = EnsureTmpText(lossPanel.transform, "LossTitle", "Defeat", 42, scratch);
        var titleLossRect = titleLoss.GetComponent<RectTransform>();
        titleLossRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleLossRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleLossRect.sizeDelta = new Vector2(500f, 70f);
        titleLossRect.anchoredPosition = Vector2.zero;

        var retryButton = EnsureMenuButton(lossPanel.transform, "RetryButton", "Retry", new Vector2(0.5f, 0.48f), scratch);
        var mainMenuLossButton = EnsureMenuButton(lossPanel.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.36f), scratch);

        var titleVictory = EnsureTmpText(victoryPanel.transform, "VictoryTitle", "Victory", 42, scratch);
        var titleVictoryRect = titleVictory.GetComponent<RectTransform>();
        titleVictoryRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleVictoryRect.anchorMax = new Vector2(0.5f, 0.7f);
        titleVictoryRect.sizeDelta = new Vector2(500f, 70f);
        titleVictoryRect.anchoredPosition = Vector2.zero;

        var mainMenuVictoryButton = EnsureMenuButton(victoryPanel.transform, "MainMenuButton", "Main Menu", new Vector2(0.5f, 0.36f), scratch);

        var so = new SerializedObject(runUi);
        so.FindProperty("victoryPanel").objectReferenceValue = victoryPanel;
        so.FindProperty("lossPanel").objectReferenceValue = lossPanel;
        so.ApplyModifiedPropertiesWithoutUndo();

        WireButton(retryButton, runUi, nameof(RunResultMenuUI.Retry), scratch, "Result.Retry");
        WireButton(mainMenuLossButton, runUi, nameof(RunResultMenuUI.ReturnToMainMenu), scratch, "Result.LossMainMenu");
        WireButton(mainMenuVictoryButton, runUi, nameof(RunResultMenuUI.ReturnToMainMenu), scratch, "Result.VictoryMainMenu");

        PrefabUtility.SaveAsPrefabAsset(root, RunResultPrefabPath);
        Object.DestroyImmediate(root);
        checklist.Add("PF_RunResultMenu");
    }

    static void CreateOrUpdateMainMenuScene(List<string> checklist)
    {
        Scene scene;
        if (File.Exists(MainMenuScenePath))
            scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
        else
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        EnsureEventSystem(checklist);
        EnsureAudioManager(scene, checklist);

        var mainMenuPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainMenuPrefabPath);
        if (mainMenuPrefab == null)
        {
            Debug.LogError("[MenuSceneSetup] Missing " + MainMenuPrefabPath);
            return;
        }

        DestroyLegacyMainMenuObjects(scene);

        var canvasInstance = FindRootObject(scene, "MainMenuCanvas");
        var isPrefabInstance = canvasInstance != null
            && PrefabUtility.GetCorrespondingObjectFromSource(canvasInstance) == mainMenuPrefab;

        if (!isPrefabInstance)
        {
            if (canvasInstance != null)
                Object.DestroyImmediate(canvasInstance);

            canvasInstance = (GameObject)PrefabUtility.InstantiatePrefab(mainMenuPrefab, scene);
            canvasInstance.name = "MainMenuCanvas";
            checklist.Add("MainMenu.PF_MainMenu");
        }

        if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
        else
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }

    static void SetupPauseMenuInScene(string scenePath, List<string> checklist)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var hud = FindRootObject(scene, "HUD");
        if (hud == null)
        {
            Debug.LogWarning("[MenuSceneSetup] HUD not found in " + scenePath);
            return;
        }

        var pausePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PauseMenuPrefabPath);
        if (pausePrefab == null)
        {
            Debug.LogError("[MenuSceneSetup] Missing " + PauseMenuPrefabPath);
            return;
        }

        DestroyLegacyPauseObjects(hud.transform);

        var pauseInstance = FindChild(hud.transform, "PauseMenuRoot");
        if (pauseInstance == null)
        {
            pauseInstance = (GameObject)PrefabUtility.InstantiatePrefab(pausePrefab);
            pauseInstance.name = "PauseMenuRoot";
            pauseInstance.transform.SetParent(hud.transform, false);
            Stretch(pauseInstance.GetComponent<RectTransform>());
            checklist.Add(scenePath + ".PF_PauseMenu");
        }

        var pauseUi = pauseInstance.GetComponent<PauseMenuUI>();
        var pauseToggle = FindChild(hud.transform, "PauseButton");
        if (pauseToggle != null && pauseToggle.GetComponent<Button>() == null)
        {
            pauseToggle.AddComponent<Button>();
            checklist.Add(scenePath + ".PauseButton.Button");
        }

        var playerInput = Object.FindFirstObjectByType<PlayerInput>();
        WirePauseMenuSceneRefs(pauseUi, pauseToggle, playerInput);

        if (pauseToggle != null)
            WireButton(pauseToggle.GetComponent<Button>(), pauseUi, nameof(PauseMenuUI.TogglePause), checklist, scenePath + ".PauseToggle");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void SetupRunResultMenuInScene(string scenePath, List<string> checklist)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var hud = FindRootObject(scene, "HUD");
        if (hud == null)
        {
            Debug.LogWarning("[MenuSceneSetup] HUD not found in " + scenePath);
            return;
        }

        var resultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RunResultPrefabPath);
        if (resultPrefab == null)
        {
            Debug.LogError("[MenuSceneSetup] Missing " + RunResultPrefabPath);
            return;
        }

        DestroyLegacyResultObjects(hud.transform);

        var resultInstance = FindChild(hud.transform, "RunResultMenuRoot");
        if (resultInstance == null)
        {
            resultInstance = (GameObject)PrefabUtility.InstantiatePrefab(resultPrefab);
            resultInstance.name = "RunResultMenuRoot";
            resultInstance.transform.SetParent(hud.transform, false);
            Stretch(resultInstance.GetComponent<RectTransform>());
            checklist.Add(scenePath + ".PF_RunResultMenu");
        }

        var runUi = resultInstance.GetComponent<RunResultMenuUI>();
        var playerInput = Object.FindFirstObjectByType<PlayerInput>();
        WireRunResultSceneRefs(runUi, playerInput);

        var controllerRoot = FindChild(hud.transform, "RunSessionControllerRoot");
        if (controllerRoot == null)
        {
            controllerRoot = new GameObject("RunSessionControllerRoot", typeof(RunSessionController));
            controllerRoot.transform.SetParent(hud.transform, false);
            checklist.Add(scenePath + ".RunSessionControllerRoot");
        }

        var runController = controllerRoot.GetComponent<RunSessionController>();
        var spawner = Object.FindFirstObjectByType<EnemySpawner>();
        var player = Object.FindFirstObjectByType<PlayerHealth>();

        var runSo = new SerializedObject(runController);
        runSo.FindProperty("enemySpawner").objectReferenceValue = spawner;
        runSo.FindProperty("playerHealth").objectReferenceValue = player;
        runSo.FindProperty("resultMenuUI").objectReferenceValue = runUi;
        runSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void SetupRunTimerInScene(string scenePath, List<string> checklist)
    {
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        var hud = FindRootObject(scene, "HUD");
        if (hud == null)
        {
            Debug.LogWarning("[MenuSceneSetup] HUD not found in " + scenePath);
            return;
        }

        var healthbar = FindChild(hud.transform, "Healthbar");
        float belowHealthY = -95f;
        if (healthbar != null)
        {
            var hbRect = healthbar.GetComponent<RectTransform>();
            belowHealthY = hbRect.anchoredPosition.y - hbRect.sizeDelta.y - 10f;
        }

        var timerGo = FindChild(hud.transform, "RunTimerText");
        if (timerGo == null)
        {
            timerGo = new GameObject("RunTimerText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(TmpFontOnEnable));
            timerGo.transform.SetParent(hud.transform, false);
            checklist.Add(scenePath + ".RunTimerText");
        }

        var rect = timerGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.sizeDelta = new Vector2(220f, 40f);
        rect.anchoredPosition = new Vector2(5f, belowHealthY);

        var tmp = timerGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "00:00";
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;

        var controller = timerGo.GetComponent<RunTimerUIController>();
        if (controller == null)
            controller = timerGo.AddComponent<RunTimerUIController>();

        var spawner = Object.FindFirstObjectByType<EnemySpawner>();

        var so = new SerializedObject(controller);
        so.FindProperty("spawner").objectReferenceValue = spawner;
        so.FindProperty("timerText").objectReferenceValue = tmp;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void DestroyLegacyMainMenuObjects(Scene scene)
    {
        // Prefab instance replaces any previously baked MainMenuCanvas tree.
    }

    static void DestroyLegacyPauseObjects(Transform hud)
    {
        // Old layout kept panels as HUD siblings; prefab keeps them under PauseMenuRoot.
        DestroyChildIfPresent(hud, "PauseMenuPanel");
        DestroyChildIfPresent(hud, "PauseSettingsPanel");

        var pauseRoot = FindChild(hud, "PauseMenuRoot");
        if (pauseRoot == null)
            return;

        var pausePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PauseMenuPrefabPath);
        if (pausePrefab != null && PrefabUtility.GetCorrespondingObjectFromSource(pauseRoot) == pausePrefab)
            return;

        Object.DestroyImmediate(pauseRoot);
    }

    static void DestroyLegacyResultObjects(Transform hud)
    {
        DestroyChildIfPresent(hud, "LossPanel");
        DestroyChildIfPresent(hud, "VictoryPanel");

        var runRoot = FindChild(hud, "RunResultMenuRoot");
        if (runRoot == null)
            return;

        var resultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(RunResultPrefabPath);
        if (resultPrefab != null && PrefabUtility.GetCorrespondingObjectFromSource(runRoot) == resultPrefab)
            return;

        Object.DestroyImmediate(runRoot);
    }

    static void DestroyChildIfPresent(Transform parent, string name)
    {
        var child = FindChild(parent, name);
        if (child == null)
            return;

        // Keep objects that already live under a menu prefab instance.
        if (PrefabUtility.IsPartOfPrefabInstance(child)
            && PrefabUtility.GetNearestPrefabInstanceRoot(child) != child)
            return;

        Object.DestroyImmediate(child);
    }

    static void WireSettingsMenu(
        SettingsMenuUI settingsMenu,
        Slider masterSlider,
        Slider musicSlider,
        Slider sfxSlider,
        Button backButton)
    {
        var so = new SerializedObject(settingsMenu);
        so.FindProperty("masterSlider").objectReferenceValue = masterSlider;
        so.FindProperty("musicSlider").objectReferenceValue = musicSlider;
        so.FindProperty("sfxSlider").objectReferenceValue = sfxSlider;
        so.FindProperty("masterValueLabel").objectReferenceValue = FindValueLabel(masterSlider);
        so.FindProperty("musicValueLabel").objectReferenceValue = FindValueLabel(musicSlider);
        so.FindProperty("sfxValueLabel").objectReferenceValue = FindValueLabel(sfxSlider);
        so.FindProperty("backButton").objectReferenceValue = backButton;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static TMP_Text FindValueLabel(Slider slider)
    {
        if (slider == null || slider.transform.parent == null)
            return null;

        return FindChild(slider.transform.parent, "ValueLabel")?.GetComponent<TMP_Text>();
    }

    static void WireMainMenuUi(
        MainMenuUI ui,
        GameObject mainPanel,
        GameObject settingsPanel,
        SettingsMenuUI settingsMenu,
        Button playButton,
        Button settingsButton,
        Button quitButton,
        List<string> checklist)
    {
        var so = new SerializedObject(ui);
        so.FindProperty("gameplaySceneName").stringValue = "SampleScene";
        so.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        so.FindProperty("playButton").objectReferenceValue = playButton;
        so.FindProperty("settingsButton").objectReferenceValue = settingsButton;
        so.FindProperty("quitButton").objectReferenceValue = quitButton;
        so.FindProperty("settingsMenu").objectReferenceValue = settingsMenu;
        so.ApplyModifiedPropertiesWithoutUndo();

        WireButton(playButton, ui, nameof(MainMenuUI.StartGame), checklist, "MainMenu.Start");
        WireButton(settingsButton, ui, nameof(MainMenuUI.OpenSettings), checklist, "MainMenu.Settings");
        WireButton(quitButton, ui, nameof(MainMenuUI.QuitGame), checklist, "MainMenu.Quit");
    }

    static void WirePauseMenuInternal(
        PauseMenuUI ui,
        GameObject pausePanel,
        GameObject settingsPanel,
        SettingsMenuUI settingsMenu,
        Button resumeButton,
        Button settingsButton,
        Button mainMenuButton,
        List<string> checklist)
    {
        var so = new SerializedObject(ui);
        so.FindProperty("pausePanel").objectReferenceValue = pausePanel;
        so.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        so.FindProperty("resumeButton").objectReferenceValue = resumeButton;
        so.FindProperty("settingsButton").objectReferenceValue = settingsButton;
        so.FindProperty("mainMenuButton").objectReferenceValue = mainMenuButton;
        so.FindProperty("settingsMenu").objectReferenceValue = settingsMenu;
        so.FindProperty("mainMenuSceneName").stringValue = "MainMenu";
        so.ApplyModifiedPropertiesWithoutUndo();

        WireButton(resumeButton, ui, nameof(PauseMenuUI.Resume), checklist, "Pause.Resume");
        WireButton(settingsButton, ui, nameof(PauseMenuUI.OpenSettings), checklist, "Pause.Settings");
        WireButton(mainMenuButton, ui, nameof(PauseMenuUI.ReturnToMainMenu), checklist, "Pause.MainMenu");
    }

    static void WirePauseMenuSceneRefs(PauseMenuUI ui, GameObject pauseToggle, PlayerInput playerInput)
    {
        if (ui == null)
            return;

        var so = new SerializedObject(ui);
        so.FindProperty("pauseToggleButton").objectReferenceValue = pauseToggle != null ? pauseToggle.GetComponent<Button>() : null;
        so.FindProperty("playerInput").objectReferenceValue = playerInput;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void WireRunResultSceneRefs(RunResultMenuUI ui, PlayerInput playerInput)
    {
        if (ui == null)
            return;

        var so = new SerializedObject(ui);
        so.FindProperty("playerInput").objectReferenceValue = playerInput;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureAudioManager(Scene scene, List<string> checklist)
    {
        if (Object.FindFirstObjectByType<AudioManager>() != null)
            return;

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioManagerPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[MenuSceneSetup] AudioManager prefab not found at " + AudioManagerPrefabPath);
            return;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = "AudioManager";
        checklist.Add("MainMenu.AudioManager");
    }

    static void WireButton(Button button, Object target, string methodName, List<string> checklist, string label)
    {
        if (button == null || target == null)
            return;

        var buttonSo = new SerializedObject(button);
        var calls = buttonSo.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        calls.ClearArray();
        calls.arraySize = 1;

        var call = calls.GetArrayElementAtIndex(0);
        call.FindPropertyRelative("m_Target").objectReferenceValue = target;
        call.FindPropertyRelative("m_MethodName").stringValue = methodName;
        call.FindPropertyRelative("m_Mode").enumValueIndex = 1;
        call.FindPropertyRelative("m_CallState").enumValueIndex = 2;

        buttonSo.ApplyModifiedPropertiesWithoutUndo();
        checklist.Add(label);
    }

    static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new(MainMenuScenePath, true),
            new(SampleScenePath, true)
        };

        foreach (var existing in EditorBuildSettings.scenes)
        {
            if (existing.path == MainMenuScenePath || existing.path == SampleScenePath)
                continue;

            scenes.Add(existing);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    static void EnsureEventSystem(List<string> checklist)
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null)
            return;

        var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        var module = eventSystem.GetComponent<InputSystemUIInputModule>();
        var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
        if (actions != null)
            module.actionsAsset = actions;

        checklist.Add("EventSystem");
    }

    static GameObject CreateScreenCanvas(string name)
    {
        var canvasGo = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 600f);
        return canvasGo;
    }

    static GameObject CreateOverlayPanel(Transform parent, string name)
    {
        var panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (parent != null)
            panel.transform.SetParent(parent, false);

        var rect = panel.GetComponent<RectTransform>();
        Stretch(rect);

        var image = panel.GetComponent<Image>();
        image.color = new Color(0.04f, 0.03f, 0.03f, 0.92f);
        image.raycastTarget = true;
        return panel;
    }

    static GameObject EnsureChildPanel(Transform parent, string name, List<string> checklist)
    {
        var panel = FindChild(parent, name);
        if (panel != null)
            return panel;

        panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);
        Stretch(panel.GetComponent<RectTransform>());
        checklist.Add(name);
        return panel;
    }

    static GameObject EnsureTmpText(Transform parent, string name, string text, int fontSize, List<string> checklist)
    {
        var existing = FindChild(parent, name);
        if (existing != null)
        {
            var existingTmp = existing.GetComponent<TextMeshProUGUI>();
            if (existingTmp != null)
                existingTmp.text = text;
            return existing;
        }

        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(TmpFontOnEnable));
        go.transform.SetParent(parent, false);

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        checklist.Add(name);
        return go;
    }

    static Button EnsureMenuButton(Transform parent, string name, string label, Vector2 anchor, List<string> checklist)
    {
        var existing = FindChild(parent, name);
        GameObject go;
        if (existing != null)
        {
            go = existing;
            var existingLabel = FindChild(go.transform, "Label")?.GetComponent<TextMeshProUGUI>();
            if (existingLabel != null)
                existingLabel.text = label;
        }
        else
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            checklist.Add(name);
        }

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.sizeDelta = new Vector2(260f, 56f);
        rect.anchoredPosition = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.color = new Color(0.2f, 0.22f, 0.28f, 1f);

        var labelGo = FindChild(go.transform, "Label");
        if (labelGo == null)
        {
            labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(TmpFontOnEnable));
            labelGo.transform.SetParent(go.transform, false);
            Stretch(labelGo.GetComponent<RectTransform>());
        }

        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go.GetComponent<Button>();
    }

    static Slider EnsureVolumeSlider(Transform parent, string name, string label, float anchorY, List<string> checklist)
    {
        var existing = FindChild(parent, name);
        GameObject row;
        if (existing != null)
        {
            row = existing;
        }
        else
        {
            row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            checklist.Add(name);

            var rowRect = row.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, anchorY);
            rowRect.anchorMax = new Vector2(0.5f, anchorY);
            rowRect.sizeDelta = new Vector2(520f, 48f);
            rowRect.anchoredPosition = Vector2.zero;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(TmpFontOnEnable));
            labelGo.transform.SetParent(row.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.sizeDelta = new Vector2(180f, 40f);
            labelRect.anchoredPosition = Vector2.zero;
            var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
            labelTmp.text = label;
            labelTmp.fontSize = 24;
            labelTmp.alignment = TextAlignmentOptions.MidlineLeft;

            var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            sliderGo.transform.SetParent(row.transform, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.offsetMin = new Vector2(190f, -12f);
            sliderRect.offsetMax = new Vector2(-70f, 12f);

            var slider = sliderGo.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(sliderGo.transform, false);
            Stretch(bg.GetComponent<RectTransform>());
            bg.GetComponent<Image>().color = new Color(0.15f, 0.16f, 0.2f, 1f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>());

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(fillArea.transform, false);
            Stretch(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = new Color(0.35f, 0.55f, 0.85f, 1f);

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderGo.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>());

            var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handle.transform.SetParent(handleArea.transform, false);
            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20f, 20f);
            handle.GetComponent<Image>().color = Color.white;

            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();

            var valueGo = new GameObject("ValueLabel", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(TmpFontOnEnable));
            valueGo.transform.SetParent(row.transform, false);
            var valueRect = valueGo.GetComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(1f, 0.5f);
            valueRect.anchorMax = new Vector2(1f, 0.5f);
            valueRect.pivot = new Vector2(1f, 0.5f);
            valueRect.sizeDelta = new Vector2(60f, 40f);
            valueRect.anchoredPosition = Vector2.zero;
            var valueTmp = valueGo.GetComponent<TextMeshProUGUI>();
            valueTmp.text = "100%";
            valueTmp.fontSize = 22;
            valueTmp.alignment = TextAlignmentOptions.MidlineRight;
        }

        return row.transform.Find("Slider")?.GetComponent<Slider>();
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

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    static GameObject FindChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child.gameObject;
        }

        return null;
    }

    static GameObject FindRootObject(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }
}
#endif
