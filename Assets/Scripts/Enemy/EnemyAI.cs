using UnityEngine;

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
    private Transform player;
    private Vector2 direction;
    private bool gameplayEnabled = true;

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
        if (!gameplayEnabled || dotDuration <= 0f)
            return;

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
        if (!gameplayEnabled)
            return;

        FollowPlayer();
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

        float currentSpeed = moveSpeed;

        if (slowTimer > 0f)
        {
            currentSpeed *= slowMultiplier;
            slowTimer -= Time.fixedDeltaTime;
        }

        if (Physics2D.OverlapCircle(rb.position, bodyCastRadius, obstacleMask))
            EnemyObstacleSteering.SeparateFromObstacles(rb, bodyCastRadius, obstacleMask);

        Vector2 steerDir = EnemyObstacleSteering.ResolveSteerDirection(
            rb.position,
            bodyCastRadius,
            direction,
            probeDistance,
            obstacleMask,
            steerAnglesDeg);

        EnemyObstacleSteering.MoveWithCollision(rb, bodyCastRadius, steerDir, currentSpeed, obstacleMask);
    }

    public void ResetState()
    {
        slowTimer = 0f;
        fearTimer = 0f;
        dotDuration = 0f;
        dotTickTimer = 0f;
        gameplayEnabled = true;

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
