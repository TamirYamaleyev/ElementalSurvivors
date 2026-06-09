using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour statusVisualSink;

    private List<StatusInstance> statuses = new();

    private StatusSystem system;
    private Enemy owner;

    private IEnemyStatusVisualSink VisualSink => statusVisualSink as IEnemyStatusVisualSink;

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
        VisualSink?.OnStatusApplied(type);
    }

    void Update()
    {
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            statuses[i].timer -= Time.deltaTime;

            if (statuses[i].timer <= 0)
            {
                var ended = statuses[i].type;
                VisualSink?.OnStatusRemoved(ended);
                statuses.RemoveAt(i);
            }
        }    
    }
}
