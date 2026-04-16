using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnRadius = 8f;

    private float timer;
    private Transform player;

    void Start()
    {
        timer = spawnInterval;
        player = PlayerController.Instance;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        Vector2 randomOffset = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnpos = player.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

        Instantiate(enemyPrefab, spawnpos, Quaternion.identity);
    }
}
