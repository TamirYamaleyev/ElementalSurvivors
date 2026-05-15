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
        Enemy best = null;
        float bestDist = float.MaxValue;

        foreach (var enemy in registry.ActiveEnemies)
        {
            float d = Vector3.SqrMagnitude(enemy.transform.position - position);

            if (d < bestDist && d <= maxRange * maxRange)
            {
                bestDist = d;
                best = enemy;
            }
        }

        return best;
    }
}
