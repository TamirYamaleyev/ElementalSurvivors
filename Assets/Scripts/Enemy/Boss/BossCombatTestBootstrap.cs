using System.Collections;
using UnityEngine;

public sealed class BossCombatTestBootstrap : MonoBehaviour
{
    [SerializeField] private Enemy bossPrefab;
    [SerializeField] private Transform spawnAnchor;
    [SerializeField] private GameObject decorativeBossRoot;
    [SerializeField] private Vector3 spawnOffset = new(0f, 8f, 0f);
    [SerializeField] private float bossVisualScale = 1.5f;
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private float bossMaxHealth = 500f;

    private StatusSystem statusSystem;
    private EnemyRegistry registry;
    private Enemy activeBoss;

    private void Start()
    {
        statusSystem = FindAnyObjectByType<StatusSystem>(FindObjectsInactive.Include);
        registry = FindAnyObjectByType<EnemyRegistry>(FindObjectsInactive.Include);

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

        if (registry == null)
        {
            Debug.LogWarning("[BossCombatTest] EnemyRegistry not found; boss will not be targetable.", this);
        }

        var player = PlayerController.Instance;
        var origin = player != null ? player.transform.position : Vector3.zero;
        var pos = spawnAnchor != null ? spawnAnchor.position : origin + spawnOffset;

        activeBoss = Instantiate(bossPrefab, pos, Quaternion.identity);

        activeBoss.ConfigureSystems(statusSystem, registry);

        if (statusSystem != null)
            activeBoss.Initialize(statusSystem, registry);

        activeBoss.OnAcquire(new SpawnContext
        {
            Position = pos,
            ScaledMaxHealth = bossMaxHealth,
            ScaledContactDamage = 0f,
            VisualScaleMultiplier = bossVisualScale
        });

        if (decorativeBossRoot != null)
            decorativeBossRoot.SetActive(false);

        EnemyWorldHealthBar.EnsureAttached(activeBoss);

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
