using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one elemental particle prefab per <see cref="StatusType"/> while ref-count &gt; 0.
/// </summary>
public class ElementalStatusVfxPresenter : MonoBehaviour, IEnemyStatusVisualSink
{
    private const int SortingOrderOffset = 25;
    private const float VfxDepthBiasZ = -0.05f;

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
    private Enemy owner;
    private bool lastFlipX;

    private Transform Anchor => optionalAnchor != null ? optionalAnchor : transform;

    private void Awake()
    {
        owner = GetComponent<Enemy>();
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
        {
            Debug.LogWarning($"[ElementalStatusVfxPresenter] Missing status prefab for {type} on {name}.", this);
            return;
        }

        var parent = Anchor;
        var instance = Instantiate(prefab, parent, worldPositionStays: false);
        var offset = vfxLocalOffset;
        offset.z = VfxDepthBiasZ;
        instance.transform.SetLocalPositionAndRotation(offset, Quaternion.identity);
        instance.layer = parent.gameObject.layer;

        activeRoots[type] = instance;
        SyncFlipMirror();
        StartCoroutine(FinalizeSpawn(instance));
    }

    private IEnumerator FinalizeSpawn(GameObject instance)
    {
        PlayAllParticles(instance);
        ApplySorting(instance);

        yield return null;

        PlayAllParticles(instance);
        ApplySorting(instance);
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
        if (owner != null)
        {
            ReactionVfxSortingUtility.ApplyAboveEnemy(vfxRoot, owner, SortingOrderOffset);
            return;
        }

        if (bodySprite == null)
            return;

        foreach (var renderer in vfxRoot.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            renderer.sortingLayerID = bodySprite.sortingLayerID;
            renderer.sortingOrder = bodySprite.sortingOrder + SortingOrderOffset;
        }
    }

    private static void PlayAllParticles(GameObject vfxRoot)
    {
        foreach (var ps in vfxRoot.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
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
