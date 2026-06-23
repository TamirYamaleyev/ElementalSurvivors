using UnityEngine;

/// <summary>
/// Keeps hail reaction VFX centered on the source enemy and exposes fall-zone size tuning.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionHailVisual : MonoBehaviour, IReactionWorldVfx
{
    [Tooltip("Horizontal spawn width of the hail fall zone (not particle size).")]
    [SerializeField] private float fallAreaWidth = 2.2f;

    [Tooltip("Depth of the hail spawn box (not particle size).")]
    [SerializeField] private float fallAreaDepth = 2.2f;

    [Tooltip("Height of the thin spawn volume above the reaction center.")]
    [SerializeField] private float spawnVolumeHeight = 0.08f;

    [Tooltip("How far above the reaction center hail particles spawn.")]
    [SerializeField] private float spawnHeight = 0.9f;

    private Enemy sourceEnemy;

    public void Initialize(ReactionVfxContext ctx)
    {
        sourceEnemy = ctx.SourceEnemy;
        transform.position = ctx.Center;
        ApplyFallArea();
    }

    private void Update()
    {
        if (sourceEnemy == null)
            return;

        transform.position = sourceEnemy.transform.position + Vector3.up * 0.25f;
    }

    private void ApplyFallArea()
    {
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(fallAreaWidth, spawnVolumeHeight, fallAreaDepth);
            shape.position = new Vector3(0f, spawnHeight, 0f);
        }
    }
}
