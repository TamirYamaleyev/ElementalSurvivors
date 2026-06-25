using System;
using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyStatusController))]
public class Enemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private EnemyAI ai;
    [SerializeField] private EnemyStatusController status;
    [SerializeField] private EnemyHealth health;

    private Vector3 defaultLocalScale;
    private Action<Enemy> poolRelease;
    private int poolTierIndex = -1;
    private bool isInitialized;
    private bool subscribedToDeath;

    private StatusSystem statusSystem;
    private EnemyRegistry registry;

    public EnemyStatusController StatusController => status;
    public int PoolTierIndex => poolTierIndex;
    public float BaselineMaxHealth => health.BaselineMaxHealth;
    public float BaselineContactDamage => health.BaselineContactDamage;
    public float LastDamageReceived => health.LastDamageReceived;
    public EnemyAI AI => ai;

    private void Awake()
    {
        if (ai == null)
            ai = GetComponent<EnemyAI>();
        if (health == null)
            health = GetComponent<EnemyHealth>();
        if (status == null)
            status = GetComponent<EnemyStatusController>();

        defaultLocalScale = transform.localScale;
    }

    private void OnDestroy()
    {
        UnsubscribeFromDeath();
    }

    public void BindPool(Action<Enemy> release, int tierIndex)
    {
        poolRelease = release;
        poolTierIndex = tierIndex;
    }

    public void ConfigureSystems(StatusSystem statusSystemRef, EnemyRegistry registryRef)
    {
        statusSystem = statusSystemRef;
        registry = registryRef;
    }

    public void Initialize(StatusSystem statusSystemRef, EnemyRegistry registryRef)
    {
        ConfigureSystems(statusSystemRef, registryRef);

        if (statusSystem != null)
            status.Initialize(statusSystem, this, statusSystem.ElementalGameplayCatalog);

        health.Initialize(this);
        isInitialized = true;
    }

    public void OnAcquire(SpawnContext ctx)
    {
        transform.SetPositionAndRotation(ctx.Position, Quaternion.identity);
        transform.localScale = defaultLocalScale;

        if (ctx.VisualScaleMultiplier > 0f && !Mathf.Approximately(ctx.VisualScaleMultiplier, 1f))
            transform.localScale = defaultLocalScale * ctx.VisualScaleMultiplier;

        health.ApplyScaledStats(ctx.ScaledMaxHealth, ctx.ScaledContactDamage);
        ai.EnsureInitialized();
        health.EnsureInitialized();
        ai.SetGameplayEnabled(true);

        if (!isInitialized)
            Initialize(statusSystem, registry);

        registry?.Register(this);
        SubscribeToDeath();
        gameObject.SetActive(true);
    }

    public void OnReleaseToPool()
    {
        UnsubscribeFromDeath();
        registry?.Unregister(this);

        ai.SetGameplayEnabled(false);
        ai.ResetState();
        health.ResetState();
        status?.ClearAllStatuses();
        GetComponent<ElementalStatusVfxPresenter>()?.ResetForPool();
        transform.localScale = defaultLocalScale;
        gameObject.SetActive(false);
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }

    private void SubscribeToDeath()
    {
        if (subscribedToDeath)
            return;

        health.OnDied += HandleDeath;
        subscribedToDeath = true;
    }

    private void UnsubscribeFromDeath()
    {
        if (!subscribedToDeath)
            return;

        health.OnDied -= HandleDeath;
        subscribedToDeath = false;
    }

    private void HandleDeath()
    {
        UnsubscribeFromDeath();
        registry?.Unregister(this);
        health.SpawnDeathLoot();

        if (poolRelease != null)
            poolRelease(this);
        else
            Destroy(gameObject);
    }
}
