using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject levelOneEnemyPrefab;
    [SerializeField] private GameObject levelTwoEnemyPrefab;
    [SerializeField] private GameObject levelThreeEnemyPrefab;

    [Header("Settings")]
    [SerializeField] private float startSpawnInterval = 1f;
    [SerializeField] private float minSpawnInterval = 0.2f;
    [SerializeField] private float rampSpeed = 0.05f;

    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private float spawnJitter = 0.5f;

    [SerializeField] private int amountToSpawnAtHalfwayPoint = 3;
    [SerializeField] private int amountToSpawnAtMax = 5;

    private float currentInterval;
    private float timer;
    private float elapsedTime;

    private Transform player;

    void Start()
    {
        player = PlayerController.Instance;

        currentInterval = startSpawnInterval;
        timer = currentInterval;
    }

    private GameObject GetEnemyPrefab(float t)
    {
        if (t >= 0.9f)
            return levelThreeEnemyPrefab;

        if (t >= 0.5f)
            return levelTwoEnemyPrefab;

        return levelOneEnemyPrefab;
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;

        float t = 1f - Mathf.Exp(-rampSpeed * elapsedTime);

        currentInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, t);

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            int spawnCount = GetSpawnCount(t);
            SpawnEnemies(spawnCount, t);

            timer = currentInterval;
        }
    }

    private int GetSpawnCount(float t)
    {
        if (t >= 0.9f) return amountToSpawnAtMax;
        if (t >= 0.5f) return amountToSpawnAtHalfwayPoint;
        return 1;
    }

    void SpawnEnemies(int count, float t)
    {
        GameObject prefab = GetEnemyPrefab(t);

        for (int i = 0; i < count; i++)
        {
            Vector2 baseOffset = Random.insideUnitCircle * spawnRadius;
            Vector2 jitter = Random.insideUnitCircle * spawnJitter;

            Vector3 spawnPos = player.position + new Vector3(
                baseOffset.x + jitter.x,
                baseOffset.y + jitter.y,
                0f
            );

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}