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

    private readonly EnemyCrowdControlState crowdControl = new();

    // Legacy DoT path used by Legacy/Weapons/OrbitOrb until legacy weapons are removed.
    private EnemyHealth health;
    private float dotDuration;
    private float dotTickTimer;
    private float dotDamage = 1f;
    private readonly float dotTickInterval = 0.5f;

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

    [SerializeField] private float knockbackDecay = 3f;

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

    //public void ApplyKnockback(Vector2 direction, float force)
    //{
    //    knockbackVelocity = direction.normalized * force;
    //}

    //public Vector2 ConsumeKnockback()
    //{
    //    Vector2 current = knockbackVelocity;

    //    knockbackVelocity = Vector2.Lerp(knockbackVelocity, Vector2.zero, knockbackDecay * Time.deltaTime);

    //    return current;
    //}

    public void SetPlayerTarget(Transform playerTransform)
    {
        player = playerTransform;
    }

    public void EnsureInitialized()
    {
        if (player == null)
            player = PlayerController.Instance;
    }

    private void Update()
    {
        var wasStunned = crowdControl.IsStunned;
        crowdControl.TickStun(Time.deltaTime);
        if (wasStunned && !crowdControl.IsStunned)
            gameplayEnabled = true;

        crowdControl.TickPull(Time.deltaTime);
        crowdControl.TickHailImmunity(Time.deltaTime);

        if (dotDuration > 0f)
            DealLegacyDoT();
    }

    public void ApplyFear(float duration) => crowdControl.ApplyFear(duration);

    public void ApplySlow(float duration, float multiplier) => crowdControl.ApplySlow(duration, multiplier);

    public void ApplyDoT(float duration, float damagePerTick)
    {
        dotDuration = duration;
        dotDamage = damagePerTick;
        dotTickTimer = 0f;
    }

    public void ApplyStun(float duration)
    {
        crowdControl.ApplyStun(duration);
        gameplayEnabled = false;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    public void ApplyKnockback(Vector2 knockbackDirection, float impulse) =>
        crowdControl.ApplyKnockback(knockbackDirection, impulse);

    public void ApplyPullToward(Vector2 target, float strength, float duration) =>
        crowdControl.ApplyPullToward(target, strength, duration);

    public bool CanBeStunnedByHail() => crowdControl.CanBeStunnedByHail();

    public void SetMovementOverride(bool enabled)
    {
        movementOverrideEnabled = enabled;

        if (!enabled && rb != null && !crowdControl.HasExternalVelocity && !crowdControl.IsPulled)
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

    public void AddHailStunImmunity(float duration) => crowdControl.AddHailStunImmunity(duration);

    private void DealLegacyDoT()
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

        if (crowdControl.TryApplyExternalVelocity(rb, Time.fixedDeltaTime))
            return;

        if (crowdControl.TryApplyPullVelocity(rb))
            return;

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

        var currentSpeed = crowdControl.ApplySlowToSpeed(moveSpeed, Time.fixedDeltaTime);

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
        direction = crowdControl.ResolveFollowDirection(baseDir, Time.fixedDeltaTime);
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
        crowdControl.Reset();
        dotDuration = 0f;
        dotTickTimer = 0f;
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
