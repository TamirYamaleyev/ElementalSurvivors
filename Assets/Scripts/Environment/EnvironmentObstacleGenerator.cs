using System.Collections;
using UnityEngine;

/// <summary>
/// At game start, instantiates obstacle prefabs at random XY positions inside the floor
/// <see cref="SpriteRenderer.bounds"/> (with padding). Prefabs should use a non-trigger
/// <see cref="Collider2D"/> and a visible sprite (e.g. unlit solid quad) so the player can see and collide with them.
/// </summary>
public sealed class EnvironmentObstacleGenerator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer floorRenderer;
    [SerializeField] private GameObject[] obstaclePrefabs = System.Array.Empty<GameObject>();
    [SerializeField] private int spawnCount = 100;
    [SerializeField] private float edgePadding = 3f;
    [SerializeField] private float scaleMin = 0.9f;
    [SerializeField] private float scaleMax = 1.1f;
    [SerializeField] private uint randomSeed;

    [Header("Spawn region (full floor hides objects off-camera)")]
    [Tooltip("If true, only spawn inside the intersection of floor bounds and a box around the player at Start.")]
    [SerializeField] private bool limitSpawnToAreaAroundPlayer = true;
    [SerializeField] private Vector2 spawnHalfExtents = new Vector2(18f, 11f);

    [Header("Player spawn clearance")]
    [Tooltip("No obstacle center is placed closer than this radius (XY) from the player at spawn time.")]
    [SerializeField] private float playerClearRadius = 3.5f;
    [SerializeField] private int playerClearMaxAttempts = 48;

    [Header("Optional overlap rejection")]
    [SerializeField] private float minSeparation;
    [SerializeField] private float separationCheckRadius = 0.4f;
    [SerializeField] private int separationMaxAttempts = 16;
    [SerializeField] private LayerMask separationLayerMask = ~0;

    [Header("Rendering")]
    [Tooltip("Added to the floor SpriteRenderer sorting order for spawned obstacle sprites. Default keeps blocks above the player sprite.")]
    [SerializeField] private int sortingOrderOffset = 22;

    [Header("Orientation")]
    [SerializeField] private bool randomizeRotation;

    private Transform _spawnRoot;

    private void Awake()
    {
        var existing = transform.Find("SpawnedObstacles");
        if (existing != null)
        {
            _spawnRoot = existing;
            return;
        }

        var holder = new GameObject("SpawnedObstacles");
        holder.transform.SetParent(transform, false);
        _spawnRoot = holder.transform;
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        // Wait one frame so Floor/Player transforms and renderers are fully initialized (order-safe).
        yield return null;

        if (floorRenderer == null)
        {
            var floorGo = GameObject.Find("Floor");
            if (floorGo != null)
                floorRenderer = floorGo.GetComponent<SpriteRenderer>();
        }

        if (floorRenderer == null)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] Assign Floor SpriteRenderer or name an object 'Floor'.", this);
            yield break;
        }

        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] obstaclePrefabs is empty.", this);
            yield break;
        }

        var nonNullCount = 0;
        foreach (var p in obstaclePrefabs)
        {
            if (p != null)
                nonNullCount++;
        }

        if (nonNullCount == 0)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] obstaclePrefabs has no non-null entries.", this);
            yield break;
        }

        LogPrefabColliderIssues();

        if (randomSeed != 0)
            Random.InitState((int)randomSeed);
        else
            Random.InitState((int)(Time.realtimeSinceStartup * 1000f) ^ GetInstanceID());

        var bounds = floorRenderer.bounds;
        var min = bounds.min;
        var max = bounds.max;
        min.x += edgePadding;
        max.x -= edgePadding;
        min.y += edgePadding;
        max.y -= edgePadding;
        if (min.x >= max.x || min.y >= max.y)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] Floor bounds too small after padding.", this);
            yield break;
        }

        if (limitSpawnToAreaAroundPlayer && spawnHalfExtents.x > 0f && spawnHalfExtents.y > 0f)
        {
            var focus = FindPlayerPlanarPosition();
            var cxMin = focus.x - spawnHalfExtents.x;
            var cxMax = focus.x + spawnHalfExtents.x;
            var cyMin = focus.y - spawnHalfExtents.y;
            var cyMax = focus.y + spawnHalfExtents.y;
            var ixMin = Mathf.Max(min.x, cxMin);
            var ixMax = Mathf.Min(max.x, cxMax);
            var iyMin = Mathf.Max(min.y, cyMin);
            var iyMax = Mathf.Min(max.y, cyMax);
            if (ixMin < ixMax && iyMin < iyMax)
            {
                min.x = ixMin;
                max.x = ixMax;
                min.y = iyMin;
                max.y = iyMax;
            }
        }

        var z = floorRenderer.transform.position.z;
        var playerPlanar = FindPlayerPlanarPosition();

        for (var i = 0; i < spawnCount; i++)
        {
            GameObject prefab;
            do
            {
                prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            } while (prefab == null);
            var pos = RandomPointAvoidingPlayer(min, max, z, playerPlanar, playerClearRadius, playerClearMaxAttempts);
            if (minSeparation > 0f && separationCheckRadius > 0f)
            {
                for (var a = 0; a < separationMaxAttempts; a++)
                {
                    if (!Physics2D.OverlapCircle(pos, separationCheckRadius, separationLayerMask))
                        break;
                    pos = RandomPointAvoidingPlayer(min, max, z, playerPlanar, playerClearRadius, playerClearMaxAttempts);
                }
            }

            var rot = randomizeRotation ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) : Quaternion.identity;
            var inst = Instantiate(prefab, pos, rot, _spawnRoot);
            var obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer >= 0)
                inst.layer = obstacleLayer;
            var s = Random.Range(scaleMin, scaleMax);
            var ls = inst.transform.localScale;
            inst.transform.localScale = new Vector3(ls.x * s, ls.y * s, ls.z);
            AlignSpawnedInstance(inst);
        }

        var floorOrder = floorRenderer.sortingOrder;
        var obstacleOrder = Mathf.Clamp(floorOrder + sortingOrderOffset, -32768, 32767);
        var childCount = _spawnRoot != null ? _spawnRoot.childCount : 0;
        var samplePos = Vector3.zero;
        var sampleSpriteOrder = obstacleOrder;
        var sampleSpriteCount = 0;
        if (_spawnRoot != null && childCount > 0)
        {
            var first = _spawnRoot.GetChild(0);
            samplePos = first.position;
            var renderers = first.GetComponentsInChildren<SpriteRenderer>(true);
            sampleSpriteCount = renderers.Length;
            if (renderers.Length > 0)
                sampleSpriteOrder = renderers[0].sortingOrder;
        }

        Debug.Log(
            $"[EnvironmentObstacleGenerator] Spawned {spawnCount} obstacles under '{(_spawnRoot != null ? _spawnRoot.name : "?")}' " +
            $"(childCount={childCount}). Region min=({min.x:F1},{min.y:F1}) max=({max.x:F1},{max.y:F1}). " +
            $"sortLayer='{floorRenderer.sortingLayerName}' floorSortingOrder={floorOrder} obstacleSortingOrder={obstacleOrder} (floor+offset={floorOrder}+{sortingOrderOffset}). " +
            $"Verify: first child worldPos=({samplePos.x:F2},{samplePos.y:F2},{samplePos.z:F2}) spriteRenderers={sampleSpriteCount} firstSpriteSortingOrder={sampleSpriteOrder}.",
            this);
    }

    private void AlignSpawnedInstance(GameObject inst)
    {
        // Do not reassign layers to the floor's Default layer: dense solid colliders would cage the player
        // and block movement toward EXP orbs (pickup uses trigger overlap). Use prefab layer (Obstacle).

        var obstacleOrder = Mathf.Clamp(floorRenderer.sortingOrder + sortingOrderOffset, -32768, 32767);
        foreach (var r in inst.GetComponentsInChildren<SpriteRenderer>(true))
        {
            WorldSprite2DDefaults.Apply(r, obstacleOrder);
            r.sortingLayerID = floorRenderer.sortingLayerID;
            r.maskInteraction = floorRenderer.maskInteraction;
            r.renderingLayerMask = floorRenderer.renderingLayerMask;
            if (r.sprite == null)
                Debug.LogWarning($"[EnvironmentObstacleGenerator] Spawned obstacle '{inst.name}' has SpriteRenderer with no sprite (will not draw).", inst);
        }
    }

    private static Vector3 RandomPoint(Vector3 min, Vector3 max, float z)
    {
        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            z);
    }

    /// <summary>
    /// Picks a random point in the spawn AABB; if <paramref name="clearRadius"/> &gt; 0, avoids a disk around the player.
    /// </summary>
    private static Vector3 RandomPointAvoidingPlayer(
        Vector3 min,
        Vector3 max,
        float z,
        Vector2 playerPlanar,
        float clearRadius,
        int maxAttempts)
    {
        if (clearRadius <= 0f)
            return RandomPoint(min, max, z);

        var r2 = clearRadius * clearRadius;
        for (var t = 0; t < maxAttempts; t++)
        {
            var p = RandomPoint(min, max, z);
            var dx = p.x - playerPlanar.x;
            var dy = p.y - playerPlanar.y;
            if (dx * dx + dy * dy >= r2)
                return p;
        }

        return RandomPoint(min, max, z);
    }

    private static Vector2 FindPlayerPlanarPosition()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
            return Vector2.zero;
        var t = p.transform.position;
        return new Vector2(t.x, t.y);
    }

    private void LogPrefabColliderIssues()
    {
        foreach (var p in obstaclePrefabs)
        {
            if (p == null)
                continue;
            if (!PrefabHasNonTriggerCollider2D(p))
                Debug.LogWarning($"[EnvironmentObstacleGenerator] Prefab '{p.name}' has no non-trigger Collider2D (physics may be wrong).", p);
        }
    }

    private static bool PrefabHasNonTriggerCollider2D(GameObject prefab)
    {
        foreach (var c in prefab.GetComponentsInChildren<Collider2D>(true))
        {
            if (!c.isTrigger)
                return true;
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (obstaclePrefabs == null)
            return;
        foreach (var p in obstaclePrefabs)
        {
            if (p == null)
                continue;
            if (!PrefabHasNonTriggerCollider2D(p))
                Debug.LogWarning($"[EnvironmentObstacleGenerator] Prefab '{p.name}' should have a non-trigger Collider2D.", p);
        }
    }
#endif
}
