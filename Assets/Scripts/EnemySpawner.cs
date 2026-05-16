using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunDifficultyProfileSO runProfile;
    [SerializeField] private EnemyTierCatalogSO tierCatalog;
    [SerializeField] private EnemyPool enemyPool;

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
        if (runProfile == null || tierCatalog == null || enemyPool == null || player == null)
            return;

        elapsedTime += Time.deltaTime;

        if (runProfile.stopSpawningWhenSessionEnds &&
            elapsedTime >= runProfile.sessionDurationSeconds)
            return;

        if (RunDifficultyEvaluator.ShouldSpawnBoss(runProfile, elapsedTime, ref nextBossAt))
            SpawnEnemy(runProfile.bossTier, isBoss: true);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            EnemyTier tier = RunDifficultyEvaluator.GetEnemyTier(runProfile, elapsedTime);
            SpawnEnemy(tier, isBoss: false);
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy(EnemyTier tier, bool isBoss)
    {
        Enemy enemy = enemyPool.Acquire(tier);
        if (enemy == null)
            return;

        Vector2 baseOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 jitter = Random.insideUnitCircle * spawnJitter;

        Vector3 spawnPos = player.position + new Vector3(
            baseOffset.x + jitter.x,
            baseOffset.y + jitter.y,
            0f
        );

        EnemySpawnContext context = EnemySpawnContextBuilder.Build(
            runProfile,
            tierCatalog,
            elapsedTime,
            tier,
            isBoss);

        enemy.ConfigureSpawn(context, spawnPos);
    }
}
