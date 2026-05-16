using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunDifficultyProfileSO runProfile;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private EnemyAI levelOneEnemy;
    [SerializeField] private EnemyAI levelTwoEnemy;
    [SerializeField] private EnemyAI levelThreeEnemy;
    [SerializeField] private EnemyAI bossEnemy;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private float spawnJitter = 0.5f;

    private float timer;
    private float elapsedTime;
    private float nextBossAt;

    private Transform player;

    void Start()
    {
        player = PlayerController.Instance;
        timer = spawnInterval;

        if (runProfile != null)
            nextBossAt = runProfile.firstBossDelaySeconds;
    }

    void Update()
    {
        if (runProfile == null || enemyPool == null || player == null)
            return;

        elapsedTime += Time.deltaTime;

        if (runProfile.stopSpawningWhenSessionEnds &&
            elapsedTime >= runProfile.sessionDurationSeconds)
            return;

        if (RunDifficultyEvaluator.ShouldSpawnBoss(runProfile, elapsedTime, ref nextBossAt))
            SpawnEnemy(GetBossEnemy(), isBoss: true);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy(GetTierEnemy(), isBoss: false);
            timer = spawnInterval;
        }
    }

    private EnemyAI GetTierEnemy()
    {
        int index = RunDifficultyEvaluator.GetPrefabIndex(runProfile, elapsedTime);

        return index switch
        {
            1 => levelTwoEnemy,
            2 => levelThreeEnemy,
            _ => levelOneEnemy,
        };
    }

    private EnemyAI GetBossEnemy()
    {
        if (bossEnemy != null)
            return bossEnemy;

        return levelThreeEnemy != null ? levelThreeEnemy : levelOneEnemy;
    }

    private void SpawnEnemy(EnemyAI prefab, bool isBoss)
    {
        if (prefab == null)
            return;

        int tierIndex = GetTierIndex(prefab);

        EnemyAI instance = enemyPool.Acquire(tierIndex);
        if (instance == null)
            return;

        Vector2 baseOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 jitter = Random.insideUnitCircle * spawnJitter;

        Vector3 spawnPos = player.position + new Vector3(
            baseOffset.x + jitter.x,
            baseOffset.y + jitter.y,
            0f
        );

        instance.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        instance.gameObject.SetActive(true);

        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        if (isBoss)
            multiplier *= runProfile.bossExtraStatMultiplier;

        float scaledHp = prefab.BaselineMaxHealth * multiplier;
        float scaledDamage = prefab.BaselineContactDamage * multiplier;

        instance.ApplyScaledStats(scaledHp, scaledDamage);

        if (isBoss && runProfile.bossVisualScale > 0f)
            instance.transform.localScale *= runProfile.bossVisualScale;
    }

    private int GetTierIndex(EnemyAI prefab)
    {
        if (prefab == levelTwoEnemy)
            return 1;

        if (prefab == levelThreeEnemy)
            return 2;

        return 0;
    }
}
