using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float flipDeadzone = 0.02f;
    [SerializeField] private float attackAnimCooldown = 0.45f;

    float _nextAttackAnimTime;
    bool _deathStarted;

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
        if (_deathStarted || animator == null || rb == null)
            return;

        animator.SetFloat(AnimationParams.Speed, rb.linearVelocity.magnitude);

        if (spriteRenderer != null)
        {
            var vx = rb.linearVelocity.x;
            if (vx > flipDeadzone)
                spriteRenderer.flipX = false;
            else if (vx < -flipDeadzone)
                spriteRenderer.flipX = true;
        }
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
