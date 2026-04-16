using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerHealth playerRef;

    [Header("Settings")]
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 5f;

    private Transform player;
    private Vector2 direction;
    private float currentHealth;

    void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerRef == null)
            playerRef = GetComponent<PlayerHealth>();

        currentHealth = maxHealth;
    }

    void Start()
    {
        player = PlayerController.Instance;
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
        direction = (player.position - transform.position).normalized;
    }

    private void FollowPlayer()
    {
        SetDirection();

        rb.linearVelocity = direction * moveSpeed;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
