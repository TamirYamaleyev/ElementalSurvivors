using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] AudioClip sfx;

    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private DirectionFacing2D.ArtForward artForward = DirectionFacing2D.ArtForward.Right;
    [SerializeField] private float extraRotationOffset;
    [SerializeField] private AnimationScr animScr;

    private Vector2 direction;
    private float damage;
    private float speed;

    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    public void SwapSpriteSheet(Sprite[] newSprites)
    {
        animScr.SwapSprites(newSprites);
    }

    private void Awake()
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sfx != null)
            AudioManager.Instance.PlaySfx(sfx);
    }

    public void Init(Vector2 direction, float damage, float speed, StatusType status, float statusDuration, StatusSystem statusSystem, Sprite[] visualSprites, float lifetime = 5f)
    {
        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        this.damage = damage;
        this.speed = speed;
        this.direction = direction.sqrMagnitude > 1e-6f ? direction.normalized : Vector2.right;

        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        if (sr != null && visualSprites != null)
            sr.sprite = visualSprites[0];

        DirectionFacing2D.Apply(transform, this.direction, artForward, extraRotationOffset);

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemy(other, out Enemy enemy))
            return;

        CombatHitUtility.ApplyStatusThenDamage(enemy, statusSystem, status, statusDuration, damage);

        // replace with pooling(?)
        Destroy(gameObject);
    }
}
