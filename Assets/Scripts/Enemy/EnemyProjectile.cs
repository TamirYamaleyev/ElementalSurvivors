using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class EnemyProjectile : MonoBehaviour
{
    private const int PlayerLayer = 6;
    private const int EnemyLayer = 9;

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private DirectionFacing2D.ArtForward artForward = DirectionFacing2D.ArtForward.Right;
    [SerializeField] private float extraRotationOffset;
    [SerializeField] private float overlapHitRadius = 0.22f;

    private Rigidbody2D rb;
    private Vector2 direction;
    private float damage;
    private float speed;
    private bool consumed;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, float damage, float speed, float lifetime = 5f)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        this.damage = damage;
        this.speed = speed;
        this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;
        consumed = false;

        DirectionFacing2D.Apply(transform, this.direction, artForward, extraRotationOffset);

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        var delta = direction * speed * Time.fixedDeltaTime;
        if (rb != null)
            rb.MovePosition(rb.position + delta);
        else
            transform.position += (Vector3)delta;

        TryOverlapHit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == EnemyLayer)
            return;

        if (!IsPlayerCollider(other))
            return;

        ApplyDamageToPlayer(other);
    }

    private void TryOverlapHit()
    {
        if (consumed)
            return;

        var hit = Physics2D.OverlapCircle(transform.position, overlapHitRadius, 1 << PlayerLayer);
        if (hit != null)
            ApplyDamageToPlayer(hit);
    }

    private static bool IsPlayerCollider(Collider2D other)
    {
        return other.gameObject.layer == PlayerLayer || other.CompareTag("Player");
    }

    private void ApplyDamageToPlayer(Collider2D other)
    {
        if (consumed)
            return;

        var playerHealth = other.GetComponent<PlayerHealth>()
            ?? other.GetComponentInParent<PlayerHealth>();
        if (playerHealth == null)
            return;

        consumed = true;
        playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }
}
