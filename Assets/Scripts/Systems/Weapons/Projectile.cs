using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private DirectionFacing2D.ArtForward artForward = DirectionFacing2D.ArtForward.Right;
    [SerializeField] private float extraRotationOffset;

    private Vector2 direction;
    private float damage;
    private float speed;

    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 direction, float damage, float speed, StatusType status, float statusDuration, StatusSystem statusSystem, Sprite visualSprite)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        this.damage = damage;
        this.speed = speed;
        this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;

        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        if (sr != null && visualSprite != null)
            sr.sprite = visualSprite;

        DirectionFacing2D.Apply(transform, this.direction, artForward, extraRotationOffset);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemy(other, out Enemy enemy))
            return;

        enemy.TakeDamage(damage);
        statusSystem.Apply(enemy, status, statusDuration);

        // replace with pooling(?)
        Destroy(gameObject);
    }
}
