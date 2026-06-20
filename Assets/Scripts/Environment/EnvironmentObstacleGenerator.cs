using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Streams obstacle prefabs in a grid around the player. Cells enter an active window as the player
/// moves and return to an object pool when they leave the despawn window.
/// </summary>
public sealed class EnvironmentObstacleGenerator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer floorRenderer;
    [SerializeField] private GameObject[] obstaclePrefabs = System.Array.Empty<GameObject>();
    [SerializeField] private uint randomSeed;

    [Header("Streaming grid")]
    [SerializeField] private float cellSize = 16f;
    [Tooltip("Obstacles are kept while the cell overlaps this box around the player.")]
    [SerializeField] private Vector2 activeHalfExtents = new(24f, 14f);
    [Tooltip("Cells outside this box are returned to the pool (should be larger than activeHalfExtents).")]
    [SerializeField] private Vector2 despawnHalfExtents = new(32f, 20f);
    [SerializeField] private int obstaclesPerCell = 4;
    [SerializeField] private float streamRefreshInterval = 0.2f;

    [Header("Floor")]
    [SerializeField] private float edgePadding = 3f;

    [Header("Placement")]
    [SerializeField] private float scaleMin = 0.9f;
    [SerializeField] private float scaleMax = 1.1f;
    [SerializeField] private float playerClearRadius = 3.5f;
    [SerializeField] private int playerClearMaxAttempts = 48;

    [Header("Optional overlap rejection")]
    [SerializeField] private float minSeparation;
    [SerializeField] private float separationCheckRadius = 0.4f;
    [SerializeField] private int separationMaxAttempts = 16;
    [SerializeField] private LayerMask separationLayerMask = ~0;

    [Header("Rendering")]
    [SerializeField] private int sortingOrderOffset = 22;

    [Header("Orientation")]
    [SerializeField] private bool randomizeRotation;

    [Header("Pool")]
    [SerializeField] private int poolPrewarmPerPrefab = 8;

    private readonly Dictionary<Vector2Int, List<GameObject>> activeCells = new();
    private readonly List<Stack<GameObject>> pools = new();

    private Transform _activeRoot;
    private Transform _poolRoot;
    private Transform _player;
    private Vector3 _floorMin;
    private Vector3 _floorMax;
    private float _floorZ;
    private int _obstacleSortOrder;
    private float _streamTimer;
    private bool _initialized;
    private int _baseSeed;

    private void Awake()
    {
        _activeRoot = GetOrCreateChild("SpawnedObstacles");
        _poolRoot = GetOrCreateChild("ObstaclePool");
    }

    private Transform GetOrCreateChild(string name)
    {
        var existing = transform.Find(name);
        if (existing != null)
            return existing;

        var holder = new GameObject(name);
        holder.transform.SetParent(transform, false);
        return holder.transform;
    }

    private void Start()
    {
        if (!TryInitialize())
            return;

        PrewarmPool();
        RefreshStreaming(force: true);
    }

    private void Update()
    {
        if (!_initialized || _player == null)
            return;

        _streamTimer -= Time.deltaTime;
        if (_streamTimer > 0f)
            return;

        _streamTimer = streamRefreshInterval;
        RefreshStreaming(force: false);
    }

    private bool TryInitialize()
    {
        if (floorRenderer == null)
        {
            var floorGo = GameObject.Find("Floor");
            if (floorGo != null)
                floorRenderer = floorGo.GetComponent<SpriteRenderer>();
        }

        if (floorRenderer == null)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] Assign Floor SpriteRenderer or name an object 'Floor'.", this);
            return false;
        }

        if (!HasValidPrefabs())
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] obstaclePrefabs is empty.", this);
            return false;
        }

        LogPrefabColliderIssues();

        _player = PlayerController.Instance;
        if (_player == null)
        {
            var playerGo = GameObject.FindGameObjectWithTag("Player");
            if (playerGo != null)
                _player = playerGo.transform;
        }

        if (_player == null)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] Player transform not found.", this);
            return false;
        }

        var bounds = floorRenderer.bounds;
        _floorMin = bounds.min;
        _floorMax = bounds.max;
        _floorMin.x += edgePadding;
        _floorMax.x -= edgePadding;
        _floorMin.y += edgePadding;
        _floorMax.y -= edgePadding;
        if (_floorMin.x >= _floorMax.x || _floorMin.y >= _floorMax.y)
        {
            Debug.LogWarning("[EnvironmentObstacleGenerator] Floor bounds too small after padding.", this);
            return false;
        }

        _floorZ = floorRenderer.transform.position.z;
        _obstacleSortOrder = Mathf.Clamp(floorRenderer.sortingOrder + sortingOrderOffset, -32768, 32767);
        _baseSeed = randomSeed != 0
            ? (int)randomSeed
            : (int)(Time.realtimeSinceStartup * 1000f) ^ GetInstanceID();

        _initialized = true;
        return true;
    }

    private void RefreshStreaming(bool force)
    {
        var playerPlanar = (Vector2)_player.position;
        UnloadDistantCells(playerPlanar);
        LoadNearbyCells(playerPlanar);

        if (force)
        {
            Debug.Log(
                $"[EnvironmentObstacleGenerator] Streaming initialized. activeCells={activeCells.Count} pooled={CountPooled()} " +
                $"cellSize={cellSize} activeHalf=({activeHalfExtents.x},{activeHalfExtents.y}) " +
                $"despawnHalf=({despawnHalfExtents.x},{despawnHalfExtents.y}) perCell={obstaclesPerCell}.",
                this);
        }
    }

    private void UnloadDistantCells(Vector2 playerPlanar)
    {
        cellsToUnload.Clear();
        foreach (var pair in activeCells)
        {
            if (!CellOverlapsBox(pair.Key, playerPlanar, despawnHalfExtents))
                cellsToUnload.Add(pair.Key);
        }

        for (var i = 0; i < cellsToUnload.Count; i++)
            UnloadCell(cellsToUnload[i]);
    }

    private void LoadNearbyCells(Vector2 playerPlanar)
    {
        if (cellSize <= 0f || obstaclesPerCell <= 0)
            return;

        var minCellX = WorldToCellIndex(playerPlanar.x - activeHalfExtents.x);
        var maxCellX = WorldToCellIndex(playerPlanar.x + activeHalfExtents.x);
        var minCellY = WorldToCellIndex(playerPlanar.y - activeHalfExtents.y);
        var maxCellY = WorldToCellIndex(playerPlanar.y + activeHalfExtents.y);

        for (var cx = minCellX; cx <= maxCellX; cx++)
        {
            for (var cy = minCellY; cy <= maxCellY; cy++)
            {
                var coord = new Vector2Int(cx, cy);
                if (activeCells.ContainsKey(coord))
                    continue;

                if (!CellIntersectsFloor(coord))
                    continue;

                LoadCell(coord, playerPlanar);
            }
        }
    }

    private readonly List<Vector2Int> cellsToUnload = new();

    private void LoadCell(Vector2Int cell, Vector2 playerPlanar)
    {
        if (!TryGetCellBounds(cell, out var min, out var max))
            return;

        var instances = new List<GameObject>(obstaclesPerCell);
        var cellSeed = _baseSeed ^ (cell.x * 73856093) ^ (cell.y * 19349663);

        for (var i = 0; i < obstaclesPerCell; i++)
        {
            var prefabIndex = PickPrefabIndex(cellSeed + i * 17);
            var prefab = obstaclePrefabs[prefabIndex];
            var pos = RandomPointAvoidingPlayer(min, max, _floorZ, playerPlanar, playerClearRadius, playerClearMaxAttempts);
            if (minSeparation > 0f && separationCheckRadius > 0f)
            {
                for (var a = 0; a < separationMaxAttempts; a++)
                {
                    if (!Physics2D.OverlapCircle(pos, separationCheckRadius, separationLayerMask))
                        break;
                    pos = RandomPointAvoidingPlayer(min, max, _floorZ, playerPlanar, playerClearRadius, playerClearMaxAttempts);
                }
            }

            var rot = randomizeRotation ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)) : Quaternion.identity;
            var inst = Acquire(prefab, prefabIndex);
            inst.transform.SetPositionAndRotation(pos, rot);

            var s = Random.Range(scaleMin, scaleMax);
            var pooled = inst.GetComponent<PooledObstacle>();
            var baseScale = pooled != null ? pooled.BaseScale : inst.transform.localScale;
            inst.transform.localScale = new Vector3(baseScale.x * s, baseScale.y * s, baseScale.z);

            AlignSpawnedInstance(inst);
            instances.Add(inst);
        }

        if (instances.Count > 0)
            activeCells[cell] = instances;
    }

    private void UnloadCell(Vector2Int cell)
    {
        if (!activeCells.TryGetValue(cell, out var instances))
            return;

        for (var i = 0; i < instances.Count; i++)
            Release(instances[i]);

        activeCells.Remove(cell);
    }

    private GameObject Acquire(GameObject prefab, int prefabIndex)
    {
        EnsurePoolSlots();
        var stack = pools[prefabIndex];
        GameObject inst;
        if (stack.Count > 0)
        {
            inst = stack.Pop();
            inst.SetActive(true);
            inst.transform.SetParent(_activeRoot, false);
        }
        else
        {
            inst = Instantiate(prefab, _activeRoot);
            var obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer >= 0)
                inst.layer = obstacleLayer;

            var pooled = inst.GetComponent<PooledObstacle>() ?? inst.AddComponent<PooledObstacle>();
            pooled.Initialize(prefabIndex, inst.transform.localScale);
        }

        return inst;
    }

    private void Release(GameObject inst)
    {
        if (inst == null)
            return;

        var pooled = inst.GetComponent<PooledObstacle>();
        if (pooled == null)
        {
            Destroy(inst);
            return;
        }

        inst.SetActive(false);
        inst.transform.SetParent(_poolRoot, false);
        EnsurePoolSlots();
        pools[pooled.PrefabIndex].Push(inst);
    }

    private void PrewarmPool()
    {
        if (poolPrewarmPerPrefab <= 0 || !HasValidPrefabs())
            return;

        EnsurePoolSlots();
        for (var i = 0; i < obstaclePrefabs.Length; i++)
        {
            var prefab = obstaclePrefabs[i];
            if (prefab == null)
                continue;

            for (var n = 0; n < poolPrewarmPerPrefab; n++)
            {
                var inst = Instantiate(prefab, _poolRoot);
                inst.SetActive(false);
                var obstacleLayer = LayerMask.NameToLayer("Obstacle");
                if (obstacleLayer >= 0)
                    inst.layer = obstacleLayer;
                var pooled = inst.GetComponent<PooledObstacle>() ?? inst.AddComponent<PooledObstacle>();
                pooled.Initialize(i, inst.transform.localScale);
                pools[i].Push(inst);
            }
        }
    }

    private void EnsurePoolSlots()
    {
        while (pools.Count < obstaclePrefabs.Length)
            pools.Add(new Stack<GameObject>());
    }

    private int PickPrefabIndex(int rollSeed)
    {
        Random.InitState(rollSeed);

        var validCount = 0;
        for (var i = 0; i < obstaclePrefabs.Length; i++)
        {
            if (obstaclePrefabs[i] != null)
                validCount++;
        }

        if (validCount <= 0)
            return 0;

        var pick = Random.Range(0, validCount);
        for (var i = 0; i < obstaclePrefabs.Length; i++)
        {
            if (obstaclePrefabs[i] == null)
                continue;
            if (pick == 0)
                return i;
            pick--;
        }

        return 0;
    }

    private int CountPooled()
    {
        var total = 0;
        for (var i = 0; i < pools.Count; i++)
            total += pools[i].Count;
        return total;
    }

    private bool CellIntersectsFloor(Vector2Int cell)
    {
        if (!TryGetCellBounds(cell, out var min, out var max))
            return false;

        return min.x < _floorMax.x && max.x > _floorMin.x && min.y < _floorMax.y && max.y > _floorMin.y;
    }

    private bool CellOverlapsBox(Vector2Int cell, Vector2 focus, Vector2 halfExtents)
    {
        if (!TryGetCellBounds(cell, out var min, out var max))
            return false;

        var boxMinX = focus.x - halfExtents.x;
        var boxMaxX = focus.x + halfExtents.x;
        var boxMinY = focus.y - halfExtents.y;
        var boxMaxY = focus.y + halfExtents.y;
        return min.x < boxMaxX && max.x > boxMinX && min.y < boxMaxY && max.y > boxMinY;
    }

    private bool TryGetCellBounds(Vector2Int cell, out Vector3 min, out Vector3 max)
    {
        var worldMinX = cell.x * cellSize;
        var worldMinY = cell.y * cellSize;
        var worldMaxX = worldMinX + cellSize;
        var worldMaxY = worldMinY + cellSize;

        min = new Vector3(
            Mathf.Max(worldMinX, _floorMin.x),
            Mathf.Max(worldMinY, _floorMin.y),
            _floorMin.z);
        max = new Vector3(
            Mathf.Min(worldMaxX, _floorMax.x),
            Mathf.Min(worldMaxY, _floorMax.y),
            _floorMax.z);

        return min.x < max.x && min.y < max.y;
    }

    private int WorldToCellIndex(float worldCoord) => Mathf.FloorToInt(worldCoord / cellSize);

    private void AlignSpawnedInstance(GameObject inst)
    {
        foreach (var r in inst.GetComponentsInChildren<SpriteRenderer>(true))
        {
            WorldSprite2DDefaults.Apply(r, _obstacleSortOrder);
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

    private bool HasValidPrefabs()
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0)
            return false;

        foreach (var p in obstaclePrefabs)
        {
            if (p != null)
                return true;
        }

        return false;
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
        if (despawnHalfExtents.x < activeHalfExtents.x)
            despawnHalfExtents.x = activeHalfExtents.x;
        if (despawnHalfExtents.y < activeHalfExtents.y)
            despawnHalfExtents.y = activeHalfExtents.y;

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

    private sealed class PooledObstacle : MonoBehaviour
    {
        public int PrefabIndex { get; private set; }
        public Vector3 BaseScale { get; private set; }

        public void Initialize(int prefabIndex, Vector3 scale)
        {
            PrefabIndex = prefabIndex;
            BaseScale = scale;
        }
    }
}
