using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private ProjectileSystem projectile;
    [SerializeField] private AreaSystem area;
    [SerializeField] private OrbitSystem orbit;
    [SerializeField] private StatusSystem status;
    [SerializeField] private ReactionEffectSystem reactionEffectSystem;
    [SerializeField] private ReactionVfxCatalogSO reactionVfxCatalog;
    [SerializeField] private ReactionGameplayCatalogSO reactionGameplayCatalog;
    [SerializeField] private ElementalStatusGameplayCatalogSO elementalStatusGameplayCatalog;

    [Header("World State")]
    [SerializeField] private EnemyRegistry registry;

    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAimDirection playerAimDirection;

    [Header("Transforms")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Transform areaSpawnPoint;
    [SerializeField] private Transform playerTransformPoint;

    [Header("Runtime")]
    [SerializeField] private WeaponSystem weaponSystem;

    void Awake()
    {
        EnsureReactionAnchor();

        if (status != null)
        {
            if (reactionEffectSystem == null)
                reactionEffectSystem = status.GetComponent<ReactionEffectSystem>();

            if (reactionEffectSystem == null)
                reactionEffectSystem = status.gameObject.AddComponent<ReactionEffectSystem>();

            status.SetReactionVfxCatalog(reactionVfxCatalog);
            status.SetReactionGameplayCatalog(reactionGameplayCatalog);
            status.SetElementalStatusGameplayCatalog(elementalStatusGameplayCatalog);
            status.SetEnemyRegistry(registry);
            status.SetEffectSystem(reactionEffectSystem);
        }

        weaponSystem.Initialize(BuildWeaponContext());
    }

    private void OnDestroy()
    {
        if (reactionAnchorRoot != null)
            ReactionRuntimeAnchor.ClearRoot(reactionAnchorRoot);
    }

    private Transform reactionAnchorRoot;

    private void EnsureReactionAnchor()
    {
        if (ReactionRuntimeAnchor.Root != null)
            return;

        var anchorGo = new GameObject("ReactionEffects");
        anchorGo.transform.SetParent(transform, false);
        reactionAnchorRoot = anchorGo.transform;
        ReactionRuntimeAnchor.SetRoot(reactionAnchorRoot);
    }

    public WeaponSystemContext BuildWeaponContext()
    {
        return new WeaponSystemContext
        {
            EnemyRegistry = registry,

            Targeting = targeting,
            ProjectileSystem = projectile,
            AreaSystem = area,
            OrbitSystem = orbit,
            StatusSystem = status,

            ProjectileSpawnPoint = projectileSpawnPoint,
            OrbitCenter = orbitCenter,
            AreaSpawnPoint = areaSpawnPoint,
            PlayerTransformPoint = playerTransformPoint != null
                ? playerTransformPoint
                : projectileSpawnPoint,

            PlayerStats = playerStats,
            AimDirection = playerAimDirection,
            PlayerAnimation = playerTransformPoint != null
                ? playerTransformPoint.GetComponent<PlayerCharacterAnimation>()
                : null
        };
    }
}
