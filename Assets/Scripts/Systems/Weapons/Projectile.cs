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

    [SerializeField] private float overlapHitRadius = 0.22f;

    private bool consumed;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    public void Init(Vector2 direction, float damage, float speed, StatusType status, float statusDuration, StatusSystem statusSystem, Sprite visualSprite, float lifetime = 5f)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        this.damage = damage;
        this.speed = speed;
        this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;

        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        consumed = false;

        if (sr != null && visualSprite != null)
            sr.sprite = visualSprite;

        DirectionFacing2D.Apply(transform, this.direction, artForward, extraRotationOffset);

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);
        TryOverlapHit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemy(other, out Enemy enemy))
            return;

        ApplyHit(enemy);
    }

    private void TryOverlapHit()
    {
        if (consumed)
            return;

        var hit = Physics2D.OverlapCircle(transform.position, overlapHitRadius, enemyLayer);
        if (hit == null || !CombatHitUtility.TryResolveEnemy(hit, out Enemy enemy))
            return;

        ApplyHit(enemy);
    }

    private void ApplyHit(Enemy enemy)
    {
        if (consumed || enemy == null)
            return;

        consumed = true;
        CombatHitUtility.ApplyStatusThenDamage(enemy, statusSystem, status, statusDuration, damage);
        Destroy(gameObject);
    }
}
