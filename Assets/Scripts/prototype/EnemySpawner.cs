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

    [Header("Obstacle Clearance")]
    [SerializeField] private float spawnBodyRadius = 0.35f;
    [SerializeField] private float spawnClearancePadding = 0.08f;
    [SerializeField] private int spawnClearMaxAttempts = 32;
    [SerializeField] private float spawnSearchRadiusExtra = 3f;

    private LayerMask obstacleMask;
    private float timer;
    private float elapsedTime;
    private float nextBossAt;

    private Transform player;

    private void Awake()
    {
        obstacleMask = LayerMask.GetMask("Obstacle");
    }

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
            SpawnEnemy(0, isBoss: true);
            return;
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

        Enemy instance = isBoss ? enemyPool.AcquireBoss() : enemyPool.Acquire(tierIndex);
        if (instance == null)
            return;

        instance.ConfigureSystems(statusSystem, enemyRegistry);

        Vector2 baseOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 jitter = Random.insideUnitCircle * spawnJitter;

        Vector3 desiredSpawn = player.position + new Vector3(
            baseOffset.x + jitter.x,
            baseOffset.y + jitter.y,
            0f
        );

        float visualScale = 1f;
        if (isBoss && runProfile.bossVisualScale > 0f)
            visualScale = runProfile.bossVisualScale;

        float checkRadius = spawnBodyRadius + spawnClearancePadding;
        if (visualScale > 0f)
            checkRadius = spawnBodyRadius * visualScale + spawnClearancePadding;

        Vector3 spawnPos = FindClearSpawnPosition(desiredSpawn, checkRadius);
        if (IsSpawnBlocked(spawnPos, checkRadius))
        {
            enemyPool.Release(instance);
            return;
        }

        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        if (isBoss)
            multiplier *= runProfile.bossExtraStatMultiplier;

        instance.OnAcquire(new SpawnContext
        {
            Position = spawnPos,
            ScaledMaxHealth = prefabRef.BaselineMaxHealth * multiplier,
            ScaledContactDamage = prefabRef.BaselineContactDamage * multiplier,
            VisualScaleMultiplier = visualScale,
        });

        if (isBoss)
            EnemyWorldHealthBar.EnsureAttached(instance);
    }

    private Vector3 FindClearSpawnPosition(Vector3 desired, float checkRadius)
    {
        if (!IsSpawnBlocked(desired, checkRadius))
            return desired;

        for (int i = 0; i < spawnClearMaxAttempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            Vector2 jitter = Random.insideUnitCircle * spawnJitter;
            Vector3 candidate = player.position + new Vector3(offset.x + jitter.x, offset.y + jitter.y, 0f);
            if (!IsSpawnBlocked(candidate, checkRadius))
                return candidate;
        }

        float maxRadius = spawnRadius + spawnJitter + spawnSearchRadiusExtra;
        const int ringCount = 5;
        const int anglesPerRing = 12;
        for (int ring = 1; ring <= ringCount; ring++)
        {
            float radius = Mathf.Lerp(spawnRadius * 0.5f, maxRadius, ring / (float)ringCount);
            for (int a = 0; a < anglesPerRing; a++)
            {
                float angle = a * (360f / anglesPerRing) * Mathf.Deg2Rad;
                Vector3 candidate = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius;
                if (!IsSpawnBlocked(candidate, checkRadius))
                    return candidate;
            }
        }

        return desired;
    }

    private bool IsSpawnBlocked(Vector3 position, float checkRadius)
    {
        if (obstacleMask == 0)
            return false;

        return Physics2D.OverlapCircle(position, checkRadius, obstacleMask) != null;
    }
}
