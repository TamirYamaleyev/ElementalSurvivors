using UnityEngine;

/// <summary>
/// Spawns five enemies in a row and applies one <see cref="StatusType"/> to each so elemental status VFX can be reviewed.
/// Optionally spawns a scaled "boss" test enemy with <see cref="ElementalParticleBootstrap"/> boss cone VFX as a child.
/// </summary>
public class ElementalVfxShowcaseBootstrap : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float horizontalSpacing = 2.2f;
    [SerializeField] private float statusDurationSeconds = 9999f;

    [Header("Boss VFX test")]
    [SerializeField] private GameObject bossVfxPrefab;
    [SerializeField] private Vector3 bossSpawnOffset = new(6.5f, 0f, 0f);
    [SerializeField] private float bossScale = 1.5f;
    [SerializeField] private Vector3 bossVfxLocalOffset = new(0f, -0.15f, 0f);

    private void Start()
    {
        var status = FindAnyObjectByType<StatusSystem>();
        var registry = FindAnyObjectByType<EnemyRegistry>();
        if (enemyPrefab == null || status == null)
        {
            Debug.LogWarning("[ElementalVfxShowcase] Assign enemy prefab; scene needs StatusSystem.");
            return;
        }

        var values = (StatusType[])System.Enum.GetValues(typeof(StatusType));
        var origin = transform.position;
        for (var i = 0; i < values.Length && i < 5; i++)
        {
            var pos = origin + new Vector3((i - 2) * horizontalSpacing, 0f, 0f);
            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.ConfigureSystems(status, registry);
            enemy.Initialize(status, registry);
            status.Apply(enemy, values[i], statusDurationSeconds);
        }

        if (bossVfxPrefab == null)
            return;

        var bossPos = origin + bossSpawnOffset;
        var boss = Instantiate(enemyPrefab, bossPos, Quaternion.identity);
        boss.transform.localScale *= bossScale;
        boss.ConfigureSystems(status, registry);
        boss.Initialize(status, registry);

        var vfx = Instantiate(bossVfxPrefab, boss.transform);
        vfx.transform.SetLocalPositionAndRotation(bossVfxLocalOffset, Quaternion.identity);

        foreach (var ps in vfx.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }
}
