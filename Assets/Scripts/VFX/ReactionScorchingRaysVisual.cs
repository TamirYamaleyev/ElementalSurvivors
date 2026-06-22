using UnityEngine;

/// <summary>
/// Spawns a heat ring plus exactly three thick beams from the ring edge in random directions.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionScorchingRaysVisual : MonoBehaviour, IReactionWorldVfx
{
    private const int BeamCount = 3;

    [SerializeField] private float sphereRadius = 0.36f;
    [SerializeField] private float minRayLength = 0.85f;
    [SerializeField] private float maxRayLength = 1.35f;
    [SerializeField] private float effectLifetime = 0.32f;
    [SerializeField] private int sortingOrderOffset = 55;

    private int sortingLayerId;
    private int sortingOrder;

    public void Initialize(ReactionVfxContext ctx)
    {
        var center = (Vector2)ctx.Center;
        CacheSortingFromEnemy(ctx.SourceEnemy);

        var ringGo = new GameObject("ScorchingWindRing");
        ringGo.AddComponent<ScorchingWindRingVisual>()
            .Initialize(center, sphereRadius, effectLifetime, sortingOrder, sortingLayerId);

        for (var i = 0; i < BeamCount; i++)
        {
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var origin = center + dir * sphereRadius;
            var length = Random.Range(minRayLength, maxRayLength);

            var go = new GameObject("ScorchingRay");
            var ray = go.AddComponent<ScorchingRayVisual>();
            ray.Initialize(origin, dir, length, effectLifetime, sortingOrder, sortingLayerId);
        }
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
