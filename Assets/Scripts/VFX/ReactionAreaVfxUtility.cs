using System.Collections.Generic;
using UnityEngine;

public static class ReactionAreaVfxUtility
{
    public static void CollectEnemiesInRadius(
        EnemyRegistry registry,
        Vector3 center,
        float radius,
        List<Enemy> results,
        Enemy exclude = null)
    {
        results.Clear();
        if (registry == null || radius <= 0f)
            return;

        var radiusSq = radius * radius;

        foreach (var enemy in registry.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            if (exclude != null && enemy == exclude)
                continue;

            var delta = enemy.transform.position - center;
            if (delta.sqrMagnitude > radiusSq)
                continue;

            results.Add(enemy);
        }
    }
}
