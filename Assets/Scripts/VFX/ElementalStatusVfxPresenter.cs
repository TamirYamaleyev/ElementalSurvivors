using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one elemental particle prefab per <see cref="StatusType"/> while ref-count &gt; 0.
/// </summary>
public class ElementalStatusVfxPresenter : MonoBehaviour, IEnemyStatusVisualSink
{
    [SerializeField] private Transform optionalAnchor;
    [SerializeField] private Vector3 vfxLocalOffset = new(0f, -0.2f, 0f);

    [Header("Status prefabs (root with ParticleSystem)")]
    [SerializeField] private GameObject fireStatusPrefab;
    [SerializeField] private GameObject waterStatusPrefab;
    [SerializeField] private GameObject windStatusPrefab;
    [SerializeField] private GameObject earthStatusPrefab;
    [SerializeField] private GameObject lightningStatusPrefab;

    private readonly Dictionary<StatusType, int> refCounts = new();
    private readonly Dictionary<StatusType, GameObject> activeRoots = new();

    private Transform Anchor => optionalAnchor != null ? optionalAnchor : transform;

    private void OnDisable()
    {
        ClearAllInstances();
    }

    public void OnStatusApplied(StatusType type)
    {
        if (!refCounts.TryGetValue(type, out var count))
            count = 0;
        refCounts[type] = count + 1;

        if (count > 0)
            return;

        var prefab = GetPrefab(type);
        if (prefab == null)
            return;

        var parent = Anchor;
        var instance = Instantiate(prefab, parent);
        instance.transform.SetLocalPositionAndRotation(vfxLocalOffset, Quaternion.identity);

        activeRoots[type] = instance;

        foreach (var ps in instance.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    public void OnStatusRemoved(StatusType type)
    {
        if (!refCounts.TryGetValue(type, out var count))
            return;

        count--;
        if (count <= 0)
        {
            refCounts.Remove(type);
            if (activeRoots.TryGetValue(type, out var root) && root != null)
            {
                foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                Destroy(root);
                activeRoots.Remove(type);
            }
        }
        else
        {
            refCounts[type] = count;
        }
    }

    private void ClearAllInstances()
    {
        foreach (var kv in activeRoots)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }

        activeRoots.Clear();
        refCounts.Clear();
    }

    private GameObject GetPrefab(StatusType type)
    {
        return type switch
        {
            StatusType.Fire => fireStatusPrefab,
            StatusType.Water => waterStatusPrefab,
            StatusType.Wind => windStatusPrefab,
            StatusType.Earth => earthStatusPrefab,
            StatusType.Lightning => lightningStatusPrefab,
            _ => null
        };
    }
}
