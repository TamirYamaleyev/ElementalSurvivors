using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusController : MonoBehaviour
{
    private const int ElementCount = 5;

    [SerializeField] private MonoBehaviour statusVisualSink;

    private readonly List<StatusType> uniqueActiveScratch = new();
    private readonly float[] dotTickTimers = new float[ElementCount];
    private readonly bool[] dotActiveScratch = new bool[ElementCount];

    private List<StatusInstance> statuses = new();

    private StatusSystem system;
    private Enemy owner;
    private EnemyHealth health;
    private ElementalStatusGameplayCatalogSO gameplayCatalog;

    private IEnemyStatusVisualSink VisualSink
    {
        get
        {
            if (statusVisualSink is IEnemyStatusVisualSink sink)
                return sink;

            return GetComponent<ElementalStatusVfxPresenter>();
        }
    }

    public void Initialize(
        StatusSystem statusSystem,
        Enemy enemy,
        ElementalStatusGameplayCatalogSO elementalGameplayCatalog)
    {
        system = statusSystem;
        owner = enemy;
        gameplayCatalog = elementalGameplayCatalog;
        health = owner != null ? owner.GetComponent<EnemyHealth>() : null;
    }

    public void AddStatus(StatusType type, float duration)
    {
        if (type == StatusType.None || owner == null || !owner.gameObject.activeInHierarchy)
            return;

        var incomingConsumed = system.ResolveInteractions(owner, statuses, type);
        if (incomingConsumed)
        {
            RefreshVisuals();
            return;
        }

        if (TryRefreshExistingStatus(type, duration))
        {
            RefreshVisuals();
            return;
        }

        statuses.Add(new StatusInstance
        {
            type = type,
            duration = duration,
            timer = duration
        });

        RefreshVisuals();
    }

    void Update()
    {
        if (owner == null || !owner.gameObject.activeInHierarchy)
            return;

        var visualsDirty = false;

        for (int i = statuses.Count - 1; i >= 0; i--)
        {
            statuses[i].timer -= Time.deltaTime;

            if (statuses[i].timer <= 0f)
            {
                ClearDotTimer(statuses[i].type);
                statuses.RemoveAt(i);
                visualsDirty = true;
            }
        }

        TickElementalDoT();

        if (visualsDirty)
            RefreshVisuals();
    }

    public bool HasStatus(StatusType type)
    {
        if (type == StatusType.None)
            return false;

        for (var i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].type == type)
                return true;
        }

        return false;
    }

    public bool IsPairAvailable(StatusType a, StatusType b, StatusType incomingType)
    {
        return IsTypeAvailable(a, incomingType) && IsTypeAvailable(b, incomingType);
    }

    public void ConsumePairForProc(StatusType a, StatusType b, StatusType incomingType)
    {
        var changed = false;

        if (a != incomingType)
            changed |= RemoveFirstOfType(a);

        if (b != incomingType)
            changed |= RemoveFirstOfType(b);

        if (changed)
            RefreshVisuals();
    }

    private bool IsTypeAvailable(StatusType type, StatusType incomingType)
    {
        if (type == incomingType)
            return true;

        return HasStatus(type);
    }

    private bool RemoveFirstOfType(StatusType type)
    {
        for (var i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].type != type)
                continue;

            statuses.RemoveAt(i);
            ClearDotTimer(type);
            return true;
        }

        return false;
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
        ClearAllDotTimers();
    }

    private bool TryRefreshExistingStatus(StatusType type, float duration)
    {
        for (var i = 0; i < statuses.Count; i++)
        {
            if (statuses[i].type != type)
                continue;

            statuses[i].duration = duration;
            statuses[i].timer = duration;
            return true;
        }

        return false;
    }

    private void TickElementalDoT()
    {
        if (gameplayCatalog == null || health == null)
            return;

        var tickInterval = gameplayCatalog.TickInterval;
        if (tickInterval <= 0f)
            return;

        SyncDotActiveFlags();

        for (var typeIndex = 0; typeIndex < ElementCount; typeIndex++)
        {
            if (!dotActiveScratch[typeIndex])
            {
                dotTickTimers[typeIndex] = 0f;
                continue;
            }

            dotTickTimers[typeIndex] += Time.deltaTime;
            if (dotTickTimers[typeIndex] < tickInterval)
                continue;

            dotTickTimers[typeIndex] = 0f;
            var statusType = (StatusType)typeIndex;
            health.TakeDamage(gameplayCatalog.DamagePerTick, gameplayCatalog.GetDamageColor(statusType));
        }
    }

    private void SyncDotActiveFlags()
    {
        for (var i = 0; i < ElementCount; i++)
            dotActiveScratch[i] = false;

        for (var i = 0; i < statuses.Count; i++)
        {
            var type = statuses[i].type;
            if (type == StatusType.None)
                continue;

            var typeIndex = (int)type;
            if (typeIndex >= 0 && typeIndex < ElementCount)
                dotActiveScratch[typeIndex] = true;
        }
    }

    private void ClearDotTimer(StatusType type)
    {
        var typeIndex = (int)type;
        if (typeIndex < 0 || typeIndex >= ElementCount)
            return;

        dotTickTimers[typeIndex] = 0f;
    }

    private void ClearAllDotTimers()
    {
        for (var i = 0; i < ElementCount; i++)
            dotTickTimers[i] = 0f;
    }

    private void RefreshVisuals()
    {
        if (VisualSink == null || system == null)
            return;

        var plan = StatusVfxResolver.Build(GetUniqueActiveTypes(), system.ReactionCatalog);
        VisualSink.RefreshStatusVisuals(plan);
    }
}
