using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns a single burst of lightning bolts from the reaction center toward nearby enemies.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionElectrowettingBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float effectRadius = 2.4f;
    [SerializeField] private int maxTargets = 6;
    [SerializeField] private float boltLifetime = 0.12f;
    [SerializeField] private Color boltTint = new(0.85f, 0.95f, 1f, 1f);

    private readonly List<Enemy> scratchTargets = new();

    private Vector3 center;
    private Enemy sourceEnemy;
    private EnemyRegistry registry;

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        sourceEnemy = ctx.SourceEnemy;
        registry = ctx.Registry;
        SpawnBolts();
    }

    private void SpawnBolts()
    {
        if (lightningVisualPrefab == null || registry == null)
            return;

        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, effectRadius, scratchTargets, sourceEnemy);

        var count = 0;
        foreach (var target in scratchTargets)
        {
            if (count >= maxTargets)
                break;

            var end = (Vector2)target.transform.position;
            var start = (Vector2)center;
            var visual = Instantiate(lightningVisualPrefab);
            visual.Initialize(start, end, null, boltLifetime);

            var sr = visual.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = boltTint;

            count++;
        }
    }
}
