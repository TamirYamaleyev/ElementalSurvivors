using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyCharacterAnimation : MonoBehaviour, IEnemyPoolReset
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flipDeadzone = 0.02f;
    [SerializeField] private float attackAnimCooldown = 0.45f;
    [Tooltip("Enable when sprite art faces left by default (e.g. Enemy2_Walk).")]
    [SerializeField] private bool invertFacingFlip;

    float _nextAttackAnimTime;
    bool _deathStarted;
    Vector2 _lastPosition;
    bool _hasLastPosition;

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
        if (_deathStarted || animator == null)
            return;

        var current = (Vector2)transform.position;
        var speed = 0f;
        var deltaX = 0f;
        if (_hasLastPosition && Time.deltaTime > 1e-6f)
        {
            var delta = current - _lastPosition;
            speed = delta.magnitude / Time.deltaTime;
            deltaX = delta.x / Time.deltaTime;
        }

        _lastPosition = current;
        _hasLastPosition = true;

        animator.SetFloat(AnimationParams.Speed, speed);

        if (spriteRenderer != null)
            ApplyHorizontalFacing(deltaX);
    }

    public void ResetMotionSample()
    {
        _lastPosition = transform.position;
        _hasLastPosition = false;
    }

    public void ResetForPool()
    {
        StopAllCoroutines();
        _deathStarted = false;
        _nextAttackAnimTime = 0f;
        ResetMotionSample();

        if (animator != null)
        {
            animator.ResetTrigger(AnimationParams.Die);
            animator.ResetTrigger(AnimationParams.Attack);
            animator.SetFloat(AnimationParams.Speed, 0f);
        }
    }

    private void ApplyHorizontalFacing(float vx)
    {
        if (Mathf.Abs(vx) <= flipDeadzone)
        {
            var player = PlayerController.Instance;
            if (player != null)
                vx = player.transform.position.x - transform.position.x;
        }

        if (vx > flipDeadzone)
            spriteRenderer.flipX = invertFacingFlip;
        else if (vx < -flipDeadzone)
            spriteRenderer.flipX = !invertFacingFlip;
    }

    public void NotifyAttack()
    {
        if (_deathStarted || animator == null)
            return;
        if (Time.time < _nextAttackAnimTime)
            return;
        _nextAttackAnimTime = Time.time + attackAnimCooldown;
        animator.SetTrigger(AnimationParams.Attack);
    }

    /// <summary>Starts death clip and destroys the enemy root when the clip finishes (or after a safety timeout).</summary>
    public void BeginDeathSequence()
    {
        if (_deathStarted)
            return;
        _deathStarted = true;
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetTrigger(AnimationParams.Die);
            StartCoroutine(CoWaitDeathThenDestroy());
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    IEnumerator CoWaitDeathThenDestroy()
    {
        DisableGameplay();

        const float safetyTimeout = 4f;
        var t = 0f;
        while (t < safetyTimeout)
        {
            t += Time.deltaTime;
            var info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Death") && info.normalizedTime >= 0.99f)
                break;
            yield return null;
        }

        Destroy(transform.root.gameObject);
    }

    void DisableGameplay()
    {
        foreach (var ai in GetComponentsInChildren<EnemyAI>(true))
            ai.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider2D>(true))
            col.enabled = false;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
