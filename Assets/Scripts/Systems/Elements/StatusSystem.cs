using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    private ReactionVfxCatalogSO reactionVfxCatalog;
    private ReactionGameplayCatalogSO reactionGameplayCatalog;
    private ElementalStatusGameplayCatalogSO elementalStatusGameplayCatalog;
    private EnemyRegistry enemyRegistry;
    private ReactionEffectSystem effectSystem;

    private readonly List<StatusPair> pairScratch = new();

    public ReactionVfxCatalogSO ReactionCatalog => reactionVfxCatalog;
    public ReactionGameplayCatalogSO GameplayCatalog => reactionGameplayCatalog;
    public ElementalStatusGameplayCatalogSO ElementalGameplayCatalog => elementalStatusGameplayCatalog;

    public void SetReactionVfxCatalog(ReactionVfxCatalogSO catalog)
    {
        reactionVfxCatalog = catalog;
        if (effectSystem != null)
            effectSystem.SetVfxCatalog(catalog);
    }

    public void SetReactionGameplayCatalog(ReactionGameplayCatalogSO catalog)
    {
        reactionGameplayCatalog = catalog;
        if (effectSystem != null)
            effectSystem.SetGameplayCatalog(catalog);
    }

    public void SetElementalStatusGameplayCatalog(ElementalStatusGameplayCatalogSO catalog)
    {
        elementalStatusGameplayCatalog = catalog;
    }

    public void SetEnemyRegistry(EnemyRegistry registry)
    {
        enemyRegistry = registry;
        if (effectSystem != null)
            effectSystem.SetEnemyRegistry(registry);
    }

    public void SetEffectSystem(ReactionEffectSystem system)
    {
        effectSystem = system;
        if (effectSystem == null)
            return;

        if (reactionGameplayCatalog != null)
            effectSystem.SetGameplayCatalog(reactionGameplayCatalog);
        if (reactionVfxCatalog != null)
            effectSystem.SetVfxCatalog(reactionVfxCatalog);
        if (enemyRegistry != null)
            effectSystem.SetEnemyRegistry(enemyRegistry);
    }

    public void Apply(Enemy enemy, StatusType type, float duration)
    {
        if (type == StatusType.None || enemy == null || !enemy.gameObject.activeInHierarchy)
            return;

        enemy.StatusController.AddStatus(type, duration);
    }

    public bool ResolveInteractions(Enemy enemy, List<StatusInstance> existing, StatusType incomingType)
    {
        if (enemy == null || effectSystem == null || incomingType == StatusType.None)
            return false;

        pairScratch.Clear();
        for (var i = 0; i < existing.Count; i++)
        {
            var status = existing[i];
            if (status.type == incomingType)
                continue;

            pairScratch.Add(new StatusPair(status.type, incomingType));
        }

        var usedIncoming = false;
        for (var i = 0; i < pairScratch.Count; i++)
        {
            var pair = pairScratch[i];
            if (!enemy.StatusController.IsPairAvailable(pair.First, pair.Second, incomingType))
                continue;

            if (!effectSystem.TryProcReaction(enemy, pair.First, pair.Second))
                continue;

            if (pair.First == incomingType || pair.Second == incomingType)
                usedIncoming = true;

            enemy.StatusController.ConsumePairForProc(pair.First, pair.Second, incomingType);
        }

        return usedIncoming;
    }

    /// <summary>Spawns a one-shot world reaction burst (showcase / debug).</summary>
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
        var parent = ReactionVfxShowcaseBootstrap.VfxContainer;
        var instance = parent != null
            ? Object.Instantiate(prefab, pos, Quaternion.identity, parent)
            : Object.Instantiate(prefab, pos, Quaternion.identity);

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
