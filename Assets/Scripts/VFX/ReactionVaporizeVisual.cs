using UnityEngine;

/// <summary>
/// Follows the source enemy and keeps vaporize steam sorted above the sprite.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionVaporizeVisual : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private int sortingOrderOffset = 25;

    private Enemy sourceEnemy;

    public void Initialize(ReactionVfxContext ctx)
    {
        sourceEnemy = ctx.SourceEnemy;
        transform.position = ctx.Center;
        RefreshSorting();
    }

    private void LateUpdate()
    {
        if (sourceEnemy == null)
            return;

        transform.position = sourceEnemy.transform.position + Vector3.up * 0.25f;
        RefreshSorting();
    }

    private void RefreshSorting()
    {
        if (sourceEnemy != null)
            ReactionVfxSortingUtility.ApplyAboveEnemy(gameObject, sourceEnemy, sortingOrderOffset);
    }
}
