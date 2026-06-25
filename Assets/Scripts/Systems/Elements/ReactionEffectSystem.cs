using UnityEngine;

public sealed class ReactionEffectSystem : MonoBehaviour
{
    [SerializeField] private ReactionGameplayCatalogSO gameplayCatalog;
    [SerializeField] private ReactionVfxCatalogSO vfxCatalog;

    private EnemyRegistry enemyRegistry;

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

    public bool TryProcReaction(Enemy sourceEnemy, StatusType a, StatusType b)
    {
        if (sourceEnemy == null || gameplayCatalog == null)
            return false;

        if (!gameplayCatalog.TryGetDefinition(a, b, out var definition))
            return false;

        var pair = new StatusPair(a, b);
        var center = sourceEnemy.transform.position + Vector3.up * 0.25f;
        var player = PlayerController.Instance;
        var registry = enemyRegistry != null ? enemyRegistry : FindAnyObjectByType<EnemyRegistry>();

        var ctx = new ReactionEffectContext(
            sourceEnemy,
            pair,
            center,
            registry,
            player,
            sourceEnemy.LastDamageReceived);

        var root = new GameObject($"ReactionGameplay_{pair.First}_{pair.Second}");
        var parent = ReactionVfxShowcaseBootstrap.VfxContainer;
        if (parent != null)
            root.transform.SetParent(parent, worldPositionStays: true);

        root.transform.position = center;

        var gameplay = AddGameplayComponent(root, pair);
        if (gameplay == null)
        {
            Destroy(root);
            return false;
        }

        var vfxPrefab = definition.vfxPrefab != null
            ? definition.vfxPrefab
            : vfxCatalog != null ? vfxCatalog.GetPrefab(pair.First, pair.Second) : null;

        if (vfxPrefab != null)
        {
            var vfxInstance = Instantiate(vfxPrefab, root.transform);
            vfxInstance.transform.localPosition = Vector3.zero;

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

        gameplay.Initialize(ctx, definition);

        if (definition.mode == ReactionGameplayMode.Instant)
            Destroy(root, GetInstantCleanupDelay(pair));

        return true;
    }

    private static float GetInstantCleanupDelay(StatusPair pair)
    {
        if (pair.First == StatusType.Fire && pair.Second == StatusType.Lightning)
            return 0.9f;

        if (pair.First == StatusType.Fire && pair.Second == StatusType.Wind)
            return 0.4f;

        if (pair.First == StatusType.Water && pair.Second == StatusType.Lightning)
            return 0.25f;

        return 0.2f;
    }

    private static IReactionGameplayEffect AddGameplayComponent(GameObject root, StatusPair pair)
    {
        return (pair.First, pair.Second) switch
        {
            (StatusType.Fire, StatusType.Water) => root.AddComponent<ReactionVaporizeZoneEffect>(),
            (StatusType.Fire, StatusType.Wind) => root.AddComponent<ReactionScorchingWindEffect>(),
            (StatusType.Fire, StatusType.Lightning) => root.AddComponent<ReactionExplosionEffect>(),
            (StatusType.Water, StatusType.Wind) => root.AddComponent<ReactionHailEffect>(),
            (StatusType.Water, StatusType.Lightning) => root.AddComponent<ReactionElectrowettingEffect>(),
            (StatusType.Wind, StatusType.Lightning) => root.AddComponent<ReactionMagnetismEffect>(),
            _ => null,
        };
    }
}
