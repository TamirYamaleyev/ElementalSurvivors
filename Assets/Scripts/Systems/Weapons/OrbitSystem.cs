using System.Collections.Generic;
using UnityEngine;

public class OrbitSystem : MonoBehaviour
{
    private List<OrbitingObject> orbitObjects = new();

    public void ClearOrbitObjects()
    {
        foreach (var orbitObj in orbitObjects)
        {
            Destroy(orbitObj.gameObject);
        }

        orbitObjects.Clear();
    }

    public void Spawn(
        OrbitingObject prefab,
        Transform center,
        int count,
        float radius,
        float speed,
        float damage,
        StatusType status,
        float statusDuration,
        StatusSystem statusSystem,
        Sprite[] sprites)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[OrbitSystem] Spawn skipped: orbit prefab is null.");
            return;
        }

        if (center == null)
        {
            Debug.LogWarning("[OrbitSystem] Spawn skipped: orbit center is null.");
            return;
        }

        if (statusSystem == null)
        {
            Debug.LogWarning("[OrbitSystem] Spawn skipped: StatusSystem is null.");
            return;
        }

        if (count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, center.position, Quaternion.identity, center);

            obj.Init(
                i,
                count,
                radius,
                speed,
                damage,
                status,
                statusDuration,
                statusSystem,
                center,
                sprites);

            orbitObjects.Add(obj);
        }

    }
}
