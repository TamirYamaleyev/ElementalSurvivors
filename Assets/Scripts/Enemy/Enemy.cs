using System;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyStatusEffects))]
[RequireComponent(typeof(EnemyContactDamage))]
public class Enemy : MonoBehaviour, IDamageable, IStatusEffectTarget, IPoolable
{
    private EnemyStats stats;
    private EnemyHealth health;
    private EnemyMovement movement;
    private EnemyStatusEffects statusEffects;
    private EnemyContactDamage contactDamage;
    private Rigidbody2D rb;

    private Vector3 defaultLocalScale;
    private Action<Enemy> poolReturn;
    private bool subscribedToDeath;

    public EnemyTier PoolTier { get; private set; }

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyMovement>();
        statusEffects = GetComponent<EnemyStatusEffects>();
        contactDamage = GetComponent<EnemyContactDamage>();
        rb = GetComponent<Rigidbody2D>();
        defaultLocalScale = transform.localScale;
    }

    public void BindPoolReturn(Action<Enemy> releaseHandler, EnemyTier tier)
    {
        poolReturn = releaseHandler;
        PoolTier = tier;
    }

    public void ConfigureSpawn(EnemySpawnContext context, Vector3 position)
    {
        transform.SetPositionAndRotation(position, Quaternion.identity);
        transform.localScale = defaultLocalScale;

        if (context.IsBoss && context.BossVisualScale > 0f)
            transform.localScale = defaultLocalScale * context.BossVisualScale;

        stats.SetRuntime(context.ScaledMaxHp, context.ScaledContactDamage);
        health.Initialize(context.ScaledMaxHp);

        if (!subscribedToDeath)
        {
            health.Died += OnHealthDied;
            subscribedToDeath = true;
        }

        movement.EnableGameplay();
        statusEffects.EnableGameplay();
        contactDamage.EnableGameplay();

        if (rb != null)
            rb.simulated = true;

        OnAcquired();
    }

    public void OnAcquired()
    {
        gameObject.SetActive(true);
    }

    public void OnReleased()
    {
        movement.DisableGameplay();
        statusEffects.DisableGameplay();
        contactDamage.DisableGameplay();

        if (rb != null)
        {
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }

        stats.ClearRuntime();
        health.Clear();
        transform.localScale = defaultLocalScale;
        gameObject.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }

    public void ApplySlow(float duration, float multiplier)
    {
        statusEffects.ApplySlow(duration, multiplier);
    }

    public void ApplyFear(float duration)
    {
        statusEffects.ApplyFear(duration);
    }

    public void ApplyDoT(float duration, float damagePerTick)
    {
        statusEffects.ApplyDoT(duration, damagePerTick);
    }

    private void OnHealthDied(EnemyHealth _)
    {
        poolReturn?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (subscribedToDeath && health != null)
            health.Died -= OnHealthDied;
    }
}
