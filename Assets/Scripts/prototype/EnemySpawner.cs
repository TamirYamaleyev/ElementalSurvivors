using UnityEngine;

public class EnemySpawner : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private RunDifficultyProfileSO runProfile;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private EnemyTierSetSO tierSet;
    [SerializeField] private StatusSystem statusSystem;
    [SerializeField] private EnemyRegistry enemyRegistry;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private float spawnJitter = 0.5f;

    private float timer;
    private float elapsedTime;
    private float nextBossAt;

    private Transform player;

    private void Start()
    {
        player = PlayerController.Instance;
        timer = spawnInterval;

        if (runProfile != null)
            nextBossAt = runProfile.firstBossDelaySeconds;
    }

    private void Update()
    {
        if (runProfile == null || enemyPool == null || tierSet == null || player == null)
            return;

        elapsedTime += Time.deltaTime;

        if (runProfile.stopSpawningWhenSessionEnds &&
            elapsedTime >= runProfile.sessionDurationSeconds)
            return;

        if (RunDifficultyEvaluator.ShouldSpawnBoss(runProfile, elapsedTime, ref nextBossAt))
        {
            int bossPoolTier = Mathf.Max(0, tierSet.tiers.Length - 1);
            SpawnEnemy(bossPoolTier, isBoss: true);
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            int tierIndex = RunDifficultyEvaluator.GetPrefabIndex(runProfile, elapsedTime);
            SpawnEnemy(tierIndex, isBoss: false);
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy(int tierIndex, bool isBoss)
    {
        Enemy prefabRef = isBoss ? tierSet.GetBossPrototype() : tierSet.GetTierPrototype(tierIndex);
        if (prefabRef == null)
            return;

        Enemy instance = enemyPool.Acquire(tierIndex);
        if (instance == null)
            return;

        instance.ConfigureSystems(statusSystem, enemyRegistry);

        Vector2 baseOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 jitter = Random.insideUnitCircle * spawnJitter;

        Vector3 spawnPos = player.position + new Vector3(
            baseOffset.x + jitter.x,
            baseOffset.y + jitter.y,
            0f
        );

        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        if (isBoss)
            multiplier *= runProfile.bossExtraStatMultiplier;

        float visualScale = 1f;
        if (isBoss && runProfile.bossVisualScale > 0f)
            visualScale = runProfile.bossVisualScale;

        instance.OnAcquire(new SpawnContext
        {
            Position = spawnPos,
            ScaledMaxHealth = prefabRef.BaselineMaxHealth * multiplier,
            ScaledContactDamage = prefabRef.BaselineContactDamage * multiplier,
            VisualScaleMultiplier = visualScale,
        });
    }
}
