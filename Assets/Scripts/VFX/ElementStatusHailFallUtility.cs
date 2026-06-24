using UnityEngine;

/// <summary>
/// Hail-fall particle setup for elemental status VFX only. Not used by reaction burst VFX.
/// </summary>
public static class ElementStatusHailFallUtility
{
    public const float VisualSizeScale = 1.05f;
    public const float EmissionRateScale = 1.55f;
    public const float MaxParticlesScale = 1.5f;
    public const float VelocityScale = 1f;

    public const float SizeScale = 1.5f;
    public const float EmissionScale = 1.35f;
    public const float FallSpeedScale = 0.32f;
    public const float StartSpeedScale = 0.5f;
    public const float HorizontalDriftScale = 0.55f;

    public const float ParticleCountScale = 0.15f;
    public const float ParticleSizeScale = 0.75f;

    static readonly HailFallZone StatusZone = new(0.74f, 0.74f, 0.46f, 0.064f);

    readonly struct HailFallZone
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

    public static void Apply(ParticleSystem ps, Color primary, Color secondary)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 6f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.72f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(
            0.15f * VelocityScale * StartSpeedScale,
            0.45f * VelocityScale * StartSpeedScale);
        main.startSize = new ParticleSystem.MinMaxCurve(
            0.035f * VisualSizeScale * SizeScale * ParticleSizeScale,
            0.055f * VisualSizeScale * SizeScale * ParticleSizeScale);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(120f * MaxParticlesScale * ParticleCountScale);
        main.gravityModifier = 0f;
        ApplyRandomTwoColor(main, primary, secondary);

        var em = ps.emission;
        em.enabled = true;
        em.rateOverTime = 50f * EmissionRateScale * EmissionScale * ParticleCountScale;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(StatusZone.FallAreaWidth, StatusZone.SpawnVolumeHeight, StatusZone.FallAreaDepth);
        shape.position = new Vector3(0f, StatusZone.SpawnHeight, 0f);
        shape.randomDirectionAmount = 0.12f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = VelRange(
            -0.35f * VelocityScale * HorizontalDriftScale,
            0.35f * VelocityScale * HorizontalDriftScale);
        vel.y = VelRange(-4.5f * VelocityScale * FallSpeedScale, -2.8f * VelocityScale * FallSpeedScale);
        vel.z = VelConst(0f);
        vel.orbitalX = VelConst(0f);
        vel.orbitalY = VelConst(0f);
        vel.orbitalZ = VelConst(0f);
        vel.radial = VelConst(0f);

        ApplyFadeAlphaOverLife(ps);
    }

    static void ApplyRandomTwoColor(ParticleSystem.MainModule main, Color a, Color b)
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

    static void ApplyFadeAlphaOverLife(ParticleSystem ps)
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

    static ParticleSystem.MinMaxCurve VelConst(float value) => new(value, value);

    static ParticleSystem.MinMaxCurve VelRange(float min, float max) => new(min, max);
}
