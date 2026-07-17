using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tornado : MonoBehaviour
{
    [SerializeField] AudioClip sfx;

    [Header("Animation")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float animationFPS = 12f;
    [SerializeField] private SpriteRenderer sr;

    [SerializeField] private float fadeDuration = 0.5f;

    private bool canHit = true;

    private float lifetimeTimer;
    private bool fading;

    private float animationTimer;
    private int currentFrame;

    private float damage;
    private float speed;
    private float knockback;
    private float lifetime;

    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    private Vector2 targetPosition;

    private HashSet<Enemy> hitEnemies = new();

    void Awake()
    {
        AudioManager.Instance.PlaySfx(sfx, 0.15f);    
    }

    public void Init(
        float damage,
        float speed,
        float range,
        float knockback,
        float statusDuration,
        float lifetime,
        StatusType status,
        StatusSystem statusSystem)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.knockback = knockback;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        PickNewTarget();

        lifetimeTimer = lifetime;
        //Destroy(gameObject, lifetime);
    }

    void Update()
    {
        Animate();

        if (fading)
            return;

        lifetimeTimer -= Time.deltaTime;

        if (lifetimeTimer <= 0f)
        {
            StartFade();
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
            PickNewTarget();
    }

    private void StartFade()
    {
        fading = true;
        canHit = false;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;

        Color color = sr.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            sr.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }

    private void Animate()
    {
        if (sprites.Length == 0)
            return;

        animationTimer += Time.deltaTime;

        float frameDuration = 1f / animationFPS;

        if (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;

            currentFrame++;
            if (currentFrame >= sprites.Length)
                currentFrame = 0;

            sr.sprite = sprites[currentFrame];
        }
    }

    private void PickNewTarget()
    {
        Camera cam = Camera.main;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(
            new Vector3(0, 0, -cam.transform.position.z));

        Vector3 topRight = cam.ViewportToWorldPoint(
            new Vector3(1, 1, -cam.transform.position.z));

        targetPosition = new Vector2(
            Random.Range(bottomLeft.x, topRight.x),
            Random.Range(bottomLeft.y, topRight.y));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canHit)
            return;

        if (!CombatHitUtility.TryResolveEnemyFromHit(other, out Enemy enemy))
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        CombatHitUtility.ApplyStatusThenDamage(enemy, statusSystem, status, statusDuration, damage);

        Vector2 direction = ((Vector2)enemy.transform.position - (Vector2)transform.position).normalized;

        CombatHitUtility.ApplyKnockback(enemy, direction, knockback);
    }
}
