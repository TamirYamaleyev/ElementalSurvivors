using UnityEngine;

public class AreaSystem : MonoBehaviour
{
    public void Cast(
        AreaWeapon prefab,
        Transform spawnPoint,
        float width,
        float height,
        float damage,
        float lifetime,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite sprite = null)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[AreaSystem] Spawnskipped: area prefab is null.");
            return;
        }

        if (statusSystem == null)
        {
            Debug.LogWarning("[AreaSystem] Spawn skipped: StatusSystem is null.");
            return;
        }

        var obj = Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);

        obj.Init(
            spawnPoint.position,
            width,
            height,
            damage,
            lifetime,
            status,
            statusDuration,
            statusSystem,
            sprite
        );
    }
}
