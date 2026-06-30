using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    public Projectile Fire(
        Projectile prefab,
        Vector2 position,
        Vector2 direction,
        float damage,
        float speed,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite sprite,
        float lifetime = 5f)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ProjectileSystem] Fire skipped: projectile prefab is null.");
            return null;
        }

        var instance = Instantiate(prefab.gameObject, position, Quaternion.identity);
        if (!instance.TryGetComponent(out Projectile proj))
        {
            Debug.LogError(
                $"[ProjectileSystem] Prefab '{prefab.name}' has no Projectile component.",
                prefab);
            Destroy(instance);
            return null;
        }

        proj.Init(direction, damage, speed, status, statusDuration, statusSystem, sprite, lifetime);
        return proj;
    }
}
