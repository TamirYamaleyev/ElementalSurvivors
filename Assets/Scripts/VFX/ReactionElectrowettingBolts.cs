using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns animated lightning bolts from the reaction center toward nearby enemies while the effect is active.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionElectrowettingBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float effectRadius = 2.4f;
    [SerializeField] private float boltInterval = 0.07f;
    [SerializeField] private int maxTargets = 6;
    [SerializeField] private float boltLifetime = 0.08f;
    [SerializeField] private Color boltTint = new(0.85f, 0.95f, 1f, 1f);

    private readonly List<Enemy> scratchTargets = new();

    private Vector3 center;
    private Enemy sourceEnemy;
    private EnemyRegistry registry;
    private float boltTimer;
    private bool initialized;

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        sourceEnemy = ctx.SourceEnemy;
        registry = ctx.Registry;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || lightningVisualPrefab == null || registry == null)
            return;

        boltTimer += Time.deltaTime;
        if (boltTimer < boltInterval)
            return;

        boltTimer = 0f;
        SpawnBolts();
    }

    private void SpawnBolts()
    {
        scratchTargets.Clear();
        var radiusSq = effectRadius * effectRadius;

        foreach (var enemy in registry.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            if (sourceEnemy != null && enemy == sourceEnemy)
                continue;

            var enemyPos = enemy.transform.position;
            var delta = enemyPos - center;
            if (delta.sqrMagnitude > radiusSq)
                continue;

            scratchTargets.Add(enemy);
            if (scratchTargets.Count >= maxTargets)
                break;
        }

        foreach (var target in scratchTargets)
        {
            var end = (Vector2)target.transform.position;
            var start = (Vector2)center;
            var visual = Instantiate(lightningVisualPrefab);
            visual.Initialize(start, end, null, boltLifetime);

            var sr = visual.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = boltTint;
        }
    }
}
