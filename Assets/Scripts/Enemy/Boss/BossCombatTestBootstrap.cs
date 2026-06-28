using System.Collections;
using UnityEngine;

public sealed class BossCombatTestBootstrap : MonoBehaviour
{
    [SerializeField] private Enemy bossPrefab;
    [SerializeField] private Vector3 spawnOffset = new(0f, 8f, 0f);
    [SerializeField] private float bossVisualScale = 1.5f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float bossMaxHealth = 500f;

    private StatusSystem statusSystem;
    private EnemyRegistry registry;
    private Enemy activeBoss;

    private void Start()
    {
        statusSystem = FindAnyObjectByType<StatusSystem>();
        registry = FindAnyObjectByType<EnemyRegistry>();

        DisableSceneInterruptions();
        TmpFontUtility.EnsureAllInScene();

        SpawnBoss();
    }

    private void DisableSceneInterruptions()
    {
        var spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.enabled = false;
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("[BossCombatTest] Assign boss prefab.", this);
            return;
        }

        var player = PlayerController.Instance;
        var origin = player != null ? player.transform.position : Vector3.zero;
        var pos = origin + spawnOffset;

        activeBoss = Instantiate(bossPrefab, pos, Quaternion.identity);
        if (statusSystem != null && registry != null)
        {
            activeBoss.ConfigureSystems(statusSystem, registry);
            activeBoss.Initialize(statusSystem, registry);
        }

        activeBoss.OnAcquire(new SpawnContext
        {
            Position = pos,
            ScaledMaxHealth = bossMaxHealth,
            ScaledContactDamage = 0f,
            VisualScaleMultiplier = bossVisualScale
        });

        var health = activeBoss.GetComponent<EnemyHealth>();
        if (health != null)
            health.OnDied += HandleBossDied;
    }

    private void HandleBossDied()
    {
        if (activeBoss != null)
        {
            var health = activeBoss.GetComponent<EnemyHealth>();
            if (health != null)
                health.OnDied -= HandleBossDied;
            activeBoss = null;
        }

        if (respawnDelay > 0f)
            StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnBoss();
    }
}
