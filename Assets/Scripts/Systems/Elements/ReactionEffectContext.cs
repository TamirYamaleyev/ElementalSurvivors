using UnityEngine;

public readonly struct ReactionEffectContext
{
    public Enemy SourceEnemy { get; }
    public StatusPair Pair { get; }
    public Vector3 Center { get; }
    public EnemyRegistry Registry { get; }
    public Transform PlayerTransform { get; }
    public float TriggerDamage { get; }

    public ReactionEffectContext(
        Enemy sourceEnemy,
        StatusPair pair,
        Vector3 center,
        EnemyRegistry registry,
        Transform playerTransform,
        float triggerDamage)
    {
        SourceEnemy = sourceEnemy;
        Pair = pair;
        Center = center;
        Registry = registry;
        PlayerTransform = playerTransform;
        TriggerDamage = triggerDamage;
    }
}
