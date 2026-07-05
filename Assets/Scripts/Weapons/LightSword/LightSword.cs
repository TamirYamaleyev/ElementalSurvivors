using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSword : MonoBehaviour
{
    [SerializeField] private float rotationDuration = 0.5f;
    [SerializeField] private float rotAngle = 90f;
    //[SerializeField] private SpriteRenderer sr;
    [SerializeField] private Transform visualsRoot;

    private float damage;
    private float lifetime;

    private StatusType status;
    private float statusDuration;
    private StatusSystem statusSystem;

    private HashSet<Enemy> hitEnemies = new();

    private Quaternion baseRotation;
    private Vector3 baseVisualScale;

    void Awake()
    {
        if (visualsRoot != null)
            baseVisualScale = visualsRoot.localScale;
    }

    public void Init(
        float damage,
        float lifetime,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite visualSprite,
        Vector2 slashDirection)
    {
        this.damage = damage;
        this.lifetime = lifetime;
        this.status = status;
        this.statusDuration = statusDuration;
        this.statusSystem = statusSystem;

        //if (sr != null && visualSprite != null)
        //    sr.sprite = visualSprite;

        // BASE ROTATION
        float angle = Mathf.Atan2(slashDirection.y, slashDirection.x) * Mathf.Rad2Deg;
        baseRotation = Quaternion.Euler(0f, 0f, angle + 90f);
        transform.rotation = baseRotation;

        // VISUAL FLIP
        float side = Mathf.Sign(slashDirection.x);

        if (visualsRoot != null)
            visualsRoot.localRotation = Quaternion.Euler(0f, side < 0 ? 0f : 180f, 0f);

        // SWING

        StartCoroutine(RotateRoutine(-rotAngle * side));

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CombatHitUtility.TryResolveEnemy(other, out Enemy enemy))
            return;

        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        CombatHitUtility.ApplyStatusThenDamage(enemy, statusSystem, status, statusDuration, damage);
    }

    private IEnumerator RotateRoutine(float swingAngle)
    {
        Quaternion startRotation = baseRotation;
        Quaternion targetRotation = baseRotation * Quaternion.Euler(0f, 0f, swingAngle);

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
