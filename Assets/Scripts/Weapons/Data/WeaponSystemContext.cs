using UnityEngine;

public class WeaponSystemContext
{
    public TargetingSystem Targeting;
    public ProjectileSystem ProjectileSystem;
    public AreaSystem AreaSystem;
    public OrbitSystem OrbitSystem;
    public StatusSystem StatusSystem;

    public PlayerStats PlayerStats;
    public PlayerAimDirection AimDirection;
    public EnemyRegistry EnemyRegistry;

    public Transform ProjectileSpawnPoint;
    public Transform OrbitCenter;
    public Transform AreaSpawnPoint;
    public Transform PlayerTransformPoint;
}