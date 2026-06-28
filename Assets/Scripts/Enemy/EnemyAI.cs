using UnityEngine;

public enum DistanceBand
{
    TooFar,
    InRange,
    TooClose
}

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyCharacterAnimation characterAnimation;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Obstacle Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float bodyCastRadius = 0.24f;
    [SerializeField] private float probeDistance = 1f;
    [SerializeField] private float[] steerAnglesDeg = { 0f, -35f, 35f, -70f, 70f, -110f, 110f };

    private EnemyHealth health;
    private float slowMultiplier = 0.5f;
    private float slowTimer;
    private float dotDuration;
    private float dotTickTimer;
    private float dotDamage = 1f;
    private float dotTickInterval = 0.5f;
    private float fearTimer;
    private float stunTimer;
    private float hailStunImmunityTimer;
    private Vector2 externalVelocity;
    private float externalVelocityDecay = 12f;
    private Vector2 pullTarget;
    private float pullStrength;
    private float pullTimer;
    private Transform player;
    private Vector2 direction;
    private bool gameplayEnabled = true;
    private bool movementOverrideEnabled = true;
    private bool useDistanceMaintenance;
    private float preferredDistance;
    private float distanceTolerance;
    private Vector2 strafeDirection;
    private bool useStrafe;

    public bool IsGameplayEnabled => gameplayEnabled;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        health = GetComponent<EnemyHealth>();

        if (characterAnimation == null)
            characterAnimation = GetComponent<EnemyCharacterAnimation>();

        if (obstacleMask == 0)
            obstacleMask = LayerMask.GetMask("Obstacle");
    }

    public void EnsureInitialized()
    {
        if (player == null)
            player = PlayerController.Instance;
    }

    private void Update()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
                gameplayEnabled = true;
        }

        if (pullTimer > 0f)
            pullTimer -= Time.deltaTime;
        else
            pullStrength = 0f;

        if (hailStunImmunityTimer > 0f)
            hailStunImmunityTimer -= Time.deltaTime;

        if (dotDuration > 0f)
            DealDoT();
    }

    public void ApplyFear(float duration)
    {
        fearTimer = duration;
    }

    public void ApplySlow(float duration, float multiplier)
    {
        slowTimer = duration;
        slowMultiplier = multiplier;
    }

    public void ApplyDoT(float duration, float damagePerTick)
    {
        dotDuration = duration;
        dotDamage = damagePerTick;
        dotTickTimer = 0f;
    }

    public void ApplyStun(float duration)
    {
        if (duration <= 0f)
            return;

        stunTimer = Mathf.Max(stunTimer, duration);
        gameplayEnabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        externalVelocity = Vector2.zero;
    }

    public void ApplyKnockback(Vector2 direction, float impulse)
    {
        if (impulse <= 0f || direction.sqrMagnitude < 1e-6f || rb == null)
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

    public void SetMovementOverride(bool enabled)
    {
        movementOverrideEnabled = enabled;

        if (!enabled && rb != null && externalVelocity.sqrMagnitude < 1e-6f && pullTimer <= 0f)
            rb.linearVelocity = Vector2.zero;
    }

    public void SetDistanceMaintenance(float preferred, float tolerance, bool enabled)
    {
        useDistanceMaintenance = enabled;
        preferredDistance = preferred;
        distanceTolerance = Mathf.Max(0.05f, tolerance);
    }

    public void SetStrafeDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 1e-6f)
        {
            useStrafe = false;
            return;
        }

        useStrafe = true;
        strafeDirection = dir.normalized;
    }

    public DistanceBand EvaluateDistanceBand()
    {
        EnsureInitialized();
        if (player == null || rb == null)
            return DistanceBand.InRange;

        var dist = Vector2.Distance(rb.position, player.position);
        if (dist > preferredDistance + distanceTolerance)
            return DistanceBand.TooFar;
        if (dist < preferredDistance - distanceTolerance)
            return DistanceBand.TooClose;

        return DistanceBand.InRange;
    }

    public void AddHailStunImmunity(float duration)
    {
        if (duration <= 0f)
            return;

        hailStunImmunityTimer = Mathf.Min(
            hailStunImmunityTimer + duration,
            10f);
    }

    private void DealDoT()
    {
        dotDuration -= Time.deltaTime;
        dotTickTimer += Time.deltaTime;

        if (dotTickTimer >= dotTickInterval)
        {
            health?.TakeDamage(dotDamage);
            dotTickTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (externalVelocity.sqrMagnitude > 1e-6f)
        {
            rb.linearVelocity = externalVelocity;
            externalVelocity = Vector2.Lerp(
                externalVelocity,
                Vector2.zero,
                externalVelocityDecay * Time.fixedDeltaTime);
            return;
        }

        if (pullTimer > 0f && pullStrength > 0f)
        {
            var delta = pullTarget - rb.position;
            if (delta.sqrMagnitude > 1e-6f)
                rb.linearVelocity = delta.normalized * pullStrength;
            return;
        }

        if (!gameplayEnabled)
            return;

        if (useDistanceMaintenance && movementOverrideEnabled)
        {
            MaintainDistanceMovement();
            return;
        }

        if (!movementOverrideEnabled)
        {
            if (rb.linearVelocity.sqrMagnitude > 1e-6f)
                rb.linearVelocity = Vector2.zero;
            return;
        }

        FollowPlayer();
    }

    private void MaintainDistanceMovement()
    {
        EnsureInitialized();
        if (player == null)
            return;

        var band = EvaluateDistanceBand();
        Vector2 moveDir;

        if (band == DistanceBand.InRange && useStrafe)
        {
            moveDir = strafeDirection;
        }
        else if (band == DistanceBand.TooFar)
        {
            moveDir = ((Vector2)player.position - rb.position).normalized;
        }
        else if (band == DistanceBand.TooClose)
        {
            moveDir = (rb.position - (Vector2)player.position).normalized;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        MoveInDirection(moveDir);
    }

    private void MoveInDirection(Vector2 moveDir)
    {
        if (moveDir.sqrMagnitude < 1e-6f || rb == null)
            return;

        var currentSpeed = moveSpeed;
        if (slowTimer > 0f)
        {
            currentSpeed *= slowMultiplier;
            slowTimer -= Time.fixedDeltaTime;
        }

        if (Physics2D.OverlapCircle(rb.position, bodyCastRadius, obstacleMask))
            EnemyObstacleSteering.SeparateFromObstacles(rb, bodyCastRadius, obstacleMask);

        var steerDir = EnemyObstacleSteering.ResolveSteerDirection(
            rb.position,
            bodyCastRadius,
            moveDir,
            probeDistance,
            obstacleMask,
            steerAnglesDeg);

        EnemyObstacleSteering.MoveWithCollision(rb, bodyCastRadius, steerDir, currentSpeed, obstacleMask);
    }

    private void SetDirection()
    {
        if (player == null)
            return;

        Vector2 baseDir = (player.position - transform.position).normalized;

        if (fearTimer > 0f)
        {
            direction = -baseDir;
            fearTimer -= Time.fixedDeltaTime;
        }
        else
        {
            direction = baseDir;
        }
    }

    private void FollowPlayer()
    {
        SetDirection();

        if (direction.sqrMagnitude < 1e-6f || rb == null)
            return;

        MoveInDirection(direction);
    }

    public void ResetState()
    {
        slowTimer = 0f;
        fearTimer = 0f;
        stunTimer = 0f;
        hailStunImmunityTimer = 0f;
        dotDuration = 0f;
        dotTickTimer = 0f;
        externalVelocity = Vector2.zero;
        pullStrength = 0f;
        pullTimer = 0f;
        gameplayEnabled = true;
        movementOverrideEnabled = true;
        useDistanceMaintenance = false;
        useStrafe = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void SetGameplayEnabled(bool enabled)
    {
        gameplayEnabled = enabled;

        if (!enabled && rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
