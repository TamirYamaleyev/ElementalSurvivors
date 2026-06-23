using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Short lightning bolts from the shrinking field edge pulled into enemies inside the field.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionMagnetismLightningBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float boltInterval = 0.07f;
    [SerializeField] private int boltsPerEnemy = 2;
    [SerializeField] private int maxBoltsPerWave = 14;
    [SerializeField] private float boltLifetime = 0.09f;
    [SerializeField] private float endpointInset = 0.04f;
    [SerializeField] private float boltThickness = 0.42f;
    [SerializeField] private Color boltTint = new(0.7f, 0.94f, 1f, 1f);
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

            for (var i = 0; i < boltsPerEnemy; i++)
            {
                if (spawned >= maxBoltsPerWave)
                    return;

                SpawnBoltToEnemy(target, fieldRadius);
                spawned++;
            }
        }
    }

    private void SpawnBoltToEnemy(Enemy target, float fieldRadius)
    {
        var end = (Vector2)target.transform.position + Vector2.up * 0.25f;
        var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        var ringDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        var start = (Vector2)center + ringDir * fieldRadius;

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
