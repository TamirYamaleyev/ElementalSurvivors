using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunDifficultyProfileSO runProfile;
    [SerializeField] private EnemyPool enemyPool;
    [SerializeField] private EnemyTierSetSO tierSet;
    [SerializeField] private StatusSystem statusSystem;
    [SerializeField] private EnemyRegistry enemyRegistry;

    [SerializeField, Range(0, 1)] private float tier2Chance = 0.25f;
    [SerializeField, Range(0, 1)] private float tier3Chance = 0.15f;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 1f;
    [Tooltip("Deprecated: on-screen disk spawn removed. Kept for scene YAML compatibility.")]
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private float spawnJitter = 0.5f;
    [Tooltip("Minimum world-space gap beyond the camera viewport edge.")]
    [SerializeField] private float viewportMargin = 1.5f;
    [Tooltip("Thickness of the offscreen spawn band outside the padded viewport.")]
    [SerializeField] private float spawnBandWidth = 3f;

    [Header("Obstacle Clearance")]
    [SerializeField] private float spawnBodyRadius = 0.35f;
    [SerializeField] private float spawnClearancePadding = 0.08f;
    [SerializeField] private int spawnClearMaxAttempts = 32;
    [Tooltip("Deprecated: clearance rings now use offscreen band samples instead.")]
    [SerializeField] private float spawnSearchRadiusExtra = 3f;

    private LayerMask obstacleMask;
    private float timer;
    private float elapsedTime;
    private int activeBossCount;
    private bool sessionComplete;
    private bool miniBoss1Spawned;
    private bool miniBoss2Spawned;
    private bool finalBossSpawned;
    private Enemy activeFinalBoss;
    private Camera spawnCamera;

    private Transform player;

    public float ElapsedTime => elapsedTime;
    public int ActiveBossCount => activeBossCount;
    public bool IsSessionComplete => sessionComplete;
    public bool IsFinalBossAlive => activeFinalBoss != null;
    public bool FinalBossWasSpawned => finalBossSpawned;

    public event Action OnSessionComplete;
    public event Action<int> OnActiveBossCountChanged;
    public event Action OnFinalBossDefeated;

    private void Awake()
    {
        obstacleMask = LayerMask.GetMask("Obstacle");
    }

    private void Start()
    {
        player = PlayerController.Instance;
        spawnCamera = Camera.main;
        timer = spawnInterval;
    }

    private void Update()
    {
        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            elapsedTime += 60f;

            if (GameTimer.Instance != null)
                GameTimer.Instance.AddTime(60f);

            Debug.Log("Dev: Skipped forward 1 minute");
        }

        if (runProfile == null || enemyPool == null || tierSet == null || player == null)
            return;

        if (spawnCamera == null)
            spawnCamera = Camera.main;

        elapsedTime += Time.deltaTime;

        TrySpawnBossMilestones();

        if (!sessionComplete && elapsedTime >= runProfile.sessionDurationSeconds)
        {
            sessionComplete = true;
            OnSessionComplete?.Invoke();
        }

        if (runProfile.stopSpawningWhenSessionEnds &&
            elapsedTime >= runProfile.sessionDurationSeconds)
            return;

        float intensity = GetSpawnIntensity();
        timer -= Time.deltaTime * intensity;

        if (timer <= 0f)
        {
            SpawnRegularEnemy(GetTierToSpawn());
            //int tierIndex = RunDifficultyEvaluator.GetPrefabIndex(runProfile, elapsedTime);
            //SpawnRegularEnemy(tierIndex);
            timer = spawnInterval;
        }
    }

    private int GetTierToSpawn()
    {
        float roll = UnityEngine.Random.value;

        bool tier2Unlocked = elapsedTime >= runProfile.tier2StartSeconds;
        bool tier3Unlocked = elapsedTime >= runProfile.tier3StartSeconds;

        if (tier3Unlocked && roll < tier3Chance)
            return 2;

        if (tier2Unlocked && roll < tier2Chance + (tier3Unlocked ? tier3Chance : 0f))
            return 1;

        return 0;
    }

    private void TrySpawnBossMilestones()
    {
        if (!miniBoss1Spawned && elapsedTime >= runProfile.tier2StartSeconds)
            miniBoss1Spawned = SpawnMiniBoss(1);

        if (!miniBoss2Spawned && elapsedTime >= runProfile.tier3StartSeconds)
            miniBoss2Spawned = SpawnMiniBoss(2);

        if (!finalBossSpawned && elapsedTime >= runProfile.sessionDurationSeconds)
            finalBossSpawned = SpawnFinalBoss();
    }

    private float GetSpawnIntensity()
    {
        if (activeBossCount <= 0)
            return 1f;

        float multiplier = runProfile.bossFightSpawnIntensityMultiplier;
        return multiplier > 0f ? multiplier : 1f;
    }

    private void SpawnRegularEnemy(int tierIndex)
    {
        Enemy prefabRef = tierSet.GetTierPrototype(tierIndex);
        if (prefabRef == null)
            return;

        Enemy instance = enemyPool.Acquire(tierIndex);
        if (instance == null)
            return;

        float visualScale = 1f;
        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        TryPlaceAndActivate(instance, prefabRef, visualScale, multiplier, trackBossEncounter: false, isFinalBoss: false);
    }

    private bool SpawnMiniBoss(int tierIndex)
    {
        Enemy prefabRef = tierSet.GetTierPrototype(tierIndex);
        if (prefabRef == null)
            return false;

        Enemy instance = enemyPool.Acquire(tierIndex);
        if (instance == null)
            return false;

        float visualScale = runProfile.miniBossVisualScale > 0f ? runProfile.miniBossVisualScale : 1.25f;
        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        multiplier *= runProfile.miniBossStatMultiplier > 0f ? runProfile.miniBossStatMultiplier : 3f;
        return TryPlaceAndActivate(instance, prefabRef, visualScale, multiplier, trackBossEncounter: true, isFinalBoss: false);
    }

    private bool SpawnFinalBoss()
    {
        Enemy prefabRef = tierSet.GetBossPrototype();
        if (prefabRef == null)
            return false;

        Enemy instance = enemyPool.AcquireBoss();
        if (instance == null)
            return false;

        float visualScale = runProfile.bossVisualScale > 0f ? runProfile.bossVisualScale : 1.5f;
        float multiplier = RunDifficultyEvaluator.GetDifficultyMultiplier(runProfile, elapsedTime);
        multiplier *= runProfile.bossExtraStatMultiplier;
        return TryPlaceAndActivate(instance, prefabRef, visualScale, multiplier, trackBossEncounter: true, isFinalBoss: true);
    }

    private bool TryPlaceAndActivate(
        Enemy instance,
        Enemy prefabRef,
        float visualScale,
        float statMultiplier,
        bool trackBossEncounter,
        bool isFinalBoss)
    {
        instance.ConfigureSystems(statusSystem, enemyRegistry);

        Vector3 desiredSpawn = EnemyOffscreenSpawnSampler.SampleOutsideViewport(
            spawnCamera,
            player.position,
            viewportMargin,
            spawnBandWidth,
            spawnJitter,
            spawnRadius);

        float checkRadius = spawnBodyRadius + spawnClearancePadding;
        if (visualScale > 0f)
            checkRadius = spawnBodyRadius * visualScale + spawnClearancePadding;

        Vector3 spawnPos = FindClearSpawnPosition(desiredSpawn, checkRadius);
        if (IsSpawnBlocked(spawnPos, checkRadius) ||
            EnemyOffscreenSpawnSampler.IsInsidePaddedViewport(spawnCamera, spawnPos, viewportMargin))
        {
            enemyPool.Release(instance);
            return false;
        }

        instance.OnAcquire(new SpawnContext
        {
            Position = spawnPos,
            ScaledMaxHealth = prefabRef.BaselineMaxHealth * statMultiplier,
            ScaledContactDamage = prefabRef.BaselineContactDamage * statMultiplier,
            VisualScaleMultiplier = visualScale,
        });

        if (!trackBossEncounter)
            return true;

        //EnemyWorldHealthBar.EnsureAttached(instance);
        TrackBossEncounter(instance, isFinalBoss);
        return true;
    }

    private void TrackBossEncounter(Enemy boss, bool isFinalBoss)
    {
        if (boss == null)
            return;

        var health = boss.GetComponent<EnemyHealth>();
        if (health == null)
            return;

        if (isFinalBoss)
            activeFinalBoss = boss;

        activeBossCount++;
        OnActiveBossCountChanged?.Invoke(activeBossCount);

        void HandleBossDied()
        {
            health.OnDied -= HandleBossDied;

            if (isFinalBoss && activeFinalBoss == boss)
            {
                activeFinalBoss = null;
                OnFinalBossDefeated?.Invoke();
            }

            activeBossCount = Mathf.Max(0, activeBossCount - 1);
            OnActiveBossCountChanged?.Invoke(activeBossCount);
        }

        health.OnDied += HandleBossDied;
    }

    private Vector3 FindClearSpawnPosition(Vector3 desired, float checkRadius)
    {
        if (!IsSpawnBlocked(desired, checkRadius) &&
            !EnemyOffscreenSpawnSampler.IsInsidePaddedViewport(spawnCamera, desired, viewportMargin))
            return desired;

        for (int i = 0; i < spawnClearMaxAttempts; i++)
        {
            Vector3 candidate = EnemyOffscreenSpawnSampler.SampleOutsideViewport(
                spawnCamera,
                player.position,
                viewportMargin,
                spawnBandWidth,
                spawnJitter,
                spawnRadius);

            if (!IsSpawnBlocked(candidate, checkRadius) &&
                !EnemyOffscreenSpawnSampler.IsInsidePaddedViewport(spawnCamera, candidate, viewportMargin))
                return candidate;
        }

        const int ringCount = 5;
        const int anglesPerRing = 12;
        for (int ring = 1; ring <= ringCount; ring++)
        {
            float bandT = ring / (float)ringCount;
            for (int a = 0; a < anglesPerRing; a++)
            {
                float angle = a * (360f / anglesPerRing) * Mathf.Deg2Rad;
                Vector3 candidate = EnemyOffscreenSpawnSampler.SampleOutsideViewportRing(
                    spawnCamera,
                    player.position,
                    viewportMargin,
                    spawnBandWidth + Mathf.Max(0f, spawnSearchRadiusExtra),
                    angle,
                    bandT,
                    spawnRadius);

                if (!IsSpawnBlocked(candidate, checkRadius) &&
                    !EnemyOffscreenSpawnSampler.IsInsidePaddedViewport(spawnCamera, candidate, viewportMargin))
                    return candidate;
            }
        }

        return desired;
    }

    private bool IsSpawnBlocked(Vector3 position, float checkRadius)
    {
        if (obstacleMask == 0)
            return false;

        return Physics2D.OverlapCircle(position, checkRadius, obstacleMask) != null;
    }
}