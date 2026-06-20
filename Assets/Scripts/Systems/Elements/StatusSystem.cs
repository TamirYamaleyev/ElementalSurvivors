using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    private ReactionVfxCatalogSO reactionVfxCatalog;
    private EnemyRegistry enemyRegistry;

    public void SetReactionVfxCatalog(ReactionVfxCatalogSO catalog)
    {
        reactionVfxCatalog = catalog;
    }

    public void SetEnemyRegistry(EnemyRegistry registry)
    {
        enemyRegistry = registry;
    }

    public void Apply(Enemy enemy, StatusType type, float duration)
    {
        enemy.StatusController.AddStatus(type, duration);
    }

    public void ResolveInteractions(Enemy enemy, List<StatusInstance> existing, StatusInstance incoming)
    {
        foreach (var s in existing)
        {
            if (s.type == incoming.type)
                continue;

            TryTriggerInteraction(enemy, s.type, incoming.type);
        }
    }

    private void TryTriggerInteraction(Enemy enemy, StatusType a, StatusType b)
    {
        if (reactionVfxCatalog == null || enemy == null)
            return;

        var prefab = reactionVfxCatalog.GetPrefab(a, b);
        if (prefab == null)
            return;

        SpawnReactionVfxInstance(enemy, prefab);
    }

    /// <summary>Spawns reaction burst VFX for a pair without changing enemy statuses.</summary>
    public void SpawnReactionVfx(Enemy enemy, StatusType a, StatusType b)
    {
        if (reactionVfxCatalog == null || enemy == null)
            return;

        var prefab = reactionVfxCatalog.GetPrefab(a, b);
        if (prefab == null)
            return;

        SpawnReactionVfxInstance(enemy, prefab);
    }

    private void SpawnReactionVfxInstance(Enemy enemy, GameObject prefab)
    {
        var pos = enemy.transform.position + Vector3.up * 0.25f;
        var instance = Object.Instantiate(prefab, pos, Quaternion.identity);

        var ctx = new ReactionVfxContext(pos, enemy, enemyRegistry);
        foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>())
        {
            if (behaviour is IReactionWorldVfx worldVfx)
                worldVfx.Initialize(ctx);
        }

        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }
}
