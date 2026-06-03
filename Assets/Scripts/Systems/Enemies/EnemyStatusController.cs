using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusController : MonoBehaviour
{
    private readonly List<StatusInstance> statuses = new();

    private StatusSystem system;
    private Enemy owner;

    public event Action<IReadOnlyList<StatusSnapshot>> StatusesChanged;

    public void Initialize(StatusSystem statusSystem, Enemy enemy)
    {
        if (owner != null)
            return;

        system = statusSystem;
        owner = enemy;
    }

    public void CopySnapshotsTo(List<StatusSnapshot> destination)
    {
        destination.Clear();
        for (int i = 0; i < statuses.Count; i++)
        {
            var s = statuses[i];
            destination.Add(new StatusSnapshot(s.type, s.timer));
        }
    }

    public void AddStatus(StatusType type, float duration)
    {
        EnsureInitialized();

        var newStatus = new StatusInstance
        {
            type = type,
            duration = duration,
            timer = duration
        };

        system.ResolveInteractions(owner, statuses, newStatus);

        statuses.Add(newStatus);
        RaiseStatusesChanged();
    }

    private void Update()
    {
        var changed = false;
        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            statuses[i].timer -= Time.deltaTime;

            if (statuses[i].timer <= 0f)
            {
                statuses.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            RaiseStatusesChanged();
    }

    private void OnDisable()
    {
        if (statuses.Count <= 0)
            return;

        statuses.Clear();
        RaiseStatusesChanged();
    }

    private void EnsureInitialized()
    {
        if (owner == null)
            owner = GetComponent<Enemy>();

        if (system == null)
            system = FindAnyObjectByType<StatusSystem>();
    }

    private void RaiseStatusesChanged()
    {
        if (StatusesChanged == null)
            return;

        var list = new List<StatusSnapshot>(statuses.Count);
        CopySnapshotsTo(list);
        StatusesChanged.Invoke(list);
    }
}
