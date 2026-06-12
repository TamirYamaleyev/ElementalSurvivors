using UnityEngine;

/// <summary>
/// Spawns one enemy per reaction pair and applies both statuses so <see cref="StatusSystem"/>
/// triggers reaction burst VFX from <see cref="ReactionVfxCatalogSO"/>.
/// </summary>
public sealed class ReactionVfxShowcaseBootstrap : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab = null!;
    [SerializeField] private float horizontalSpacing = 2f;
    [SerializeField] private float statusDurationSeconds = 9999f;

    private static readonly (StatusType A, StatusType B)[] Pairs =
    {
        (StatusType.Fire, StatusType.Water),
        (StatusType.Fire, StatusType.Earth),
        (StatusType.Fire, StatusType.Wind),
        (StatusType.Fire, StatusType.Lightning),
        (StatusType.Water, StatusType.Wind),
        (StatusType.Water, StatusType.Earth),
        (StatusType.Water, StatusType.Lightning),
        (StatusType.Wind, StatusType.Earth),
        (StatusType.Wind, StatusType.Lightning),
        (StatusType.Earth, StatusType.Lightning),
    };

    private void Start()
    {
        var status = FindAnyObjectByType<StatusSystem>();
        var registry = FindAnyObjectByType<EnemyRegistry>();
        if (enemyPrefab == null || status == null)
        {
            Debug.LogWarning("[ReactionVfxShowcase] Assign enemy prefab; scene needs StatusSystem.");
            return;
        }

        if (registry == null)
        {
            Debug.LogWarning("[ReactionVfxShowcase] EnemyRegistry not found in scene.", this);
            return;
        }

        var origin = transform.position;
        var n = Pairs.Length;
        var totalWidth = (n - 1) * horizontalSpacing;

        for (var i = 0; i < n; i++)
        {
            var x = -totalWidth * 0.5f + i * horizontalSpacing;
            var pos = origin + new Vector3(x, 0f, 0f);
            var enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            enemy.ConfigureSystems(status, registry);
            enemy.Initialize(status, registry);

            var (a, b) = Pairs[i];
            status.Apply(enemy, a, statusDurationSeconds);
            status.Apply(enemy, b, statusDurationSeconds);
        }
    }
}
