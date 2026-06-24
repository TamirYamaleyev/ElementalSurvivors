using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small direct inward lightning segments along the pull axis (field edge toward each enemy).
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionMagnetismLightningBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float boltInterval = 0.06f;
    [SerializeField] private int segmentsPerEnemy = 3;
    [SerializeField] private float segmentLength = 0.2f;
    [SerializeField] private int maxSegmentsPerWave = 18;
    [SerializeField] private float boltLifetime = 0.07f;
    [SerializeField] private float endpointInset = 0.02f;
    [SerializeField] private float boltThickness = 0.28f;
    [SerializeField] private Color boltTint = new(0.72f, 0.95f, 1f, 1f);
    [SerializeField] private int sortingOrderOffset = 58;

    private readonly List<Enemy> scratchTargets = new();

    private Vector3 center;
    private EnemyRegistry registry;
    private ReactionMagneticFieldShrink fieldShrink;
    private int sortingLayerId;
    private int sortingOrder;
    private float boltTimer;
    private bool initialized;

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        registry = ctx.Registry;
        fieldShrink = GetComponent<ReactionMagneticFieldShrink>();
        CacheSortingFromEnemy(ctx.SourceEnemy);
        initialized = true;

        if (fieldShrink != null)
            SpawnPullBolts(fieldShrink.InitialFieldRadius);
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
        SpawnPullBolts(fieldShrink.CurrentFieldRadius);
    }

    private void SpawnPullBolts(float fieldRadius)
    {
        if (fieldRadius <= 0.01f)
            return;

        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, fieldRadius, scratchTargets);

        var spawned = 0;
        foreach (var target in scratchTargets)
        {
            if (target == null)
                continue;

            spawned += SpawnDirectPullSegments(target, fieldRadius, spawned);
            if (spawned >= maxSegmentsPerWave)
                return;
        }
    }

    private int SpawnDirectPullSegments(Enemy target, float fieldRadius, int spawnedSoFar)
    {
        var end = (Vector2)target.transform.position + Vector2.up * 0.25f;
        var delta = end - (Vector2)center;
        if (delta.sqrMagnitude < 1e-6f)
            return 0;

        var pullDir = delta.normalized;
        var ringPoint = (Vector2)center + pullDir * fieldRadius;
        var pullSpan = Vector2.Distance(ringPoint, end);
        if (pullSpan < 0.08f)
            return 0;

        var spawned = 0;
        var step = pullSpan / segmentsPerEnemy;
        var segLen = Mathf.Min(segmentLength, step * 0.85f);

        for (var i = 0; i < segmentsPerEnemy; i++)
        {
            if (spawnedSoFar + spawned >= maxSegmentsPerWave)
                break;

            var segStart = ringPoint + pullDir * (step * i);
            var segEnd = segStart + pullDir * segLen;
            SpawnBoltSegment(segStart, segEnd);
            spawned++;
        }

        return spawned;
    }

    private void SpawnBoltSegment(Vector2 start, Vector2 end)
    {
        var visual = Instantiate(lightningVisualPrefab, transform);
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
