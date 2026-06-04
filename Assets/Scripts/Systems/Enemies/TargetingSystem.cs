using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    private EnemyRegistry registry;

    public void Initialize(EnemyRegistry enemyRegistry)
    {
        registry = enemyRegistry;
    }

    public Enemy GetNearest(Vector3 position, float maxRange)
    {
        if (registry == null || registry.ActiveEnemies == null)
            return null;

        Enemy best = null;
        float bestDistSqr = float.MaxValue * maxRange;

        Vector3 pos = position;

        for (int i = 0; i < registry.ActiveEnemies.Count; i++)
        {
            Enemy e = registry.ActiveEnemies[i];

            if (e == null) 
                continue;

            Vector3 diff = e.transform.position - pos;
            float distSqr = diff.sqrMagnitude;

            if (distSqr <= bestDistSqr)
            {
                bestDistSqr = distSqr;
                best = e;
            }
        }

        return best;
    }
}