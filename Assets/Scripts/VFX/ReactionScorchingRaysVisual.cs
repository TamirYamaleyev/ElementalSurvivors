using UnityEngine;

/// <summary>
/// BG3-style scorching rays: bursts of straight heat beams in random directions from the target.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionScorchingRaysVisual : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private float rayBurstInterval = 0.07f;
    [SerializeField] private int minRaysPerBurst = 5;
    [SerializeField] private int maxRaysPerBurst = 9;
    [SerializeField] private float minRayLength = 1.1f;
    [SerializeField] private float maxRayLength = 2.6f;
    [SerializeField] private float minRayLifetime = 0.14f;
    [SerializeField] private float maxRayLifetime = 0.30f;
    [SerializeField] private int sortingOrderOffset = 40;

    private Vector3 center;
    private Enemy sourceEnemy;
    private float burstTimer;
    private int sortingLayerId;
    private int sortingOrder;
    private bool initialized;

    public void Initialize(ReactionVfxContext ctx)
    {
        sourceEnemy = ctx.SourceEnemy;
        center = ctx.Center;
        CacheSortingFromEnemy(sourceEnemy);
        initialized = true;
        SpawnBurst();
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (sourceEnemy != null)
        {
            center = sourceEnemy.transform.position + Vector3.up * 0.25f;
            CacheSortingFromEnemy(sourceEnemy);
        }

        burstTimer += Time.deltaTime;
        if (burstTimer < rayBurstInterval)
            return;

        burstTimer = 0f;
        SpawnBurst();
    }

    private void SpawnBurst()
    {
        var rayCount = Random.Range(minRaysPerBurst, maxRaysPerBurst + 1);
        for (var i = 0; i < rayCount; i++)
        {
            var angle = Random.Range(0f, 360f);
            var length = Random.Range(minRayLength, maxRayLength);
            var lifetime = Random.Range(minRayLifetime, maxRayLifetime);

            var go = new GameObject("ScorchingRay");
            go.transform.SetParent(transform, false);

            var ray = go.AddComponent<ScorchingRayVisual>();
            ray.Initialize(center, angle, length, lifetime, sortingOrder, sortingLayerId);
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
