using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RunSessionController : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private RunResultMenuUI resultMenuUI;

    private bool runFinished;

    private void Awake()
    {
        if (enemySpawner == null)
            enemySpawner = FindFirstObjectByType<EnemySpawner>();

        if (playerHealth == null)
            playerHealth = FindFirstObjectByType<PlayerHealth>();

        if (resultMenuUI == null)
            resultMenuUI = FindFirstObjectByType<RunResultMenuUI>();
    }

    private void Start()
    {
        if (playerHealth != null)
            playerHealth.OnDied += HandleLoss;

        if (enemySpawner != null)
        {
            enemySpawner.OnSessionComplete += HandleSessionComplete;
            enemySpawner.OnActiveBossCountChanged += HandleBossCountChanged;
        }

        // Edge case: if session already completed when the controller starts.
        TryShowVictory();
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= HandleLoss;

        if (enemySpawner != null)
        {
            enemySpawner.OnSessionComplete -= HandleSessionComplete;
            enemySpawner.OnActiveBossCountChanged -= HandleBossCountChanged;
        }
    }

    private void HandleLoss()
    {
        if (runFinished)
            return;

        runFinished = true;
        if (enemySpawner != null)
            enemySpawner.enabled = false;
        resultMenuUI?.ShowLoss();
    }

    private void HandleSessionComplete()
    {
        TryShowVictory();
    }

    private void HandleBossCountChanged(int _)
    {
        TryShowVictory();
    }

    private void TryShowVictory()
    {
        if (runFinished)
            return;

        if (enemySpawner == null || !enemySpawner.IsSessionComplete)
            return;

        if (enemySpawner.ActiveBossCount != 0)
            return;

        runFinished = true;
        enemySpawner.enabled = false;
        resultMenuUI?.ShowVictory();
    }
}

