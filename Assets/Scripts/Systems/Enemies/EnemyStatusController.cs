using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusController : MonoBehaviour
{
    private List<StatusInstance> statuses = new();

    private StatusSystem system;
    private Enemy owner;

    public void Initialize(StatusSystem statusSystem, Enemy enemy)
    {
        system = statusSystem;
        owner = enemy;
    }

    public void AddStatus(StatusType type, float duration)
    {
        var newStatus = new StatusInstance
        {
            type = type,
            duration = duration,
            timer = duration
        };

        system.ResolveInteractions(owner, statuses, newStatus);

        statuses.Add(newStatus);
    }

    void Update()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            statuses[i].timer -= Time.deltaTime;

            if (statuses[i].timer <= 0)
                statuses.RemoveAt(i);
        }    
    }
}
