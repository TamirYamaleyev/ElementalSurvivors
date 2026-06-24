using System.Collections;
using UnityEngine;

/// <summary>
/// Spawns five enemies in a row and applies one <see cref="StatusType"/> to each so elemental status VFX can be reviewed.
/// Optionally spawns a scaled "boss" test enemy with <see cref="ElementalParticleBootstrap"/> boss cone VFX as a child.
/// </summary>
public class ElementalVfxShowcaseBootstrap : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float horizontalSpacing = 2.2f;
    [SerializeField] private float spawnHeightY = 1.5f;
    [SerializeField] private float statusDurationSeconds = 9999f;

    [Header("Boss VFX test")]
    [SerializeField] private GameObject bossVfxPrefab;
    [SerializeField] private Vector3 bossSpawnOffset = new(6.5f, 0f, 0f);
    [SerializeField] private float bossScale = 1.5f;
    [SerializeField] private Vector3 bossVfxLocalOffset = new(0f, -0.15f, 0f);

    private static readonly StatusType[] ShowcaseElements =
    {
        StatusType.Fire,
        StatusType.Water,
        StatusType.Wind,
        StatusType.Earth,
        StatusType.Lightning,
    };

    private void Start()
    {
        var status = FindAnyObjectByType<StatusSystem>();
        var registry = FindAnyObjectByType<EnemyRegistry>();
        if (enemyPrefab == null || status == null)
        {
            Debug.LogWarning("[ElementalVfxShowcase] Assign enemy prefab; scene needs StatusSystem.");
            return;
        }

        DisableSceneInterruptions();
        TmpFontUtility.EnsureAllInScene();
        HideGameplayUi();

        StartCoroutine(SpawnShowcaseEnemies(status, registry));
    }

    private IEnumerator SpawnShowcaseEnemies(StatusSystem status, EnemyRegistry registry)
    {
        yield return null;

        var origin = transform.position;
        origin.y = spawnHeightY;

        for (var i = 0; i < ShowcaseElements.Length; i++)
        {
            var element = ShowcaseElements[i];
            var x = (i - (ShowcaseElements.Length - 1) * 0.5f) * horizontalSpacing;
            var pos = origin + new Vector3(x, 0f, 0f);

            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.ConfigureSystems(status, registry);
            enemy.Initialize(status, registry);
            PrepareShowcaseEnemy(enemy, registry);

            ElementShowcaseLabel.Create(enemy.transform, element.ToString());
            status.Apply(enemy, element, statusDurationSeconds);

            yield return null;
            RefreshStatusParticles(enemy);
        }

        if (bossVfxPrefab == null)
            yield break;

        var bossPos = origin + bossSpawnOffset;
        var boss = Instantiate(enemyPrefab, bossPos, Quaternion.identity);
        boss.transform.localScale *= bossScale;
        boss.ConfigureSystems(status, registry);
        boss.Initialize(status, registry);
        PrepareShowcaseEnemy(boss, registry);

        var vfx = Instantiate(bossVfxPrefab, boss.transform, worldPositionStays: false);
        vfx.transform.SetLocalPositionAndRotation(bossVfxLocalOffset, Quaternion.identity);

        yield return null;
        RefreshParticleSystems(vfx);
    }

    private static void RefreshStatusParticles(Enemy enemy)
    {
        if (enemy == null)
            return;

        RefreshParticleSystems(enemy.gameObject);
    }

    private static void RefreshParticleSystems(GameObject root)
    {
        foreach (var ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    private static void HideGameplayUi()
    {
        var hud = GameObject.Find("HUD");
        if (hud != null)
            hud.SetActive(false);

        var levelUpRoot = GameObject.Find("LevelUpUIRoot");
        if (levelUpRoot != null)
            levelUpRoot.SetActive(false);
    }

    private static void DisableSceneInterruptions()
    {
        var spawner = FindAnyObjectByType<EnemySpawner>();
        if (spawner != null)
            spawner.gameObject.SetActive(false);

        var pool = FindAnyObjectByType<EnemyPool>();
        if (pool != null)
            pool.gameObject.SetActive(false);

        var weaponSystem = FindAnyObjectByType<WeaponSystem>();
        if (weaponSystem != null)
            weaponSystem.enabled = false;
    }

    private static void PrepareShowcaseEnemy(Enemy enemy, EnemyRegistry registry)
    {
        var ai = enemy.GetComponent<EnemyAI>();
        if (ai != null)
            ai.SetGameplayEnabled(false);

        if (registry != null)
            registry.Register(enemy);
    }
}
