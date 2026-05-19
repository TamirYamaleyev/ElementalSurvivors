using UnityEngine;

public class WeaponSystemContext
{
    public TargetingSystem Targeting;
    public ProjectileSystem ProjectileSystem;
    public AreaSystem AreaSystem;
    public OrbitSystem OrbitSystem;
    public StatusSystem StatusSystem;
    public IPlayerStatsProvider PlayerStats;

    public Transform ProjectileSpawnPoint;
    public Transform OrbitCenter;
}
