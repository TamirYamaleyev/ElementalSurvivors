using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private ProjectileSystem projectile;
    [SerializeField] private AreaSystem area;
    [SerializeField] private OrbitSystem orbit;
    [SerializeField] private StatusSystem status;

    [Header("World State")]
    [SerializeField] private EnemyRegistry registry;

    [Header("Player")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAimDirection playerAimDirection;

    [Header("Transforms")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private Transform areaSpawnPoint;

    [Header("Runtime")]
    [SerializeField] private WeaponSystem weaponSystem;

    void Awake()
    {
        weaponSystem.Initialize(BuildWeaponContext());
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

            PlayerStats = playerStats,
            AimDirection = playerAimDirection
        };
    }
}
