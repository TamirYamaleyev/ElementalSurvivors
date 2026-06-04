using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private ProjectileSystem projectile;
    [SerializeField] private AreaSystem area;
    [SerializeField] private OrbitSystem orbit;
    [SerializeField] private StatusSystem status;

    [Header("Player")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerAimDirection playerAimDirection;

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
            Targeting = targeting,
            ProjectileSystem = projectile,
            AreaSystem = area,
            OrbitSystem = orbit,
            StatusSystem = status,

            ProjectileSpawnPoint = projectileSpawnPoint,
            OrbitCenter = orbitCenter,
            PlayerStats = playerStats,
            AimDirection = playerAimDirection
        };
    }
}
