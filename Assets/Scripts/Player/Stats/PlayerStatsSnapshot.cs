public readonly struct PlayerStatsSnapshot
{
    public float MaxHealth { get; }
    public float MoveSpeed { get; }
    public float DamageMultiplier { get; }
    public float AttackSpeed { get; }
    public float ProjectileSpeedMultiplier { get; }
    public float CollectRadius { get; }

    public PlayerStatsSnapshot(
        float maxHealth,
        float moveSpeed,
        float damageMultiplier,
        float attackSpeed,
        float projectileSpeedMultiplier,
        float collectRadius)
    {
        MaxHealth = maxHealth;
        MoveSpeed = moveSpeed;
        DamageMultiplier = damageMultiplier;
        AttackSpeed = attackSpeed;
        ProjectileSpeedMultiplier = projectileSpeedMultiplier;
        CollectRadius = collectRadius;
    }
}
