using UnityEngine;

public sealed class EnemyCrowdControlState
{
    private float slowMultiplier = 0.5f;
    private float slowTimer;
    private float fearTimer;
    private float stunTimer;
    private float hailStunImmunityTimer;
    private Vector2 externalVelocity;
    private readonly float externalVelocityDecay = 12f;
    private Vector2 pullTarget;
    private float pullStrength;
    private float pullTimer;

    public bool IsStunned => stunTimer > 0f;
    public bool HasExternalVelocity => externalVelocity.sqrMagnitude > 1e-6f;
    public bool IsPulled => pullTimer > 0f && pullStrength > 0f;
    public bool HasSlow => slowTimer > 0f;
    public float SlowMultiplier => slowMultiplier;

    public void ApplyFear(float duration)
    {
        fearTimer = duration;
    }

    public void ApplySlow(float duration, float multiplier)
    {
        slowTimer = duration;
        slowMultiplier = multiplier;
    }

    public void ApplyStun(float duration)
    {
        if (duration <= 0f)
            return;

        stunTimer = Mathf.Max(stunTimer, duration);
        externalVelocity = Vector2.zero;
        pullTimer = 0f;
        pullStrength = 0f;
    }

    public void ApplyKnockback(Vector2 direction, float impulse)
    {
        if (impulse <= 0f || direction.sqrMagnitude < 1e-6f)
            return;

        pullTimer = 0f;
        pullStrength = 0f;
        pullTarget = Vector2.zero;
        externalVelocity = direction.normalized * impulse;
    }

    public void ApplyPullToward(Vector2 target, float strength, float duration)
    {
        if (strength <= 0f || duration <= 0f)
            return;

        pullTarget = target;
        pullStrength = strength;
        pullTimer = Mathf.Max(pullTimer, duration);
    }

    public bool CanBeStunnedByHail() => hailStunImmunityTimer <= 0f;

    public void AddHailStunImmunity(float duration)
    {
        if (duration <= 0f)
            return;

        hailStunImmunityTimer = Mathf.Min(hailStunImmunityTimer + duration, 10f);
    }

    public void TickStun(float deltaTime)
    {
        if (stunTimer > 0f)
            stunTimer -= deltaTime;
    }

    public void TickPull(float deltaTime)
    {
        if (pullTimer > 0f)
            pullTimer -= deltaTime;
        else
            pullStrength = 0f;
    }

    public void TickHailImmunity(float deltaTime)
    {
        if (hailStunImmunityTimer > 0f)
            hailStunImmunityTimer -= deltaTime;
    }

    public Vector2 ResolveFollowDirection(Vector2 toPlayer, float fixedDeltaTime)
    {
        if (fearTimer > 0f)
        {
            fearTimer -= fixedDeltaTime;
            return -toPlayer;
        }

        return toPlayer;
    }

    public float ApplySlowToSpeed(float baseSpeed, float fixedDeltaTime)
    {
        if (slowTimer <= 0f)
            return baseSpeed;

        slowTimer -= fixedDeltaTime;
        return baseSpeed * slowMultiplier;
    }

    public bool TryApplyExternalVelocity(Rigidbody2D rb, float fixedDeltaTime)
    {
        if (!HasExternalVelocity || rb == null)
            return false;

        rb.linearVelocity = externalVelocity;
        externalVelocity = Vector2.Lerp(
            externalVelocity,
            Vector2.zero,
            externalVelocityDecay * fixedDeltaTime);
        return true;
    }

    public bool TryApplyPullVelocity(Rigidbody2D rb)
    {
        if (!IsPulled || rb == null)
            return false;

        var delta = pullTarget - rb.position;
        if (delta.sqrMagnitude > 1e-6f)
            rb.linearVelocity = delta.normalized * pullStrength;
        return true;
    }

    public void ClearExternalMotion()
    {
        externalVelocity = Vector2.zero;
        pullStrength = 0f;
        pullTimer = 0f;
    }

    public void Reset()
    {
        slowTimer = 0f;
        fearTimer = 0f;
        stunTimer = 0f;
        hailStunImmunityTimer = 0f;
        externalVelocity = Vector2.zero;
        pullStrength = 0f;
        pullTimer = 0f;
    }
}
