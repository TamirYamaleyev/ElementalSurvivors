using UnityEngine;

public class ProjectileSystem : MonoBehaviour
{
    public Projectile Fire(
        Projectile prefab,
        Vector2 position,
        Quaternion rotation,
        float damage,
        float speed,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem)
    {
        // replace with pooling
        Projectile proj = Instantiate(prefab, position, rotation);

        proj.Init(damage, speed, status, statusDuration, statusSystem);

        return proj;
    }
}
