using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Diff-based orchestrator: one base VFX when a single element is active; reaction VFX for all valid pairs when 2+.
/// </summary>
public class ElementalStatusVfxPresenter : MonoBehaviour, IEnemyStatusVisualSink
{
    private const int SortingOrderOffset = 25;
    private const float VfxDepthBiasZ = -0.05f;

    [SerializeField] private Transform optionalAnchor;
    [SerializeField] private Vector3 vfxLocalOffset = new(0f, -0.6f, 0f);
    [SerializeField] private ReactionVfxCatalogSO reactionCatalog;

    [Header("Status prefabs (root with ParticleSystem)")]
    [SerializeField] private GameObject fireStatusPrefab;
    [SerializeField] private GameObject waterStatusPrefab;
    [SerializeField] private GameObject windStatusPrefab;
    [SerializeField] private GameObject earthStatusPrefab;
    [SerializeField] private GameObject lightningStatusPrefab;

    private readonly Dictionary<StatusType, GameObject> baseRoots = new();
    private readonly Dictionary<StatusPair, GameObject> reactionRoots = new();

    private SpriteRenderer bodySprite;
    private Enemy owner;
    private EnemyRegistry enemyRegistry;
    private bool lastFlipX;

    private Transform Anchor => optionalAnchor != null ? optionalAnchor : transform;

    private void Awake()
    {
        owner = GetComponent<Enemy>();
        enemyRegistry = FindAnyObjectByType<EnemyRegistry>();
        bodySprite = Anchor.GetComponent<SpriteRenderer>();
        if (bodySprite == null)
            bodySprite = Anchor.GetComponentInChildren<SpriteRenderer>(true);
    }

    private void LateUpdate()
    {
        if (bodySprite == null || (baseRoots.Count == 0 && reactionRoots.Count == 0))
            return;

        if (bodySprite.flipX == lastFlipX)
            return;

        lastFlipX = bodySprite.flipX;
        SyncFlipMirror();
    }

    public void ResetForPool()
    {
        StopAllCoroutines();
        ClearAllInstances();
        lastFlipX = bodySprite != null && bodySprite.flipX;
    }

    public void RefreshStatusVisuals(StatusVfxPlan plan)
    {
        if (!isActiveAndEnabled)
            return;

        SyncBaseVisuals(plan.SoloElement);
        SyncReactionVisuals(plan.ReactionPairs);
        SyncFlipMirror();
    }

    private void SyncBaseVisuals(StatusType? soloElement)
    {
        var toRemove = new List<StatusType>();
        foreach (var kv in baseRoots)
        {
            if (!soloElement.HasValue || kv.Key != soloElement.Value)
                toRemove.Add(kv.Key);
        }

        foreach (var type in toRemove)
            DestroyRoot(baseRoots, type);

        if (!soloElement.HasValue || soloElement.Value == StatusType.None)
            return;

        var element = soloElement.Value;
        if (baseRoots.ContainsKey(element))
            return;

        var prefab = GetBasePrefab(element);
        if (prefab == null)
        {
            Debug.LogWarning($"[ElementalStatusVfxPresenter] Missing status prefab for {element} on {name}.", this);
            return;
        }

        var instance = SpawnBaseChild(prefab);
        baseRoots[element] = instance;
        ScheduleFinalizeSpawn(instance);
    }

    private void SyncReactionVisuals(StatusPair[] desiredPairs)
    {
        var desired = new HashSet<StatusPair>();
        if (desiredPairs != null)
        {
            foreach (var pair in desiredPairs)
                desired.Add(pair);
        }

        var toRemove = new List<StatusPair>();
        foreach (var kv in reactionRoots)
        {
            if (!desired.Contains(kv.Key))
                toRemove.Add(kv.Key);
        }

        foreach (var pair in toRemove)
            DestroyRoot(reactionRoots, pair);

        if (desiredPairs == null || reactionCatalog == null)
            return;

        foreach (var pair in desiredPairs)
        {
            if (reactionRoots.ContainsKey(pair))
                continue;

            var prefab = reactionCatalog.GetPrefab(pair.First, pair.Second);
            if (prefab == null)
                continue;

            var offset = vfxLocalOffset;
            offset.z = VfxDepthBiasZ;
            var instance = ReactionVfxAttachUtility.AttachToEnemy(
                prefab,
                Anchor,
                offset,
                owner,
                enemyRegistry);

            reactionRoots[pair] = instance;
            ApplySorting(instance);
        }
    }

    private GameObject SpawnBaseChild(GameObject prefab)
    {
        var instance = Instantiate(prefab, Anchor, worldPositionStays: false);
        var offset = vfxLocalOffset;
        offset.z = VfxDepthBiasZ;
        instance.transform.SetLocalPositionAndRotation(offset, Quaternion.identity);
        instance.layer = Anchor.gameObject.layer;
        return instance;
    }

    private void ScheduleFinalizeSpawn(GameObject instance)
    {
        if (isActiveAndEnabled)
            StartCoroutine(FinalizeSpawnDelayed(instance));
        else
            FinalizeSpawn(instance);
    }

    private IEnumerator FinalizeSpawnDelayed(GameObject instance)
    {
        if (instance == null)
            yield break;

        FinalizeSpawn(instance);

        yield return null;

        if (instance == null || !IsTrackedInstance(instance))
            yield break;

        FinalizeSpawn(instance);
    }

    private bool IsTrackedInstance(GameObject instance)
    {
        if (instance == null)
            return false;

        foreach (var kv in baseRoots)
        {
            if (kv.Value == instance)
                return true;
        }

        foreach (var kv in reactionRoots)
        {
            if (kv.Value == instance)
                return true;
        }

        return false;
    }

    private void FinalizeSpawn(GameObject instance)
    {
        if (instance == null)
            return;

        PlayAllParticles(instance);
        ApplySorting(instance);
    }

    private void ClearAllInstances()
    {
        foreach (var kv in baseRoots)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }

        foreach (var kv in reactionRoots)
        {
            if (kv.Value != null)
                Destroy(kv.Value);
        }

        baseRoots.Clear();
        reactionRoots.Clear();
    }

    private static void DestroyRoot<TKey>(Dictionary<TKey, GameObject> roots, TKey key)
    {
        if (!roots.TryGetValue(key, out var root))
            return;

        if (root != null)
        {
            foreach (var ps in root.GetComponentsInChildren<ParticleSystem>())
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            Destroy(root);
        }

        roots.Remove(key);
    }

    private void ApplySorting(GameObject vfxRoot)
    {
        if (vfxRoot == null)
            return;

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

    private void SyncFlipMirror()
    {
        if (bodySprite == null)
            return;

        var mirrorX = bodySprite.flipX ? -1f : 1f;
        MirrorRoots(baseRoots.Values, mirrorX);
        MirrorRoots(reactionRoots.Values, mirrorX);
    }

    private static void MirrorRoots(IEnumerable<GameObject> roots, float mirrorX)
    {
        foreach (var root in roots)
        {
            if (root == null)
                continue;

            var t = root.transform;
            var localScale = t.localScale;
            localScale.x = Mathf.Abs(localScale.x) * mirrorX;
            t.localScale = localScale;
        }
    }

    private static void PlayAllParticles(GameObject vfxRoot)
    {
        if (vfxRoot == null)
            return;

        foreach (var ps in vfxRoot.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    private GameObject GetBasePrefab(StatusType type)
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
