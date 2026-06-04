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
        StatusSystem statusSystem)
    {
        // replace with pooling
        Projectile proj = Instantiate(prefab, position, Quaternion.identity);

        proj.Init(damage, speed, status, statusDuration, statusSystem);

        return proj;
    }
}
