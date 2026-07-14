using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class PauseMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button pauseToggleButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private SettingsMenuUI settingsMenu;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private AudioClip buttonClickClip;

    private bool isOpen;
    private bool settingsOpen;

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void Start()
    {
        if (settingsMenu != null)
            settingsMenu.Closed += CloseSettings;

        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
    }

    private void OnDestroy()
    {
        if (settingsMenu != null)
            settingsMenu.Closed -= CloseSettings;
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (settingsOpen)
        {
            CloseSettings();
            return;
        }

        if (!GamePauseController.CanOpenPauseMenu)
            return;

        TogglePause();
    }

    public void TogglePause()
    {
        if (!GamePauseController.CanOpenPauseMenu)
            return;

        Debug.Log("togglep");

        if (isOpen)
            Resume();
        else
            OpenPause();
    }

    public void OpenPause()
    {
        if (!GamePauseController.CanOpenPauseMenu || pausePanel == null)
            return;

        isOpen = true;
        settingsOpen = false;
        pausePanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        GamePauseController.RequestPause(GamePauseController.PauseReason.PauseMenu);
        SwitchActionMap("UI");
    }

    public void Resume()
    {
        if (!isOpen)
            return;

        isOpen = false;
        settingsOpen = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        GamePauseController.ReleasePause(GamePauseController.PauseReason.PauseMenu);
        SwitchActionMap("Player");
    }

    public void OpenSettings()
    {
        if (!isOpen || settingsPanel == null)
            return;

        PlayClick();
        settingsOpen = true;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (!settingsOpen)
            return;

        settingsOpen = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(true);
    }

    public void ReturnToMainMenu()
    {
        PlayClick();
        isOpen = false;
        settingsOpen = false;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        GamePauseController.ForceResume();
        SwitchActionMap("Player");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SwitchActionMap(string mapName)
    {
        if (playerInput == null || string.IsNullOrEmpty(mapName))
            return;

        if (playerInput.actions == null || playerInput.actions.FindActionMap(mapName) == null)
            return;

        playerInput.SwitchCurrentActionMap(mapName);
    }

    private void PlayClick()
    {
        if (buttonClickClip != null)
            AudioManager.Instance?.PlaySfx(buttonClickClip);
    }
}
