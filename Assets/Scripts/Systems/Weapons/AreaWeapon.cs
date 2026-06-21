using UnityEngine;

public class AreaWeapon : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private LayerMask enemyLayer;

    private Vector2 position;
    private float width;
    private float height;
    private float damage;
    private float lifetime;
    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();
    }

    public void Init(
        Vector2 position,
        float width,
        float height,
        float damage,
        float lifetime,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite visualSprite
        )
    {
        this.position = position;
        this.damage = damage;
        this.lifetime = lifetime;
        this.width = width;
        this.height = height;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null && visualSprite != null)
            sr.sprite = visualSprite;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemy(other, out Enemy enemy))
            return;

        enemy.TakeDamage(damage);
        statusSystem.Apply(enemy, status, statusDuration);
    }
}
