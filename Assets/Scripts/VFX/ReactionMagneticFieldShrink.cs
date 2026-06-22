using UnityEngine;

/// <summary>
/// Terraria-style magnetic field: appears large then shrinks inward (VFX only).
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionMagneticFieldShrink : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private float startScale = 2.2f;
    [SerializeField] private float endScale = 0.12f;
    [SerializeField] private float shrinkDuration = 1f;
    [SerializeField] private float fieldBaseRadius = 0.7f;

    private float elapsed;
    private bool active;

    public float StartScale => startScale;
    public float EndScale => endScale;
    public float ShrinkDuration => shrinkDuration;
    public bool IsActive => active;
    public float CurrentScale => transform.localScale.x;
    public float CurrentFieldRadius => fieldBaseRadius * CurrentScale;

    public void Initialize(ReactionVfxContext ctx)
    {
        transform.position = ctx.Center;
    }

    private void Start()
    {
        transform.localScale = Vector3.one * startScale;
        elapsed = 0f;
        active = true;
    }

    private void Update()
    {
        if (!active)
            return;

        elapsed += Time.deltaTime;
        var t = shrinkDuration > 0f ? Mathf.Clamp01(elapsed / shrinkDuration) : 1f;
        var eased = t * t;
        var scale = Mathf.Lerp(startScale, endScale, eased);
        transform.localScale = Vector3.one * scale;

        if (t >= 1f)
            active = false;
    }
}
