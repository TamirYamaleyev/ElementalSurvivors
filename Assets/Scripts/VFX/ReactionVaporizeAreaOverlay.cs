using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns per-enemy steam overlays on all enemies inside the vaporize zone while active.
/// </summary>
[DisallowMultipleComponent]
public sealed class ReactionVaporizeAreaOverlay : MonoBehaviour, IReactionWorldVfx
{
    [SerializeField] private float effectRadius = 1.25f;
    [SerializeField] private float duration = 4f;
    [SerializeField] private float refreshInterval = 0.15f;
    [SerializeField] private int sortingOrderOffset = 30;

    private readonly List<Enemy> scratchTargets = new();
    private readonly Dictionary<Enemy, GameObject> activeOverlays = new();

    private Vector3 center;
    private EnemyRegistry registry;
    private float elapsed;
    private float refreshTimer;
    private bool initialized;

    public void Configure(float effectDuration, float radius)
    {
        duration = Mathf.Max(0.1f, effectDuration);
        effectRadius = Mathf.Max(0.1f, radius);
    }

    public void Initialize(ReactionVfxContext ctx)
    {
        center = ctx.Center;
        registry = ctx.Registry;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (!initialized)
            return;

        transform.position = center;
    }

    private void Update()
    {
        if (!initialized)
            return;

        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            ClearAllOverlays();
            enabled = false;
            return;
        }

        refreshTimer += Time.deltaTime;
        if (refreshTimer < refreshInterval)
            return;

        refreshTimer = 0f;
        RefreshOverlays();
    }

    private void OnDestroy()
    {
        ClearAllOverlays();
    }

    private void RefreshOverlays()
    {
        registry = ReactionGameplayEffectUtility.ResolveRegistry(registry);
        if (registry == null)
            return;

        ReactionAreaVfxUtility.CollectEnemiesInRadius(registry, center, effectRadius, scratchTargets);

        var inside = new HashSet<Enemy>(scratchTargets);

        var toRemove = new List<Enemy>();
        foreach (var pair in activeOverlays)
        {
            if (pair.Key == null || !inside.Contains(pair.Key))
                toRemove.Add(pair.Key);
        }

        foreach (var enemy in toRemove)
            RemoveOverlay(enemy);

        foreach (var enemy in scratchTargets)
        {
            if (activeOverlays.ContainsKey(enemy))
                continue;

            activeOverlays[enemy] = CreateEnemySteam(enemy);
        }
    }

    private void RemoveOverlay(Enemy enemy)
    {
        if (!activeOverlays.TryGetValue(enemy, out var overlay))
            return;

        if (overlay != null)
            Destroy(overlay);

        activeOverlays.Remove(enemy);
    }

    private void ClearAllOverlays()
    {
        foreach (var overlay in activeOverlays.Values)
        {
            if (overlay != null)
                Destroy(overlay);
        }

        activeOverlays.Clear();
    }

    private GameObject CreateEnemySteam(Enemy enemy)
    {
        var go = new GameObject("VaporizeEnemySteam");
        go.transform.SetParent(enemy.transform, false);
        go.transform.localPosition = Vector3.up * 0.25f;

        var ps = go.AddComponent<ParticleSystem>();
        ConfigureMiniSteam(ps);
        ps.Play(true);

        ReactionVfxSortingUtility.ApplyAboveEnemy(go, enemy, sortingOrderOffset);
        return go;
    }

    private static void ConfigureMiniSteam(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 6f;
        main.startLifetime = 0.92f;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.14f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 40;
        main.gravityModifier = 0f;
        main.startColor = new Color(0.92f, 0.95f, 1f, 0.7f);

        var em = ps.emission;
        em.rateOverTime = 18f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.18f;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelRange(0.4f, 0.9f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelRange(0.1f, 0.25f));

        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Circle);
    }

    private static ParticleSystem.MinMaxCurve VelConst(float value)
    {
        return new ParticleSystem.MinMaxCurve(value, value);
    }

    private static ParticleSystem.MinMaxCurve VelRange(float min, float max)
    {
        return new ParticleSystem.MinMaxCurve(min, max);
    }

    private static void ApplyVelocityOverLifetime(
        ParticleSystem.VelocityOverLifetimeModule vel,
        ParticleSystem.MinMaxCurve x,
        ParticleSystem.MinMaxCurve y,
        ParticleSystem.MinMaxCurve z,
        ParticleSystem.MinMaxCurve orbitalX,
        ParticleSystem.MinMaxCurve orbitalY,
        ParticleSystem.MinMaxCurve orbitalZ,
        ParticleSystem.MinMaxCurve radial)
    {
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = x;
        vel.y = y;
        vel.z = z;
        vel.orbitalX = orbitalX;
        vel.orbitalY = orbitalY;
        vel.orbitalZ = orbitalZ;
        vel.radial = radial;
    }
}
