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
        Projectile proj = Instantiate(prefab, position, Quaternion.identity);

        Vector2 dir = targetPos - position;
        if (dir.sqrMagnitude < 1e-6f)
            dir = Vector2.right;
        else
            dir.Normalize();

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        proj.Init(damage, speed, status, statusDuration, statusSystem);

        return proj;
    }
}
