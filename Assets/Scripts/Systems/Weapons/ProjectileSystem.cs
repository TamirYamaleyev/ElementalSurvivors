using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    public Projectile Fire(
        Projectile prefab,
        Vector2 position,
        Vector2 targetPos,
        float damage,
        float speed,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite sprite)
    {
        // replace with pooling
        Projectile proj = Instantiate(prefab, position, Quaternion.identity);

        proj.Init(targetPos, damage, speed, status, statusDuration, statusSystem, sprite);

        return proj;
    }
}
