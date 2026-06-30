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
        var registry = enemyRegistry != null ? enemyRegistry : FindAnyObjectByType<EnemyRegistry>();

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

        var gameplay = AddGameplayComponent(root, pair);
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

        if (!UsesSustainedLifecycle(pair, definition))
            Destroy(root, GetInstantCleanupDelay(pair));

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

    private static bool UsesSustainedLifecycle(StatusPair pair, ReactionGameplayDefinition definition)
    {
        if (definition.mode == ReactionGameplayMode.Sustained)
            return true;

        return pair == new StatusPair(StatusType.Fire, StatusType.Water)
            || pair == new StatusPair(StatusType.Water, StatusType.Wind)
            || pair == new StatusPair(StatusType.Water, StatusType.Earth)
            || pair == new StatusPair(StatusType.Wind, StatusType.Earth)
            || pair == new StatusPair(StatusType.Wind, StatusType.Lightning);
    }

    private static float GetInstantCleanupDelay(StatusPair pair)
    {
        if (pair.First == StatusType.Fire && pair.Second == StatusType.Lightning)
            return 0.9f;

        if (pair.First == StatusType.Fire && pair.Second == StatusType.Wind)
            return 0.4f;

        if (pair.First == StatusType.Water && pair.Second == StatusType.Lightning)
            return 0.25f;

        if (pair.First == StatusType.Fire && pair.Second == StatusType.Earth)
            return 0.35f;

        if (pair.First == StatusType.Earth && pair.Second == StatusType.Lightning)
            return 1.8f;

        return 0.2f;
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

    private static IReactionGameplayEffect AddGameplayComponent(GameObject root, StatusPair pair)
    {
        return (pair.First, pair.Second) switch
        {
            (StatusType.Fire, StatusType.Water) => root.AddComponent<ReactionVaporizeZoneEffect>(),
            (StatusType.Fire, StatusType.Earth) => root.AddComponent<ReactionCrystallizeEffect>(),
            (StatusType.Fire, StatusType.Wind) => root.AddComponent<ReactionScorchingWindEffect>(),
            (StatusType.Fire, StatusType.Lightning) => root.AddComponent<ReactionExplosionEffect>(),
            (StatusType.Water, StatusType.Wind) => root.AddComponent<ReactionHailEffect>(),
            (StatusType.Water, StatusType.Earth) => root.AddComponent<ReactionGrowthZoneEffect>(),
            (StatusType.Water, StatusType.Lightning) => root.AddComponent<ReactionElectrowettingEffect>(),
            (StatusType.Wind, StatusType.Earth) => root.AddComponent<ReactionDustSandStormEffect>(),
            (StatusType.Wind, StatusType.Lightning) => root.AddComponent<ReactionMagnetismEffect>(),
            (StatusType.Earth, StatusType.Lightning) => root.AddComponent<ReactionStaticChargeEffect>(),
            _ => null,
        };
    }
}
