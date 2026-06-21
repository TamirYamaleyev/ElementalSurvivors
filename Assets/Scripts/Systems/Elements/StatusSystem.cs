using System.Collections.Generic;
using UnityEngine;

public class StatusSystem : MonoBehaviour
{
    private ReactionVfxCatalogSO reactionVfxCatalog;

    public void SetReactionVfxCatalog(ReactionVfxCatalogSO catalog)
    {
        reactionVfxCatalog = catalog;
    }

    public void Apply(Enemy enemy, StatusType type, float duration)
    {
        if (type == StatusType.None)
            return;

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

        var pos = enemy.transform.position + Vector3.up * 0.25f;
        var instance = Object.Instantiate(prefab, pos, Quaternion.identity);

        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }
}
