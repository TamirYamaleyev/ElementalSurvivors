using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public sealed class RunSessionController : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RunResultMenuUI resultMenuUI;
    [SerializeField] private PlayerInput playerInput;

    private bool runFinished;
    private bool isEndless;

    public bool IsEndless => isEndless;

    private void Awake()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (resultMenuUI == null)
            resultMenuUI = FindFirstObjectByType<RunResultMenuUI>();

        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
    }

    private void Start()
    {
        if (RunLaunchContext.ConsumePendingMode() == RunMode.Endless)
        {
            isEndless = true;
            enemySpawner?.BeginEndlessFromLaunch();
        }

        if (playerHealth != null)
            playerHealth.OnDied += HandleLoss;

        if (enemySpawner != null)
        {
            enemySpawner.OnSessionComplete += HandleSessionComplete;
            enemySpawner.OnFinalBossDefeated += HandleFinalBossDefeated;
        }

        TryShowVictory();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandleLoss;

        if (enemySpawner != null)
        {
            enemySpawner.OnSessionComplete -= HandleSessionComplete;
            enemySpawner.OnFinalBossDefeated -= HandleFinalBossDefeated;
        }
    }

    private void HandleLoss()
    {
        if (runFinished)
            return;

        runFinished = true;
        if (enemySpawner != null)
            enemySpawner.enabled = false;

        int elapsed = enemySpawner != null
            ? Mathf.Max(0, Mathf.FloorToInt(enemySpawner.ElapsedTime))
            : 0;

        if (isEndless)
        {
            bool isNewRecord = EndlessBestTimeStore.TryUpdateBest(elapsed);
            int best = EndlessBestTimeStore.GetBestSeconds();
            resultMenuUI?.ShowLoss(isEndless: true, elapsed, bestSeconds: best, isNewRecord: isNewRecord);
            return;
        }

        resultMenuUI?.ShowLoss(isEndless: false, elapsed, bestSeconds: 0, isNewRecord: false);
    }

    private void HandleSessionComplete()
    {
        TryShowVictory();
    }

    private void HandleFinalBossDefeated()
    {
        TryShowVictory();
    }

    private void TryShowVictory()
    {
        if (runFinished || isEndless)
            return;

        if (enemySpawner == null || !enemySpawner.IsSessionComplete)
            return;

        if (!enemySpawner.FinalBossWasSpawned)
            return;

        if (enemySpawner.IsFinalBossAlive)
            return;

        runFinished = true;
        enemySpawner.enabled = false;
        resultMenuUI?.ShowVictory();
    }

    public void ResumeFromVictoryEndless()
    {
        if (enemySpawner == null)
            return;

        runFinished = false;
        isEndless = true;
        enemySpawner.enabled = true;
        enemySpawner.EnterEndlessMode();

        GamePauseController.ForceResume();
        SwitchActionMap("Player");
    }

    private void SwitchActionMap(string mapName)
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        if (playerInput == null || string.IsNullOrEmpty(mapName))
            return;

        if (playerInput.actions == null || playerInput.actions.FindActionMap(mapName) == null)
            return;

        playerInput.SwitchCurrentActionMap(mapName);
    }
}
