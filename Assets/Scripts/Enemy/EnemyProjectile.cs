using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private DirectionFacing2D.ArtForward artForward = DirectionFacing2D.ArtForward.Right;
    [SerializeField] private float extraRotationOffset;

    private Vector2 direction;
    private float damage;
    private float speed;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 direction, float damage, float speed, float lifetime = 5f)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        this.damage = damage;
        this.speed = speed;
        this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;

        DirectionFacing2D.Apply(transform, this.direction, artForward, extraRotationOffset);

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        var playerHealth = other.GetComponent<PlayerHealth>()
            ?? other.GetComponentInParent<PlayerHealth>();
        playerHealth?.TakeDamage(damage);

        Destroy(gameObject);
    }
}
