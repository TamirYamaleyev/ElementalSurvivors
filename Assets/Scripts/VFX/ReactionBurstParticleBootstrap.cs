using UnityEngine;

/// <summary>
/// One-shot or short world burst for elemental pair reactions. Kind is set per prefab in editor / builder.
/// </summary>
[DisallowMultipleComponent]
public class ReactionBurstParticleBootstrap : MonoBehaviour
{
    private const float VisualAreaScale = 2f;
    private const float VisualSizeScale = 1.75f;
    private const float EmissionRateScale = 1.55f;
    private const float MaxParticlesScale = 1.5f;
    private const float VelocityScale = 1f;

    private static readonly Color Sandy = new(0.76f, 0.64f, 0.42f);
    private static readonly Color NearBlack = new(0.04f, 0.04f, 0.06f);

    public enum ReactionBurstKind
    {
        Vaporize = 0,
        Crystallize = 1,
        ScorchingWind = 2,
        Explosion = 3,
        Growth = 4,
        Hail = 5,
        Electrowetting = 6,
        DustSandStorm = 7,
        Magnetism = 8,
        StaticCharge = 9
    }

    [SerializeField] private ReactionBurstKind kind;
    [SerializeField] private Sprite particleSprite;

    private void Awake()
    {
        var life = GetComponent<ReactionBurstLifetime>();
        if (life == null)
            life = gameObject.AddComponent<ReactionBurstLifetime>();

        var ps = GetComponent<ParticleSystem>();
        if (ps == null)
            ps = gameObject.AddComponent<ParticleSystem>();

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);

