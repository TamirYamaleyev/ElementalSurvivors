using UnityEngine;

public class OrbitSystem : MonoBehaviour
{
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
        Sprite sprite
        )

    {
        for (int i = 0; i < count; i++)
        {
            // replace with pooling
            var obj = Instantiate(prefab);

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
                sprite
                );
        }
    }
}
