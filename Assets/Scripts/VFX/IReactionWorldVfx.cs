using UnityEngine;

public interface IReactionWorldVfx
{
    void Initialize(ReactionVfxContext ctx);
}

public readonly struct ReactionVfxContext
{
    public Vector3 Center { get; }
    public Enemy SourceEnemy { get; }
    public EnemyRegistry Registry { get; }

    public ReactionVfxContext(Vector3 center, Enemy sourceEnemy, EnemyRegistry registry)
    {
        Center = center;
        SourceEnemy = sourceEnemy;
        Registry = registry;
    }
}
