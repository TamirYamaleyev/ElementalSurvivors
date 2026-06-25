using UnityEngine;

/// <summary>
/// Electrowetting: one lightning bolt from the source enemy to each target in range.
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
        var sourceEnemy = ctx.SourceEnemy;
        var registry = ctx.Registry;
        if (lightningVisualPrefab == null || registry == null)
            return;

        if (sourceEnemy != null && sourceEnemy.gameObject.activeInHierarchy)
            CacheSortingFromEnemy(sourceEnemy);

        var start = (Vector2)ctx.Center;
        var center = start;
        var radiusSq = effectRadius * effectRadius;

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
