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
    [SerializeField] private EnemyContactDamage contactDamage;

    private Vector3 defaultLocalScale;
    private Action<Enemy> poolRelease;
    private int poolTierIndex = -1;
    private bool isInitialized;
    private bool subscribedToDeath;

    private StatusSystem statusSystem;
    private EnemyRegistry registry;
    private Transform playerTransform;

    public EnemyStatusController StatusController => status;
    public int PoolTierIndex => poolTierIndex;
    public float BaselineMaxHealth => health.BaselineMaxHealth;
    public float BaselineContactDamage => health.BaselineContactDamage;
    public float LastDamageReceived => health.LastDamageReceived;
    public EnemyAI AI => ai;
    public Transform PlayerTransform => playerTransform;

    private void Awake()
    {
        if (ai == null)
            ai = GetComponent<EnemyAI>();
        if (health == null)
            health = GetComponent<EnemyHealth>();
        if (status == null)
            status = GetComponent<EnemyStatusController>();
        if (contactDamage == null)
            contactDamage = GetComponent<EnemyContactDamage>();
        if (contactDamage == null)
            contactDamage = gameObject.AddComponent<EnemyContactDamage>();
        if (GetComponent<EnemyDamageNumberPresenter>() == null)
            gameObject.AddComponent<EnemyDamageNumberPresenter>();

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
        ConfigureEnemyRegistry(registry);
        isInitialized = true;
    }

    private void ConfigureEnemyRegistry(EnemyRegistry registryRef)
    {
        var vfxPresenter = GetComponent<ElementalStatusVfxPresenter>();
        if (vfxPresenter != null && registryRef != null)
            vfxPresenter.SetEnemyRegistry(registryRef);
    }

    public void OnAcquire(SpawnContext ctx)
    {
        status?.ResetForPool();

        transform.SetPositionAndRotation(ctx.Position, Quaternion.identity);
        transform.localScale = defaultLocalScale;

        if (ctx.VisualScaleMultiplier > 0f && !Mathf.Approximately(ctx.VisualScaleMultiplier, 1f))
            transform.localScale = defaultLocalScale * ctx.VisualScaleMultiplier;

        health.ApplyScaledStats(ctx.ScaledMaxHealth, ctx.ScaledContactDamage);
        ConfigurePlayerTarget(PlayerController.Instance);
        ai.EnsureInitialized();
        contactDamage?.EnsureInitialized(playerTransform);
        ai.SetGameplayEnabled(true);

        if (!isInitialized && statusSystem != null)
            Initialize(statusSystem, registry);

        registry?.Register(this);
        SubscribeToDeath();
        gameObject.SetActive(true);
    }

    public void OnReleaseToPool()
    {
        UnsubscribeFromDeath();
        registry?.Unregister(this);

        // Deactivate before restoring HP / AI so late-frame hits cannot re-apply debuffs.
        gameObject.SetActive(false);

        ai.ResetState();
        ai.SetGameplayEnabled(false);
        health.ResetState();
        status?.ResetForPool();

        foreach (var reset in GetComponents<IEnemyPoolReset>())
            reset.ResetForPool();

        transform.localScale = defaultLocalScale;
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
    }

    private void ConfigurePlayerTarget(Transform player)
    {
        playerTransform = player;
        ai?.SetPlayerTarget(playerTransform);

        if (contactDamage != null)
            contactDamage.SetPlayerTarget(playerTransform);

        var rangedAttack = GetComponent<EnemyRangedAttack>();
        if (rangedAttack != null)
            rangedAttack.SetPlayerTarget(playerTransform);

        var bossAi = GetComponent<BossAI>();
        if (bossAi != null)
            bossAi.SetPlayerTarget(playerTransform);

        var bossAttack = GetComponent<BossAttackController>();
        if (bossAttack != null)
            bossAttack.SetPlayerTarget(playerTransform);

        ConfigureEnemyRegistry(registry);
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
