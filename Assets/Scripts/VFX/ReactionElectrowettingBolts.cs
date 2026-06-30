using UnityEngine;

/// <summary>
/// Electrowetting: sequential chain lightning bolts along the gameplay jump path.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionElectrowettingBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private float effectRadius = 4f;
    [SerializeField] private float boltLifetime = 0.14f;
    [SerializeField] private float endpointInset = 0.05f;
    [SerializeField] private float boltThickness = 0.42f;
    [SerializeField] private Color boltTint = new(0.55f, 0.9f, 1f, 1f);
    [SerializeField] private int sortingOrderOffset = 54;

    private int sortingLayerId;
    private int sortingOrder;

    public void Initialize(ReactionVfxContext ctx)
    {
        if (lightningVisualPrefab == null)
            return;

        if (ctx.SourceEnemy != null && ctx.SourceEnemy.gameObject.activeInHierarchy)
            CacheSortingFromEnemy(ctx.SourceEnemy);

        var effect = GetComponentInParent<ReactionElectrowettingEffect>();
        if (effect != null && effect.ChainTargets.Count > 0)
        {
            SpawnChainBolts(ctx.Center, effect.ChainTargets);
            return;
        }

        SpawnRadialFallbackBolts(ctx);
    }

    private void SpawnChainBolts(Vector3 center, System.Collections.Generic.IReadOnlyList<Enemy> targets)
    {
        var previous = (Vector2)center;

        for (var i = 0; i < targets.Count; i++)
        {
            var enemy = targets[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            var end = (Vector2)enemy.transform.position + Vector2.up * 0.25f;
            SpawnBolt(previous, end);
            previous = end;
        }
    }

    private void SpawnRadialFallbackBolts(ReactionVfxContext ctx)
    {
        var registry = ctx.Registry;
        if (registry == null)
            return;

        var start = (Vector2)ctx.Center;
        var center = start;
        var radiusSq = effectRadius * effectRadius;
        var sourceEnemy = ctx.SourceEnemy;

        foreach (var enemy in registry.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy || enemy == sourceEnemy)
                continue;

            var delta = (Vector2)enemy.transform.position - center;
            if (delta.sqrMagnitude > radiusSq)
                continue;

            var end = (Vector2)enemy.transform.position + Vector2.up * 0.25f;
            SpawnBolt(start, end);
        }
    }

    private void SpawnBolt(Vector2 start, Vector2 end)
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
