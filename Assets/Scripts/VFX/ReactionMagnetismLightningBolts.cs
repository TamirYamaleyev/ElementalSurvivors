using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns lightning bolts from the contracting magnetic field edge toward enemies inside the field.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionMagnetismLightningBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float boltInterval = 0.1f;
    [SerializeField] private int maxTargetsPerWave = 6;
    [SerializeField] private float boltLifetime = 0.1f;
    [SerializeField] private Color boltTint = new(0.75f, 0.95f, 1f, 1f);

    private readonly List<Enemy> scratchTargets = new();

    private Vector3 center;
    private Enemy sourceEnemy;
    private EnemyRegistry registry;
    private ReactionMagneticFieldShrink fieldShrink;
    private float boltTimer;
    private bool initialized;

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        sourceEnemy = ctx.SourceEnemy;
        registry = ctx.Registry;
        fieldShrink = GetComponent<ReactionMagneticFieldShrink>();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || lightningVisualPrefab == null || registry == null || fieldShrink == null)
            return;

        if (!fieldShrink.IsActive)
            return;

        boltTimer += Time.deltaTime;
        if (boltTimer < boltInterval)
            return;

        boltTimer = 0f;
        SpawnBolts();
    }

    private void SpawnBolts()
    {
        var currentRadius = fieldShrink.CurrentFieldRadius;
        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, currentRadius, scratchTargets, sourceEnemy);

        var count = 0;
        foreach (var target in scratchTargets)
        {
            if (count >= maxTargetsPerWave)
                break;

            var enemyPos = (Vector2)target.transform.position;
            var delta = enemyPos - (Vector2)center;
            if (delta.sqrMagnitude < 1e-6f)
                continue;

            var dir = delta.normalized;
            var start = (Vector2)center + dir * currentRadius;
            var end = enemyPos;

            var visual = Instantiate(lightningVisualPrefab);
            visual.Initialize(start, end, null, boltLifetime);

            var sr = visual.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = boltTint;

            count++;
        }
    }
}
