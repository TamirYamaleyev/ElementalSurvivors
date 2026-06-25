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
        Projectile proj = Instantiate(prefab, position, Quaternion.identity);
        proj.Init(direction, damage, speed, status, statusDuration, statusSystem, sprite, lifetime);
        return proj;
    }
}
