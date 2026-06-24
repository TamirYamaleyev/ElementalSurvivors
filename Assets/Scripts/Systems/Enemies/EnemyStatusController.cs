using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour statusVisualSink;

    private readonly List<StatusType> uniqueActiveScratch = new();

    private List<StatusInstance> statuses = new();

    private StatusSystem system;
    private Enemy owner;

    private IEnemyStatusVisualSink VisualSink
    {
        get
        {
            if (statusVisualSink is IEnemyStatusVisualSink sink)
                return sink;

            return GetComponent<ElementalStatusVfxPresenter>();
        }
    }

    public void Initialize(StatusSystem statusSystem, Enemy enemy)
    {
        system = statusSystem;
        owner = enemy;
    }

    public void AddStatus(StatusType type, float duration)
    {
        if (type == StatusType.None)
            return;

        var newStatus = new StatusInstance
        {
            type = type,
            duration = duration,
            timer = duration
        };

        system.ResolveInteractions(owner, statuses, newStatus);

        statuses.Add(newStatus);
        RefreshVisuals();
    }

    void Update()
    {
        var visualsDirty = false;

        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            statuses[i].timer -= Time.deltaTime;

            if (statuses[i].timer <= 0)
            {
                statuses.RemoveAt(i);
                visualsDirty = true;
            }
        }

        if (visualsDirty)
            RefreshVisuals();
    }

    public IReadOnlyList<StatusType> GetUniqueActiveTypes()
    {
        uniqueActiveScratch.Clear();

        for (var i = 0; i < statuses.Count; i++)
        {
            var type = statuses[i].type;
            if (type == StatusType.None)
                continue;

            if (!uniqueActiveScratch.Contains(type))
                uniqueActiveScratch.Add(type);
        }

        return uniqueActiveScratch;
    }

    /// <summary>Clears active statuses without notifying the visual sink (call presenter reset separately).</summary>
    public void ClearAllStatuses()
    {
        statuses.Clear();
    }

    private void RefreshVisuals()
    {
        if (VisualSink == null || system == null)
            return;

        var plan = StatusVfxResolver.Build(GetUniqueActiveTypes(), system.ReactionCatalog);
        VisualSink.RefreshStatusVisuals(plan);
    }
}
