using System.Collections.Generic;
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
        float maxRangeSqr = maxRange * maxRange;
        float bestDistSqr = float.MaxValue;

        Vector3 pos = position;

        for (int i = 0; i < registry.ActiveEnemies.Count; i++)
        {
            Enemy e = registry.ActiveEnemies[i];

            if (!CombatHitUtility.IsEnemyTargetable(e))
                continue;

            Vector3 diff = e.transform.position - pos;
            float distSqr = diff.sqrMagnitude;

            if (distSqr > maxRangeSqr || distSqr >= bestDistSqr)
                continue;

            bestDistSqr = distSqr;
            best = e;
        }

        return best;
    }

    public List<Enemy> GetChainTargets(Vector2 startPosition, int chainCount, float range)
    {
        var result = new List<Enemy>();
        if (registry == null || registry.ActiveEnemies == null || chainCount <= 0)
            return result;

        var visited = new HashSet<Enemy>();
        Vector2 currentPos = startPosition;
        float maxSqr = range * range;

        for (int i = 0; i < chainCount; i++)
        {
            Enemy best = null;
            float bestDist = float.MaxValue;

            foreach (var enemy in registry.ActiveEnemies)
            {
                if (!CombatHitUtility.IsEnemyTargetable(enemy) || visited.Contains(enemy))
                    continue;

                Vector2 delta = (Vector2)enemy.transform.position - currentPos;
                float d = delta.sqrMagnitude;

                if (d > maxSqr || d >= bestDist)
                    continue;

                bestDist = d;
                best = enemy;
            }

            if (best == null)
                break;

            result.Add(best);
            visited.Add(best);
            currentPos = best.transform.position;
        }

        return result;
    }
}