        switch (kind)
        {
            case ReactionBurstKind.Vaporize:
                life.SetDestroyAfter(2.4f);
                ConfigureVaporize(ps);
                break;
            case ReactionBurstKind.Crystallize:
                life.SetDestroyAfter(2.4f);
                ConfigureCrystallize(ps);
                break;
            case ReactionBurstKind.ScorchingWind:
                life.SetDestroyAfter(2.4f);
                ConfigureScorchingWind(ps);
                break;
            case ReactionBurstKind.Explosion:
                life.SetDestroyAfter(0.85f);
                ConfigureExplosion(ps);
                break;
            case ReactionBurstKind.Growth:
                life.SetDestroyAfter(2.4f);
                ConfigureGrowth(ps);
                break;
            case ReactionBurstKind.Hail:
                life.SetDestroyAfter(2.4f);
                ConfigureHail(ps);
                break;
            case ReactionBurstKind.Electrowetting:
                life.SetDestroyAfter(1.6f);
                ConfigureElectrowetting(ps);
                break;
            case ReactionBurstKind.DustSandStorm:
                life.SetDestroyAfter(2.6f);
                ConfigureDustSandStorm(ps);
                break;
            case ReactionBurstKind.Magnetism:
                life.SetDestroyAfter(2.6f);
                ConfigureMagnetism(ps);
                break;
            case ReactionBurstKind.StaticCharge:
                life.SetDestroyAfter(1.8f);
                ConfigureStaticCharge(ps);
                break;
        }

        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, particleSprite);
    }

    private static void ApplyCommonLoop(ParticleSystem ps, float lifetime, float rate, float startSize, int maxParticles)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 6f;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.startSize = startSize;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = maxParticles;
        main.gravityModifier = 0f;

        var em = ps.emission;
        em.rateOverTime = rate * EmissionRateScale;
    }

    private static void RandomTwoColor(ParticleSystem.MainModule main, Color a, Color b)
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

    private static void EnableOrbitalRing(ParticleSystem ps, float orbitalZ, float radial, float radius)
    {
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius * VisualAreaScale;
        shape.position = Vector3.zero;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.y = new ParticleSystem.MinMaxCurve(0f);
        vel.z = new ParticleSystem.MinMaxCurve(0f);
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(orbitalZ * VelocityScale);
        vel.radial = new ParticleSystem.MinMaxCurve(radial * VelocityScale);
    }

    private static void ConfigureVaporize(ParticleSystem ps)
    {
        var c = new Color(0.96f, 0.96f, 1f);
        ApplyCommonLoop(ps, 0.52f, 32f, 0.11f * VisualSizeScale, Mathf.RoundToInt(100f * MaxParticlesScale));
        var main = ps.main;
        main.startColor = c;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.55f * VisualAreaScale, 0.12f, 0.18f);
        shape.position = new Vector3(0f, -0.22f * VisualAreaScale, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.y = new ParticleSystem.MinMaxCurve(2.1f * VelocityScale);
        vel.z = new ParticleSystem.MinMaxCurve(0f);
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        vel.radial = new ParticleSystem.MinMaxCurve(0f);

        SolidColorOverLife(ps, c);
    }

    private static void ConfigureCrystallize(ParticleSystem ps)
    {
        var c = new Color(0.2f, 0.45f, 1f);
        ApplyCommonLoop(ps, 0.55f, 36f, 0.1f * VisualSizeScale, Mathf.RoundToInt(110f * MaxParticlesScale));
        var main = ps.main;
        main.startColor = c;
        EnableOrbitalRing(ps, -4.2f, 0.12f, 0.26f);
        SolidColorOverLife(ps, c);
    }

    private static void ConfigureScorchingWind(ParticleSystem ps)
    {
        ApplyCommonLoop(ps, 0.5f, 38f, 0.095f * VisualSizeScale, Mathf.RoundToInt(120f * MaxParticlesScale));
        var main = ps.main;
        RandomTwoColor(main, Color.white, new Color(1f, 0.2f, 0.12f));
        EnableOrbitalRing(ps, -5.2f, 0.18f, 0.28f);
        DisableColorOverLife(ps);
    }

    private static void ConfigureExplosion(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.35f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.32f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.2f * VelocityScale, 5.5f * VelocityScale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.12f * VisualSizeScale, 0.22f * VisualSizeScale);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(220f * MaxParticlesScale);
        main.gravityModifier = 0f;
        RandomTwoColor(main, new Color(1f, 0.15f, 0.08f), new Color(1f, 0.92f, 0.2f));

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 90, 110) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.08f * VisualAreaScale;
        shape.randomDirectionAmount = 0.35f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;

        DisableColorOverLife(ps);
    }

    private static void ConfigureGrowth(ParticleSystem ps)
    {
        var c = new Color(0.12f, 0.82f, 0.32f);
        ApplyCommonLoop(ps, 0.55f, 30f, 0.1f * VisualSizeScale, Mathf.RoundToInt(100f * MaxParticlesScale));
        var main = ps.main;
        main.startColor = c;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f * VisualAreaScale, 0.1f, 0.16f);
        shape.position = new Vector3(0f, -0.24f * VisualAreaScale, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.y = new ParticleSystem.MinMaxCurve(1.9f * VelocityScale);
        vel.z = new ParticleSystem.MinMaxCurve(0f);
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f);
        vel.radial = new ParticleSystem.MinMaxCurve(0f);

        SolidColorOverLife(ps, c);
    }

    private static void ConfigureHail(ParticleSystem ps)
    {
        var c = new Color(0.25f, 0.55f, 1f);
        ApplyCommonLoop(ps, 0.48f, 40f, 0.09f * VisualSizeScale, Mathf.RoundToInt(130f * MaxParticlesScale));
        var main = ps.main;
        main.startColor = c;
        EnableOrbitalRing(ps, 5.5f, -0.08f, 0.24f);
        SolidColorOverLife(ps, c);
    }

    private static void ConfigureElectrowetting(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.45f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f * VelocityScale, 4.8f * VelocityScale);
        main.startSize = 0.1f * VisualSizeScale;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(160f * MaxParticlesScale);
        main.gravityModifier = 0f;
        RandomTwoColor(main, new Color(0.2f, 0.45f, 1f), new Color(1f, 0.9f, 0.25f));

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 70, 95) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.06f * VisualAreaScale;
        shape.randomDirectionAmount = 1f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;
        DisableColorOverLife(ps);
    }

    private static void ConfigureDustSandStorm(ParticleSystem ps)
    {
        var c = Sandy;
        ApplyCommonLoop(ps, 0.6f, 34f, 0.11f * VisualSizeScale, Mathf.RoundToInt(115f * MaxParticlesScale));
        var main = ps.main;
        main.startColor = c;
        EnableOrbitalRing(ps, -3.4f, 0.22f, 0.32f);
        SolidColorOverLife(ps, c);
    }

    private static void ConfigureMagnetism(ParticleSystem ps)
    {
        ApplyCommonLoop(ps, 0.5f, 42f, 0.088f * VisualSizeScale, Mathf.RoundToInt(140f * MaxParticlesScale));
        var main = ps.main;
        RandomTwoColor(main, NearBlack, new Color(0.15f, 0.35f, 1f));
        EnableOrbitalRing(ps, -6.2f, 0.1f, 0.27f);

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        var sz = new AnimationCurve(
            new Keyframe(0f, 0.65f),
            new Keyframe(0.25f, 1.15f),
            new Keyframe(0.5f, 0.7f),
            new Keyframe(0.75f, 1.05f),
            new Keyframe(1f, 0.65f)
        );
        sol.size = new ParticleSystem.MinMaxCurve(1f, sz);

        DisableColorOverLife(ps);
    }

    private static void ConfigureStaticCharge(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.4f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.15f, 0.38f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f * VelocityScale, 5f * VelocityScale);
        main.startSize = 0.08f * VisualSizeScale;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(180f * MaxParticlesScale);
        main.gravityModifier = 0f;
        RandomTwoColor(main, Sandy, Color.white);

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 55, 85) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.05f * VisualAreaScale;
        shape.randomDirectionAmount = 0.92f;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;
        DisableColorOverLife(ps);
    }

    private static void SolidColorOverLife(ParticleSystem ps, Color color)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    private static void DisableColorOverLife(ParticleSystem ps)
    {
        var col = ps.colorOverLifetime;
        col.enabled = false;
    }
}
