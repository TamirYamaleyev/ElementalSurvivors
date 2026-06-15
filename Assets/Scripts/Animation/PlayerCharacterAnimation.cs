using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flipDeadzone = 0.02f;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    void LateUpdate()
    {
        if (animator == null || rb == null)
            return;

        var speed = rb.linearVelocity.magnitude;
        animator.SetFloat(AnimationParams.Speed, speed);

        if (spriteRenderer != null)
        {
            var vx = rb.linearVelocity.x;
            if (vx > flipDeadzone)
                spriteRenderer.flipX = false;
            else if (vx < -flipDeadzone)
                spriteRenderer.flipX = true;
        }
    }

    /// <summary>Called once per weapon volley (not per thrust in a combo).</summary>
    public void NotifyAttack()
    {
        if (animator == null)
            return;
        animator.SetTrigger(AnimationParams.Attack);
    }
}
