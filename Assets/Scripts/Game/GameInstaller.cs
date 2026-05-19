using UnityEngine;

public class GameInstaller : MonoBehaviour
{
    [SerializeField] private TargetingSystem targeting;
    [SerializeField] private ProjectileSystem projectile;
    [SerializeField] private AreaSystem area;
    [SerializeField] private OrbitSystem orbit;
    [SerializeField] private StatusSystem status;
    [SerializeField] private PlayerStats playerStats;

    public WeaponSystemContext BuildWeaponContext()
    {
        return new WeaponSystemContext
        {
            Targeting = targeting,
            ProjectileSystem = projectile,
            AreaSystem = area,
            OrbitSystem = orbit,
            StatusSystem = status,
            PlayerStats = playerStats
        };
    }
}
