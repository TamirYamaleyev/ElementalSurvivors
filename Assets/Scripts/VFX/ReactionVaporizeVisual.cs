using UnityEngine;

/// <summary>
/// Keeps vaporize steam at the reaction center and sorted above the source sprite.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionVaporizeVisual : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private int sortingOrderOffset = 25;

    public void Initialize(ReactionVfxContext ctx)
    {
        transform.position = ctx.Center;
        if (ctx.SourceEnemy != null)
            ReactionVfxSortingUtility.ApplyAboveEnemy(gameObject, ctx.SourceEnemy, sortingOrderOffset);
    }
}
