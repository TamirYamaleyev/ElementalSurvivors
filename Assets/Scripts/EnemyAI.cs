using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private GameObject expOrb;

    [Header("Settings")]
    [SerializeField] private float maxHealth = 25f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 5f;

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

        Debug.Log($"Took {amount} damage\nNew HP: {currentHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        Instantiate(expOrb, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
