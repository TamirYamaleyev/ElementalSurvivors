using UnityEngine;

public static class EnemyProjectileUtility
{
    public static EnemyProjectile Spawn(
        EnemyProjectile prefab,
        Vector2 origin,
        Vector2 direction,
        float damage,
        float speed,
        float lifetime = 5f)
    {
        if (prefab == null)
            return null;

        var instance = Object.Instantiate(prefab, origin, Quaternion.identity);
        instance.Init(direction, damage, speed, lifetime);
        return instance;
    }
}
