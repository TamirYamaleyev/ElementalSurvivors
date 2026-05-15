using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunDifficultyProfileSO runProfile;
    [SerializeField] private GameObject levelOneEnemyPrefab;
    [SerializeField] private GameObject levelTwoEnemyPrefab;
    [SerializeField] private GameObject levelThreeEnemyPrefab;
    [SerializeField] private GameObject bossEnemyPrefab;

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
        if (runProfile == null || player == null)
            return;

        elapsedTime += Time.deltaTime;

        if (runProfile.stopSpawningWhenSessionEnds &&
            elapsedTime >= runProfile.sessionDurationSeconds)
            return;

        if (RunDifficultyEvaluator.ShouldSpawnBoss(runProfile, elapsedTime, ref nextBossAt))
            SpawnEnemy(GetBossPrefab(), isBoss: true);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy(GetTierPrefab(), isBoss: false);
            timer = spawnInterval;
        }
    }

    private GameObject GetTierPrefab()
    {
        int index = RunDifficultyEvaluator.GetPrefabIndex(runProfile, elapsedTime);

        return index switch
        {
            1 => levelTwoEnemyPrefab,
            2 => levelThreeEnemyPrefab,
            _ => levelOneEnemyPrefab,
        };
    }

    private GameObject GetBossPrefab()
    {
        if (bossEnemyPrefab != null)
            return bossEnemyPrefab;

        return levelThreeEnemyPrefab != null ? levelThreeEnemyPrefab : levelOneEnemyPrefab;
    }

    private void SpawnEnemy(GameObject prefab, bool isBoss)
    {
        if (prefab == null)
            return;

        Vector2 baseOffset = Random.insideUnitCircle * spawnRadius;
        Vector2 jitter = Random.insideUnitCircle * spawnJitter;

        Vector3 spawnPos = player.position + new Vector3(
            baseOffset.x + jitter.x,
            baseOffset.y + jitter.y,
            0f
        );

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

        EnemyAI prefabAi = prefab.GetComponent<EnemyAI>();
        EnemyAI instanceAi = instance.GetComponent<EnemyAI>();

        if (prefabAi == null || instanceAi == null)
            return;

        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        if (isBoss)
            multiplier *= runProfile.bossExtraStatMultiplier;

        float scaledHp = prefabAi.BaselineMaxHealth * multiplier;
        float scaledDamage = prefabAi.BaselineContactDamage * multiplier;

        instanceAi.ApplyScaledStats(scaledHp, scaledDamage);

        if (isBoss && runProfile.bossVisualScale > 0f)
            instance.transform.localScale *= runProfile.bossVisualScale;
    }
}
