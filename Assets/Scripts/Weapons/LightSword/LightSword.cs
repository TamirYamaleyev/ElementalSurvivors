using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSword : MonoBehaviour
{
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private float rotAngle = 90f;
    [SerializeField] private SpriteRenderer sr;

    private float damage;
    private float lifetime;

    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    private HashSet<Enemy> hitEnemies = new();

    void Start()
    {
        Rotate90(true);
    }

    public void Init(
        float damage,
        float lifetime,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite visualSprite)
    {
        this.damage = damage;
        this.lifetime = lifetime;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        if (sr != null && visualSprite != null)
            sr.sprite = visualSprite;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemy(other, out Enemy enemy))
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        enemy.TakeDamage(damage);

        statusSystem.Apply(enemy, status, statusDuration);
    }

    private void Rotate90(bool isClockwise)
    {
        if (isClockwise)
            StartCoroutine(RotateRoutine(rotAngle));
        else
            StartCoroutine(RotateRoutine(-rotAngle));
    }

    private IEnumerator RotateRoutine(float angle)
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0f, 0f, angle);

        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / rotationDuration;
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);

            yield return null;
        }

        transform.rotation = targetRotation;
    }
}
