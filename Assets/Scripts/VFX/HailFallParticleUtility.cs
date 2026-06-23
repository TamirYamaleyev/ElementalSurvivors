using UnityEngine;

/// <summary>
/// Shared hail-style falling particle setup for reaction hail and elemental status VFX.
/// </summary>
public static class HailFallParticleUtility
{
    public const float VisualAreaScale = 2f;
    public const float VisualSizeScale = 1.05f;
    public const float EmissionRateScale = 1.55f;
    public const float MaxParticlesScale = 1.5f;
    public const float VelocityScale = 1f;

    public static readonly HailFallZone ReactionZone = new(
        1.1f * VisualAreaScale,
        1.1f * VisualAreaScale,
        0.45f * VisualAreaScale,
        0.08f);

    public static readonly HailFallZone ElementStatusZone = new(
        0.74f,
        0.74f,
        0.46f,
        0.064f);

    public const float ElementStatusSizeScale = 1.5f;
    public const float ElementStatusEmissionScale = 1.35f;
    public const float ElementStatusFallSpeedScale = 0.55f;

    public readonly struct HailFallZone
    {
        public readonly float FallAreaWidth;
        public readonly float FallAreaDepth;
        public readonly float SpawnHeight;
        public readonly float SpawnVolumeHeight;

        public HailFallZone(float fallAreaWidth, float fallAreaDepth, float spawnHeight, float spawnVolumeHeight)
        {
            FallAreaWidth = fallAreaWidth;
            FallAreaDepth = fallAreaDepth;
            SpawnHeight = spawnHeight;
            SpawnVolumeHeight = spawnVolumeHeight;
        }
    }

    public static void ApplyHailFall(
        ParticleSystem ps,
        Color primary,
        Color secondary,
        HailFallZone zone,
        float sizeScale = 1f,
        float emissionScale = 1f,
        float fallSpeedScale = 1f)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 6f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f * VelocityScale, 0.45f * VelocityScale);
        main.startSize = new ParticleSystem.MinMaxCurve(
            0.035f * VisualSizeScale * sizeScale,
            0.055f * VisualSizeScale * sizeScale);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(120f * MaxParticlesScale);
        main.gravityModifier = 0f;
        ApplyRandomTwoColor(main, primary, secondary);

        var em = ps.emission;
        em.enabled = true;
        em.rateOverTime = 50f * EmissionRateScale * emissionScale;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(zone.FallAreaWidth, zone.SpawnVolumeHeight, zone.FallAreaDepth);
        shape.position = new Vector3(0f, zone.SpawnHeight, 0f);
        shape.randomDirectionAmount = 0.12f;

        var fall = fallSpeedScale;
        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelRange(-0.35f * VelocityScale, 0.35f * VelocityScale),
            VelRange(-4.5f * VelocityScale * fall, -2.8f * VelocityScale * fall),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f));

        ApplyFadeAlphaOverLife(ps);
    }

    public static void ApplyShapeZone(ParticleSystem ps, HailFallZone zone)
    {
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(zone.FallAreaWidth, zone.SpawnVolumeHeight, zone.FallAreaDepth);
        shape.position = new Vector3(0f, zone.SpawnHeight, 0f);
    }

    private static void ApplyRandomTwoColor(ParticleSystem.MainModule main, Color a, Color b)
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(a, 0f), new GradientColorKey(b, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        main.startColor = new ParticleSystem.MinMaxGradient
        {
            mode = ParticleSystemGradientMode.RandomColor,
            gradient = g
        };
    }

    private static void ApplyFadeAlphaOverLife(ParticleSystem ps)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.15f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
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
