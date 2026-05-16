public readonly struct EnemySpawnContext
{
    public readonly float ScaledMaxHp;
    public readonly float ScaledContactDamage;
    public readonly bool IsBoss;
    public readonly float BossVisualScale;

    public EnemySpawnContext(
        float scaledMaxHp,
        float scaledContactDamage,
        bool isBoss,
        float bossVisualScale)
    {
        ScaledMaxHp = scaledMaxHp;
        ScaledContactDamage = scaledContactDamage;
        IsBoss = isBoss;
        BossVisualScale = bossVisualScale;
    }
}
