using UnityEngine;

/// <summary>Attaches reaction prefabs to an enemy for sustained status-driven display.</summary>
public static class ReactionVfxAttachUtility
{
    public static GameObject AttachToEnemy(
        GameObject prefab,
        Transform parent,
        Vector3 localOffset,
        Enemy enemy,
        EnemyRegistry registry)
    {
        var instance = Object.Instantiate(prefab, parent);
        instance.transform.SetLocalPositionAndRotation(localOffset, Quaternion.identity);

        if (enemy != null)
            instance.layer = parent.gameObject.layer;

        foreach (var life in instance.GetComponentsInChildren<ReactionBurstLifetime>(true))
            Object.Destroy(life);

        if (enemy != null)
        {
            var center = enemy.transform.position + Vector3.up * 0.25f;
            var ctx = new ReactionVfxContext(center, enemy, registry);
            foreach (var behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is IReactionWorldVfx worldVfx)
                    worldVfx.Initialize(ctx);
            }
        }

        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }

        return instance;
    }
}
