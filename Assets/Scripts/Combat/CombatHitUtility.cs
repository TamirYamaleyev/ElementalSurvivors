using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy prefabs use a trigger capsule (player contact) and a solid circle (obstacles).
/// Weapon hits must ignore trigger colliders or the same enemy is damaged twice per contact.
/// </summary>
public static class CombatHitUtility
{
    public static void ApplyKnockback(Enemy enemy, Vector2 direction, float force)
    {
        if (enemy == null || force <= 0f)
            return;

        if (enemy.AI != null)
            enemy.AI.ApplyKnockback(direction, force);
    }

    public static bool IsWeaponHitCollider(Collider2D collider)
    {
        return collider != null && !collider.isTrigger;
    }

    public static bool IsEnemyAlive(Enemy enemy)
    {
        if (enemy == null || !enemy.gameObject.activeInHierarchy)
            return false;

        var health = enemy.GetComponent<EnemyHealth>();
        return health != null && health.CurrentHealth > 0f;
    }

    public static bool IsEnemyTargetable(Enemy enemy)
    {
        return IsEnemyAlive(enemy)
            && enemy.AI != null
            && enemy.AI.IsGameplayEnabled;
    }

    public static bool TryResolveEnemyFromHit(Collider2D collider, out Enemy enemy)
    {
        return TryResolveEnemy(collider, out enemy);
    }

    public static bool TryResolveEnemy(Collider2D collider, out Enemy enemy)
    {
        enemy = null;
        if (!IsWeaponHitCollider(collider))
            return false;

        enemy = collider.GetComponentInParent<Enemy>();
        return enemy != null && enemy.gameObject.activeInHierarchy;
    }

    public static bool TryResolveEnemyHealth(Collider2D collider, out EnemyHealth health)
    {
        health = null;
        if (!IsWeaponHitCollider(collider))
            return false;

        health = collider.GetComponentInParent<EnemyHealth>();
        return health != null;
    }

    public static void ForEachEnemyInArea(
        Vector2 position,
        float radius,
        LayerMask enemyLayer,
        Action<Enemy> onEnemy)
    {
        if (onEnemy == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(position, radius, enemyLayer);
        var processed = new HashSet<Enemy>();

        foreach (Collider2D hit in hits)
        {
            if (!TryResolveEnemy(hit, out Enemy enemy) || !processed.Add(enemy))
                continue;

            onEnemy(enemy);
        }
    }

    /// <summary>Applies elemental status before damage so reaction procs can run before a lethal hit deactivates the enemy.</summary>
    public static void ApplyStatusThenDamage(
        Enemy enemy,
        StatusSystem statusSystem,
        StatusType status,
        float statusDuration,
        float damage)
    {
        if (enemy == null)
            return;

        if (!IsEnemyAlive(enemy))
            return;

        if (statusSystem != null && status != StatusType.None && statusDuration > 0f)
            statusSystem.Apply(enemy, status, statusDuration, damage);

        if (damage > 0f)
            enemy.TakeDamage(damage);
    }
}
