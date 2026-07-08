using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RunResultMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private AudioClip buttonClickClip;

    private LevelUpUI levelUpUI;

    private void Awake()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(false);

        levelUpUI = FindFirstObjectByType<LevelUpUI>();
    }

    private void Start()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public void ShowVictory()
    {
        levelUpUI?.HideLevelUpPanel();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (lossPanel != null)
            lossPanel.SetActive(false);

        GamePauseController.RequestPause(GamePauseController.PauseReason.RunEnd);
        SwitchActionMap("UI");
    }

    public void ShowLoss()
    {
        levelUpUI?.HideLevelUpPanel();

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(true);

        GamePauseController.RequestPause(GamePauseController.PauseReason.RunEnd);
        SwitchActionMap("UI");
    }

    public void Retry()
    {
        PlayClick();
        GamePauseController.ForceResume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        PlayClick();
        GamePauseController.ForceResume();
        SceneManager.LoadScene("MainMenu");
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

