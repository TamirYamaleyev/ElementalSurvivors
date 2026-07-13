using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class RunResultMenuUI : MonoBehaviour
{
    const string VictoryMessage = "Thanks for playing!";
    const string LossMessage = "Better luck next time!";

    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject lossPanel;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private RunSessionController runSessionController;
    [SerializeField] private AudioClip buttonClickClip;

    [Header("Victory Panel")]
    [SerializeField] private TMP_Text victoryTitleText;
    [SerializeField] private TMP_Text victorySurviveTimeText;
    [SerializeField] private TMP_Text victoryMessageText;
    [SerializeField] private Button victoryEndlessButton;
    [SerializeField] private Button victoryRestartButton;
    [SerializeField] private Button victoryExitButton;

    [Header("Loss Panel")]
    [SerializeField] private TMP_Text lossTitleText;
    [SerializeField] private TMP_Text lossSurviveTimeText;
    [SerializeField] private TMP_Text lossMessageText;
    [SerializeField] private Button lossEndlessButton;
    [SerializeField] private Button lossRestartButton;
    [SerializeField] private Button lossExitButton;

    private LevelUpUI levelUpUI;

    private void Awake()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(false);

        levelUpUI = FindFirstObjectByType<LevelUpUI>();

        if (lossEndlessButton != null)
            lossEndlessButton.interactable = false;
    }

    private void Start()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (runSessionController == null)
            runSessionController = FindFirstObjectByType<RunSessionController>();
    }

    public void ShowVictory()
    {
        levelUpUI?.HideLevelUpPanel();
        ApplyVictoryContent();

        if (victoryEndlessButton != null)
            victoryEndlessButton.interactable = true;

        if (victoryPanel != null)
            victoryPanel.SetActive(true);

        if (lossPanel != null)
            lossPanel.SetActive(false);

        GamePauseController.RequestPause(GamePauseController.PauseReason.RunEnd);
        SwitchActionMap("UI");
    }

    public void ShowLoss()
    {
        ShowLoss(isEndless: false, ResolveElapsedSeconds(), bestSeconds: 0, isNewRecord: false);
    }

    public void ShowLoss(bool isEndless, int elapsedSeconds, int bestSeconds, bool isNewRecord)
    {
        levelUpUI?.HideLevelUpPanel();
        ApplyLossContent(isEndless, elapsedSeconds, bestSeconds, isNewRecord);

        if (lossEndlessButton != null)
            lossEndlessButton.interactable = false;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        if (lossPanel != null)
            lossPanel.SetActive(true);

        GamePauseController.RequestPause(GamePauseController.PauseReason.RunEnd);
        SwitchActionMap("UI");
    }

    public void Restart()
    {
        PlayClick();
        GamePauseController.ForceResume();
        RunLaunchContext.PendingMode = RunMode.Standard;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>Legacy alias for Restart (older prefab OnClick wiring).</summary>
    public void Retry() => Restart();

    public void ExitToMainMenu()
    {
        PlayClick();
        GamePauseController.ForceResume();
        RunLaunchContext.PendingMode = RunMode.Standard;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>Legacy alias for ExitToMainMenu.</summary>
    public void ReturnToMainMenu() => ExitToMainMenu();

    public void OnEndlessClicked()
    {
        PlayClick();

        if (runSessionController == null)
            runSessionController = FindFirstObjectByType<RunSessionController>();

        if (runSessionController == null)
            return;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        runSessionController.ResumeFromVictoryEndless();
    }

    private void ApplyVictoryContent()
    {
        string timeText = EndlessBestTimeStore.FormatTime(ResolveElapsedSeconds());

        if (victoryTitleText != null)
            victoryTitleText.text = "Victory";
        if (victorySurviveTimeText != null)
            victorySurviveTimeText.text = timeText;
        if (victoryMessageText != null)
            victoryMessageText.text = VictoryMessage;
    }

    private void ApplyLossContent(bool isEndless, int elapsedSeconds, int bestSeconds, bool isNewRecord)
    {
        string timeText = EndlessBestTimeStore.FormatTime(elapsedSeconds);

        if (lossTitleText != null)
            lossTitleText.text = "Defeat";
        if (lossSurviveTimeText != null)
            lossSurviveTimeText.text = timeText;

        if (lossMessageText == null)
            return;

        if (!isEndless)
        {
            lossMessageText.text = LossMessage;
            return;
        }

        string bestText = EndlessBestTimeStore.FormatTime(bestSeconds);
        if (isNewRecord)
            lossMessageText.text = $"Time: {timeText}\nBest: {bestText}\nNew Record!";
        else
            lossMessageText.text = $"Time: {timeText}\nBest: {bestText}";
    }

    private int ResolveElapsedSeconds()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (enemySpawner == null)
            return 0;

        return Mathf.Max(0, Mathf.FloorToInt(enemySpawner.ElapsedTime));
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
