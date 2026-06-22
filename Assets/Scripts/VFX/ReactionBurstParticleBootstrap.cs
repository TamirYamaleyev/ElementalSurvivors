using UnityEngine;

/// <summary>
/// One-shot or short world burst for elemental pair reactions. Kind is set per prefab in editor / builder.
/// </summary>
[DisallowMultipleComponent]
public class ReactionBurstParticleBootstrap : MonoBehaviour
{
    private const float VisualAreaScale = 2f;
    private const float VisualSizeScale = 1.05f;
    private const float FineParticleScale = 0.7f;
    private const float EmissionRateScale = 1.55f;
    private const float MaxParticlesScale = 1.5f;
    private const float VelocityScale = 1f;

    private static readonly Color Sandy = new(0.76f, 0.64f, 0.42f);
    private static readonly Color SteamWhite = new(0.96f, 0.97f, 1f);
    private static readonly Color SteamBlueGray = new(0.85f, 0.89f, 0.94f);

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
                life.SetDestroyAfter(0.5f);
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
                life.SetDestroyAfter(0.35f);
                ConfigureElectrowetting(ps);
                break;
            case ReactionBurstKind.DustSandStorm:
                life.SetDestroyAfter(2.6f);
                ConfigureDustSandStorm(ps);
                break;
            case ReactionBurstKind.Magnetism:
                life.SetDestroyAfter(1.2f);
                ConfigureMagnetism(ps);
                break;
            case ReactionBurstKind.StaticCharge:
                life.SetDestroyAfter(1.8f);
                ConfigureStaticCharge(ps);
                break;
        }

        if (kind != ReactionBurstKind.StaticCharge && kind != ReactionBurstKind.ScorchingWind)
            ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.GetReactionShape(kind));
    }

    private static void ApplyCommonLoop(ParticleSystem ps, float lifetime, float rate, float startSize, int maxParticles)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(orbitalZ * VelocityScale),
            VelConst(radial * VelocityScale));
    }

    private static void ConfigureVaporize(ParticleSystem ps)
    {
        ConfigureSteamCore(ps);
        ConfigureSteamEscape(CreateCoreChild(ps.transform, "SteamEscape"));
    }

    private static void ConfigureSteamCore(ParticleSystem ps)
    {
        ApplyCommonLoop(
            ps,
            0.8f,
            62f,
            new ParticleSystem.MinMaxCurve(0.12f * VisualSizeScale, 0.18f * VisualSizeScale),
            Mathf.RoundToInt(200f * MaxParticlesScale));

        var main = ps.main;
        RandomTwoColor(main, SteamWhite, SteamBlueGray);

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.62f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelRange(0.6f * VelocityScale, 1f * VelocityScale),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelRange(0.15f * VelocityScale, 0.35f * VelocityScale));

        SteamAlphaOverLife(ps, 0.55f, 0.25f);
    }

    private static void ConfigureSteamEscape(Transform escapeTransform)
    {
        var ps = escapeTransform.gameObject.AddComponent<ParticleSystem>();

        ApplyCommonLoop(
            ps,
            1.05f,
            34f,
            new ParticleSystem.MinMaxCurve(0.06f * VisualSizeScale, 0.11f * VisualSizeScale),
            Mathf.RoundToInt(120f * MaxParticlesScale));

        var main = ps.main;
        RandomTwoColor(main, SteamWhite, SteamBlueGray);

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.68f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelRange(-0.4f * VelocityScale, 0.4f * VelocityScale),
            VelRange(1.8f * VelocityScale, 2.8f * VelocityScale),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelRange(0.8f * VelocityScale, 1.4f * VelocityScale));

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 1.6f)
        ));

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.15f;
        noise.frequency = 0.35f;
        noise.scrollSpeed = 0.4f;

        SteamAlphaOverLife(ps, 0.45f, 0f);
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Circle);
    }

    private static void ConfigureCrystallize(ParticleSystem ps)
    {
        var c = new Color(0.2f, 0.45f, 1f);
        ApplyCommonLoop(ps, 0.55f, 36f, 0.1f * VisualSizeScale, Mathf.RoundToInt(110f * MaxParticlesScale));
        var main = ps.main;
        main.startColor = c;
        EnableOrbitalRing(ps, -4.2f, 0.12f, 0.26f);
        SolidColorOverLife(ps, c);
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Triangle);
    }

    private static void ConfigureScorchingWind(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 0;
        main.startSpeed = 0f;
        main.startSize = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = 0f;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(System.Array.Empty<ParticleSystem.Burst>());

        var shape = ps.shape;
        shape.enabled = false;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;
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
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f * VisualSizeScale, 0.36f * VisualSizeScale);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(280f * MaxParticlesScale);
        main.gravityModifier = 0f;
        RandomTwoColor(main, new Color(1f, 0.15f, 0.08f), new Color(1f, 0.92f, 0.2f));

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 130, 160) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.38f * VisualAreaScale;
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
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelConst(1.9f * VelocityScale),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f));

        SolidColorOverLife(ps, c);
    }

    private static void ConfigureHail(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 6f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f * VelocityScale, 0.45f * VelocityScale);
        main.startSize = new ParticleSystem.MinMaxCurve(
            0.035f * VisualSizeScale,
            0.055f * VisualSizeScale);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(120f * MaxParticlesScale);
        main.gravityModifier = 0f;
        RandomTwoColor(main, new Color(0.72f, 0.9f, 1f), new Color(0.95f, 0.99f, 1f));

        var em = ps.emission;
        em.rateOverTime = 50f * EmissionRateScale;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.5f * VisualAreaScale, 0.08f, 0.2f);
        shape.position = new Vector3(0f, 0.45f * VisualAreaScale, 0f);
        shape.randomDirectionAmount = 0.12f;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelRange(-0.35f * VelocityScale, 0.35f * VelocityScale),
            VelRange(-4.5f * VelocityScale, -2.8f * VelocityScale),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f));

        FadeAlphaOverLife(ps);
    }

    private static void ConfigureElectrowetting(ParticleSystem ps)
    {
        ConfigureElectrowettingBurst(ps);
        ConfigureElectrowettingGlow(CreateCoreChild(ps.transform, "CoreGlow"));
    }

    private static void ConfigureElectrowettingBurst(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.18f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f * VelocityScale, 2.2f * VelocityScale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f * VisualSizeScale, 0.12f * VisualSizeScale);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(80f * MaxParticlesScale);
        main.gravityModifier = 0f;
        RandomTwoColor(main, new Color(0.2f, 0.55f, 1f), new Color(1f, 0.92f, 0.25f));

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 45, 60) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.28f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(2.4f * VelocityScale),
            VelRange(1.2f * VelocityScale, 2.4f * VelocityScale));

        FadeAlphaOverLife(ps);
    }

    private static void ConfigureElectrowettingGlow(Transform coreTransform)
    {
        var ps = coreTransform.gameObject.AddComponent<ParticleSystem>();
        var glowColor = new Color(0.55f, 0.82f, 1f);

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.15f;
        main.startLifetime = 0.12f;
        main.startSpeed = 0f;
        main.startSize = 0.22f * VisualSizeScale;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 20;
        main.gravityModifier = 0f;
        main.startColor = glowColor;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 8, 12) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.12f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;

        SolidColorOverLife(ps, glowColor);
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Circle);
    }

    private static void ConfigureDustSandStorm(ParticleSystem ps)
    {
        ApplyCommonLoop(
            ps,
            0.55f,
            52f,
            new ParticleSystem.MinMaxCurve(0.035f * VisualSizeScale, 0.065f * VisualSizeScale),
            Mathf.RoundToInt(180f * MaxParticlesScale));

        var main = ps.main;
        RandomTwoColor(main, Sandy, new Color(0.45f, 0.35f, 0.22f));
        EnableOrbitalRing(ps, -3.4f, 0.12f, 0.42f);
        DustAlphaOverLife(ps, 0.75f, 0.95f);
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Hexagon);

        ConfigureDustHaze(CreateCoreChild(ps.transform, "DustHaze"));
    }

    private static void ApplyCommonLoop(
        ParticleSystem ps,
        float lifetime,
        float rate,
        ParticleSystem.MinMaxCurve startSize,
        int maxParticles)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

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

    private static void ConfigureDustHaze(Transform hazeTransform)
    {
        var ps = hazeTransform.gameObject.AddComponent<ParticleSystem>();
        var hazeColor = new Color(Sandy.r, Sandy.g, Sandy.b, 0.5f);

        ApplyCommonLoop(ps, 0.75f, 28f, 0.16f * VisualSizeScale, 100);
        var main = ps.main;
        main.startColor = hazeColor;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.48f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(-1.6f * VelocityScale),
            VelConst(0.05f * VelocityScale));

        DustAlphaOverLife(ps, 0.45f, 0.7f);
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Circle);
    }

    private static void DustAlphaOverLife(ParticleSystem ps, float startAlpha, float endAlpha)
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
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(endAlpha * 0.35f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    private static void SteamAlphaOverLife(ParticleSystem ps, float startAlpha, float endAlpha)
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
                new GradientAlphaKey(startAlpha, 0f),
                new GradientAlphaKey(endAlpha, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    private static void ConfigureMagnetism(ParticleSystem ps)
    {
        ConfigureMagneticFieldCore(
            ps,
            new Color(0.7f, 0.95f, 1f),
            Color.white);
        ConfigureMagneticCoreGlow(CreateCoreChild(ps.transform, "CoreGlow"));
    }

    private static Transform CreateCoreChild(Transform parent, string childName)
    {
        var child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = Vector3.zero;
        return child.transform;
    }

    private static void ConfigureMagneticFieldCore(ParticleSystem ps, Color primary, Color secondary)
    {
        var fineSize = 0.05f * VisualSizeScale * FineParticleScale;
        ApplyCommonLoop(ps, 0.45f, 55f, fineSize, Mathf.RoundToInt(150f * MaxParticlesScale));
        var main = ps.main;
        RandomTwoColor(main, primary, secondary);

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.35f * VisualAreaScale;
        shape.position = Vector3.zero;

        var vel = ps.velocityOverLifetime;
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(3f * VelocityScale),
            VelConst(-2.8f * VelocityScale));

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.5f, 0.75f),
            new Keyframe(1f, 0.35f)
        ));

        FadeAlphaOverLife(ps);
    }

    private static void ConfigureMagneticCoreGlow(Transform coreTransform)
    {
        var ps = coreTransform.gameObject.AddComponent<ParticleSystem>();
        var glowColor = new Color(0.85f, 0.98f, 1f);
        var coreSize = 0.08f * VisualSizeScale * FineParticleScale;

        ApplyCommonLoop(ps, 0.35f, 28f, coreSize, 60);
        var main = ps.main;
        main.startColor = glowColor;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.08f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        vel.enabled = false;

        SolidColorOverLife(ps, glowColor);
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Circle);
    }

    private static void FadeAlphaOverLife(ParticleSystem ps)
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

    private static void ConfigureStaticCharge(ParticleSystem ps)
    {
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.maxParticles = 0;
        main.startSpeed = 0f;
        main.startSize = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = 0f;

        var em = ps.emission;
        em.rateOverTime = 0f;
        em.SetBursts(System.Array.Empty<ParticleSystem.Burst>());

        var shape = ps.shape;
        shape.enabled = false;

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

    private static ParticleSystem.MinMaxCurve ConstantCurve(float value)
    {
        return VelConst(value);
    }

    private static void ApplyOrbitalVelocity(
        ParticleSystem.VelocityOverLifetimeModule vel,
        float orbitalZ,
        float radial,
        float orbitalX = 0f,
        float orbitalY = 0f)
    {
        ApplyVelocityOverLifetime(
            vel,
            VelConst(0f),
            VelConst(0f),
            VelConst(0f),
            VelConst(orbitalX),
            VelConst(orbitalY),
            VelConst(orbitalZ),
            VelConst(radial));
    }
}
