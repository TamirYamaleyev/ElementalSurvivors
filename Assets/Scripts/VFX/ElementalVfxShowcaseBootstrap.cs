using UnityEngine;

/// <summary>
/// Spawns five enemies in a row and applies one <see cref="StatusType"/> to each so elemental status VFX can be reviewed.
/// </summary>
public class ElementalVfxShowcaseBootstrap : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private float horizontalSpacing = 2.2f;
    [SerializeField] private float statusDurationSeconds = 9999f;

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
    }
}
