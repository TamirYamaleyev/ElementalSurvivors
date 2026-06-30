using UnityEngine;

public sealed class ReactionEffectSystem : MonoBehaviour
{
    [SerializeField] private ReactionGameplayCatalogSO gameplayCatalog;
    [SerializeField] private ReactionVfxCatalogSO vfxCatalog;

    private EnemyRegistry enemyRegistry;
    private StatusSystem statusSystem;

    public ReactionGameplayCatalogSO GameplayCatalog => gameplayCatalog;

    public void SetGameplayCatalog(ReactionGameplayCatalogSO catalog)
    {
        gameplayCatalog = catalog;
    }

    public void SetVfxCatalog(ReactionVfxCatalogSO catalog)
    {
        vfxCatalog = catalog;
    }

    public void SetEnemyRegistry(EnemyRegistry registry)
    {
        enemyRegistry = registry;
    }

    public void SetStatusSystem(StatusSystem system)
    {
        statusSystem = system;
    }

    public bool TryProcReaction(Enemy sourceEnemy, StatusType a, StatusType b, float pendingDamage = 0f)
    {
        if (sourceEnemy == null || gameplayCatalog == null)
            return false;

        if (!gameplayCatalog.TryGetDefinition(a, b, out var definition))
            return false;

        var pair = new StatusPair(a, b);
        var center = sourceEnemy.transform.position + Vector3.up * 0.25f;
        var player = PlayerController.Instance;
        var registry = enemyRegistry;

        if (registry == null)
        {
            Debug.LogWarning("ReactionEffectSystem: EnemyRegistry is not configured.");
            return false;
        }

        var triggerDamage = pendingDamage > 0f
            ? pendingDamage
            : sourceEnemy.LastDamageReceived;

        var ctx = new ReactionEffectContext(
            sourceEnemy,
            pair,
            center,
            registry,
            player,
            triggerDamage,
            statusSystem);

        var root = new GameObject($"ReactionGameplay_{pair.First}_{pair.Second}");
        var parent = ResolveReactionParent();
        if (parent != null)
            root.transform.SetParent(parent, worldPositionStays: true);

        root.transform.position = center;

        var gameplay = ReactionEffectRegistry.CreateGameplayComponent(root, pair);
        if (gameplay == null)
        {
            Destroy(root);
            return false;
        }

        gameplay.Initialize(ctx, definition);

        var vfxPrefab = definition.vfxPrefab != null
            ? definition.vfxPrefab
            : vfxCatalog != null ? vfxCatalog.GetPrefab(pair.First, pair.Second) : null;

        if (vfxPrefab != null)
        {
            var vfxInstance = Instantiate(vfxPrefab, root.transform);
            vfxInstance.transform.localPosition = Vector3.zero;

            if (definition.mode == ReactionGameplayMode.Sustained)
                ConfigureSustainedReactionVfx(vfxInstance, definition);

            var vfxCtx = new ReactionVfxContext(center, sourceEnemy, registry);
            foreach (var behaviour in vfxInstance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is IReactionWorldVfx worldVfx)
                    worldVfx.Initialize(vfxCtx);
            }

            foreach (var ps in vfxInstance.GetComponentsInChildren<ParticleSystem>(true))
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }

        if (!ReactionEffectRegistry.UsesSustainedLifecycle(pair, definition))
            Destroy(root, ReactionEffectRegistry.GetInstantCleanupDelay(pair));

        return true;
    }

    private static Transform ResolveReactionParent()
    {
        var runtime = ReactionRuntimeAnchor.Root;
        if (runtime != null && runtime.gameObject.activeInHierarchy)
            return runtime;

        var showcase = ReactionVfxShowcaseBootstrap.VfxContainer;
        if (showcase != null && showcase.gameObject.activeInHierarchy)
            return showcase;

        return null;
    }

    private static void ConfigureSustainedReactionVfx(GameObject vfxRoot, ReactionGameplayDefinition definition)
    {
        if (vfxRoot == null || definition == null)
            return;

        var lifetime = Mathf.Max(0.1f, definition.duration);

        foreach (var burst in vfxRoot.GetComponentsInChildren<ReactionBurstLifetime>(true))
        {
            burst.SetDestroyAfter(lifetime);
            burst.DisableAutoDestroy();
        }

        foreach (var overlay in vfxRoot.GetComponentsInChildren<ReactionVaporizeAreaOverlay>(true))
            overlay.Configure(lifetime, definition.radius);
    }
}
