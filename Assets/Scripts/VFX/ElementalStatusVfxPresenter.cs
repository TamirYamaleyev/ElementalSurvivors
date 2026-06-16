using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one elemental particle prefab per <see cref="StatusType"/> while ref-count &gt; 0.
/// </summary>
public class ElementalStatusVfxPresenter : MonoBehaviour, IEnemyStatusVisualSink
{
    private const int SortingOrderOffset = 1;

    [SerializeField] private Transform optionalAnchor;
    [SerializeField] private Vector3 vfxLocalOffset = new(0f, -0.6f, 0f);

    [Header("Status prefabs (root with ParticleSystem)")]
    [SerializeField] private GameObject fireStatusPrefab;
    [SerializeField] private GameObject waterStatusPrefab;
    [SerializeField] private GameObject windStatusPrefab;
    [SerializeField] private GameObject earthStatusPrefab;
    [SerializeField] private GameObject lightningStatusPrefab;

    private readonly Dictionary<StatusType, int> refCounts = new();
    private readonly Dictionary<StatusType, GameObject> activeRoots = new();

    private SpriteRenderer bodySprite;
    private bool lastFlipX;

    private Transform Anchor => optionalAnchor != null ? optionalAnchor : transform;

    private void Awake()
    {
        bodySprite = Anchor.GetComponent<SpriteRenderer>();
        if (bodySprite == null)
            bodySprite = Anchor.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void LateUpdate()
    {
        if (bodySprite == null || activeRoots.Count == 0)
            return;

        if (bodySprite.flipX == lastFlipX)
            return;

        lastFlipX = bodySprite.flipX;
        SyncFlipMirror();
    }

    public void ResetForPool()
    {
        ClearAllInstances();
        lastFlipX = bodySprite != null && bodySprite.flipX;
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
        ApplySorting(instance);
        SyncFlipMirror();

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

    private void ApplySorting(GameObject vfxRoot)
    {
        if (bodySprite == null)
            return;

        foreach (var renderer in vfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            renderer.sortingLayerID = bodySprite.sortingLayerID;
            renderer.sortingOrder = bodySprite.sortingOrder + SortingOrderOffset;
        }
    }

    private void SyncFlipMirror()
    {
        if (bodySprite == null)
            return;

        var mirrorX = bodySprite.flipX ? -1f : 1f;
        foreach (var kv in activeRoots)
        {
            if (kv.Value == null)
                continue;

            var t = kv.Value.transform;
            var localScale = t.localScale;
            localScale.x = Mathf.Abs(localScale.x) * mirrorX;
            t.localScale = localScale;
        }
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
