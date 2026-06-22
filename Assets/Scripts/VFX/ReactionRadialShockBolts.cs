using UnityEngine;

/// <summary>
/// Radial lightning shock burst from the reaction center (electrowetting-style).
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionRadialShockBolts : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private ChaingLightningVisual lightningVisualPrefab;
    [SerializeField] private int boltCount = 11;
    [SerializeField] private float minBoltLength = 0.75f;
    [SerializeField] private float maxBoltLength = 1.25f;
    [SerializeField] private float originRadius = 0.08f;
    [SerializeField] private float boltLifetime = 0.14f;
    [SerializeField] private float endpointInset = 0.06f;
    [SerializeField] private float boltThickness = 0.55f;
    [SerializeField] private Color boltTint = new(0.45f, 0.88f, 1f, 1f);
    [SerializeField] private int sortingOrderOffset = 52;

    public void Initialize(ReactionVfxContext ctx)
    {
        if (lightningVisualPrefab == null)
            return;

        var center = (Vector2)ctx.Center;
        var sortingLayerId = 0;
        var sortingOrder = sortingOrderOffset;

        if (ctx.SourceEnemy != null)
        {
            var body = ctx.SourceEnemy.GetComponentInChildren<SpriteRenderer>();
            if (body != null)
            {
                sortingLayerId = body.sortingLayerID;
                sortingOrder = body.sortingOrder + sortingOrderOffset;
            }
        }

        for (var i = 0; i < boltCount; i++)
        {
            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var length = Random.Range(minBoltLength, maxBoltLength);
            var start = center + dir * originRadius;
            var end = center + dir * length;

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
    }
}
