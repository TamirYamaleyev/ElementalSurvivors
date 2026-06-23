using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Electrowetting lightning: radial pull into the affected enemy plus bolts to nearby targets.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionElectrowettingBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float pullRingRadius = 0.95f;
    [SerializeField] private int pullBoltsOnSource = 5;
    [SerializeField] private float effectRadius = 2.4f;
    [SerializeField] private int maxNearbyTargets = 4;
    [SerializeField] private float boltLifetime = 0.12f;
    [SerializeField] private float endpointInset = 0.04f;
    [SerializeField] private float boltThickness = 0.45f;
    [SerializeField] private Color boltTint = new(0.55f, 0.9f, 1f, 1f);
    [SerializeField] private int sortingOrderOffset = 54;

    private readonly List<Enemy> scratchTargets = new();

    private Vector3 center;
    private Enemy sourceEnemy;
    private EnemyRegistry registry;
    private int sortingLayerId;
    private int sortingOrder;

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        sourceEnemy = ctx.SourceEnemy;
        registry = ctx.Registry;
        CacheSortingFromEnemy(sourceEnemy);

        if (lightningVisualPrefab == null)
            return;

        if (sourceEnemy != null)
        {
            for (var i = 0; i < pullBoltsOnSource; i++)
                SpawnPullBolt((Vector2)sourceEnemy.transform.position + Vector2.up * 0.25f);
        }

        if (registry == null)
            return;

        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, effectRadius, scratchTargets, sourceEnemy);

        var count = 0;
        foreach (var target in scratchTargets)
        {
            if (count >= maxNearbyTargets)
                break;

            SpawnPullBolt((Vector2)target.transform.position + Vector2.up * 0.25f);
            count++;
        }
    }

    private void SpawnPullBolt(Vector2 end)
    {
        var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        var ringDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var start = (Vector2)center + ringDir * pullRingRadius;

        var visual = Instantiate(lightningVisualPrefab);
        visual.Initialize(start, end, null, boltLifetime, endpointInset);

        var sr = visual.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = boltTint;
            sr.sortingLayerID = sortingLayerId;
            sr.sortingOrder = sortingOrder;
        }

        visual.transform.localScale = new Vector3(
            boltThickness,
            visual.transform.localScale.y,
            1f);
    }

    private void CacheSortingFromEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        var body = enemy.GetComponentInChildren<SpriteRenderer>();
        if (body == null)
            return;

        sortingLayerId = body.sortingLayerID;
        sortingOrder = body.sortingOrder + sortingOrderOffset;
    }
}
