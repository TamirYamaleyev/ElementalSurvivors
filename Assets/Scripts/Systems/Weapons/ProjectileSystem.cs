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
        Sprite sprite)
    {
        Projectile proj = Instantiate(prefab, position, Quaternion.identity);
        proj.Init(direction, damage, speed, status, statusDuration, statusSystem, sprite);
        return proj;
    }
}
