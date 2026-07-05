using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuUI : MonoBehaviour
{
    [SerializeField] private string gameplaySceneName = "SampleScene";
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private SettingsMenuUI settingsMenu;
    [SerializeField] private AudioClip buttonClickClip;

    private void Awake()
    {
        GamePauseController.ForceResume();
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ShowMainPanel();
    }

    private void Start()
    {
        if (settingsMenu != null)
            settingsMenu.Closed += ShowMainPanel;
    }

    private void OnDestroy()
    {
        if (settingsMenu != null)
            settingsMenu.Closed -= ShowMainPanel;
    }

    public void StartGame()
    {
        PlayClick();
        GamePauseController.ForceResume();
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        PlayClick();

        if (mainPanel != null)
            mainPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void ShowMainPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainPanel != null)
            mainPanel.SetActive(true);
    }

    public void QuitGame()
    {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayClick()
    {
        if (buttonClickClip != null)
            AudioManager.Instance?.PlaySfx(buttonClickClip);
    }
}
