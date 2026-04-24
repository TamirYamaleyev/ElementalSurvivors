using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    public static event System.Action<EnemyAI> CombatRequested;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject expOrb;
    [SerializeField] private CombatStatsSO combatStats;

    [Header("Settings")]
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("Enemy only chases the player while they are within this distance (world units).")]
    [SerializeField] private float aggroRadius = 6f;

    private Transform player;
    private Vector2 direction;
    private float currentHealth;
    private bool movementEnabled = true;

    public CombatStatsSO CombatStats => combatStats;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
    }

    void Start()
    {
        player = PlayerController.Instance;
    }

    void FixedUpdate()
    {
        if (!movementEnabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        FollowPlayer();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            RequestCombat(this);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.CompareTag("Player"))
            RequestCombat(this);
    }

    private void FollowPlayer()
    {
        if (player == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float aggroSq = aggroRadius * aggroRadius;
        if (toPlayer.sqrMagnitude > aggroSq)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (toPlayer.sqrMagnitude > 0.0001f)
            direction = toPlayer.normalized;
        else
            direction = Vector2.zero;

        rb.linearVelocity = direction * moveSpeed;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        Debug.Log($"Took {amount} damage\nNew HP: {currentHealth}");

        if (currentHealth <= 0f)
            Defeat();
    }

    /// <summary>Turn-based combat defeated this enemy — same cleanup as world death.</summary>
    public void DefeatFromCombat()
    {
        Defeat();
    }

    private void Defeat()
    {
        if (expOrb != null)
            Instantiate(expOrb, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public static void RequestCombat(EnemyAI enemy)
    {
        CombatRequested?.Invoke(enemy);
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled)
            rb.linearVelocity = Vector2.zero;
    }
}
