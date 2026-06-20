using UnityEngine;

/// <summary>
/// Keeps hail reaction VFX centered on the source enemy while ice pebbles fall.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionHailVisual : MonoBehaviour, IReactionWorldVfx
{
    private Enemy sourceEnemy;

    public void Initialize(ReactionVfxContext ctx)
    {
        sourceEnemy = ctx.SourceEnemy;
        transform.position = ctx.Center;
    }

    private void Update()
    {
        if (sourceEnemy == null)
            return;

        transform.position = sourceEnemy.transform.position + Vector3.up * 0.25f;
    }
}
