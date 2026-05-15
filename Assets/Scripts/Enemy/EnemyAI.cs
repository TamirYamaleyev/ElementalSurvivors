
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Settings")]
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 5f;

    private float slowMultiplier = 0.5f;
    private float slowTimer;

    private float dotDuration;
    private float dotTickTimer;
    private float dotDamage = 1f;
    private float dotTickInterval = 0.5f;

    private float fearTimer;

    private PlayerHealth playerRef;
    private Transform player;
    private Vector2 direction;
    private float currentHealth;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
    }

    void Start()
    {
        player = PlayerController.Instance;

        if (playerRef == null)
            playerRef = player.GetComponent<PlayerHealth>();
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

    void FixedUpdate()
    {
        FollowPlayer();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerRef != null)
                playerRef.TakeDamage(damage);
        }
    }

    private void SetDirection()
    {
        if (player == null) return;

        Vector2 baseDir = (player.position - transform.position).normalized;

        if (fearTimer > 0f)
        {
            direction = -baseDir;

            fearTimer -= Time.deltaTime;
        }
        else
        {
            direction = baseDir;
        }
    }

    private void FollowPlayer()
    {
        SetDirection();

        float currentSpeed = moveSpeed;

        if (slowTimer > 0f)
        {
            currentSpeed *= slowMultiplier;
            slowTimer -= Time.deltaTime;
        }

        rb.linearVelocity = direction * currentSpeed;
    }

    public float BaselineMaxHealth => maxHealth;
    public float BaselineContactDamage => damage;

    public void ApplyScaledStats(float maxHp, float contactDamage)
    {
        maxHealth = maxHp;
        currentHealth = maxHp;
        damage = contactDamage;
    }
}
