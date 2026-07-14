using UnityEngine;

/// <summary>
/// Attached to each elemental status VFX prefab root. Adds/configures <see cref="ParticleSystem"/> on first Awake.
/// </summary>
[DisallowMultipleComponent]
public class ElementalParticleBootstrap : MonoBehaviour
{
    private static readonly Color ElementColorFirePrimary = ElementStatusPalette.FirePrimary;
    private static readonly Color ElementColorFireSecondary = new(1f, 0.4f, 0.12f);
    private static readonly Color ElementColorWaterPrimary = ElementStatusPalette.WaterPrimary;
    private static readonly Color ElementColorWaterSecondary = ElementStatusPalette.WaterSecondary;
    private static readonly Color ElementColorWindPrimary = ElementStatusPalette.WindPrimary;
    private static readonly Color ElementColorWindSecondary = new(0.75f, 1f, 0.7f);
    private static readonly Color ElementColorEarthPrimary = ElementStatusPalette.EarthPrimary;
    private static readonly Color ElementColorEarthSecondary = new(0.95f, 0.78f, 0.4f);
    private static readonly Color ElementColorLightningPrimary = ElementStatusPalette.LightningPrimary;
    private static readonly Color ElementColorLightningSecondary = ElementStatusPalette.LightningSecondary;
    private static readonly Color BossBlackParticle = new(0.02f, 0.02f, 0.025f);

    private const float VisualAreaScale = 2f;
    private const float VisualSizeScale = 1.75f;
    private const float EmissionRateScale = 1.55f;
    private const float VelocityScale = 1f;

    public enum PresetKind
    {
        Fire = 0,
        Water = 1,
        Wind = 2,
        Earth = 3,
        Lightning = 4,
        BossRisingCone = 5
    }

    [SerializeField] private PresetKind kind;
    [SerializeField] private Sprite particleSprite;

    private Mesh runtimeBossBlackTriangleMesh;

    private void Awake()
    {
        var ps = GetComponent<ParticleSystem>();
        if (ps == null)
            ps = gameObject.AddComponent<ParticleSystem>();

        switch (kind)
        {
            case PresetKind.Fire:
                ConfigureElementHailFall(ps, ElementColorFirePrimary, ElementColorFireSecondary);
                break;
            case PresetKind.Water:
                ConfigureElementHailFall(ps, ElementColorWaterPrimary, ElementColorWaterSecondary);
                break;
            case PresetKind.Wind:
                ConfigureElementHailFall(ps, ElementColorWindPrimary, ElementColorWindSecondary);
                break;
            case PresetKind.Earth:
                ConfigureElementHailFall(ps, ElementColorEarthPrimary, ElementColorEarthSecondary);
                break;
            case PresetKind.Lightning:
                ConfigureElementHailFall(ps, ElementColorLightningPrimary, ElementColorLightningSecondary);
                break;
            case PresetKind.BossRisingCone:
                ApplyCommon(ps);
                ConfigureBossRisingCone(ps);
                AddBossBlackTriangleDispersal();
                break;
        }

        ApplyParticleMaterial(ps);

        if (kind != PresetKind.BossRisingCone)
        {
            // Prewarm so showcase / first frame shows particles immediately.
            ps.Simulate(0.4f, true, true);
            ps.Play(true);
        }
    }

    private void ApplyParticleMaterial(ParticleSystem ps)
    {
        if (kind == PresetKind.BossRisingCone)
        {
            if (particleSprite != null)
                ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, particleSprite);
            else
                ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.GetElementShape(kind));
            return;
        }

        // Sprites/Default multiplies particle startColor reliably for all element tints.
        // Prefer assigned sprite when present (same white square on all status prefabs).
        if (particleSprite != null)
            ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, particleSprite);
        else
            ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.GetElementShape(kind));
    }

    private void OnDestroy()
    {
        if (runtimeBossBlackTriangleMesh != null)
        {
            Destroy(runtimeBossBlackTriangleMesh);
            runtimeBossBlackTriangleMesh = null;
        }
    }

    private static void ConfigureElementHailFall(ParticleSystem ps, Color primary, Color secondary)
    {
        ElementStatusHailFallUtility.Apply(ps, primary, secondary);
    }

    private static void ApplyCommon(ParticleSystem ps)
    {
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 8f;
        main.startLifetime = 0.55f;
        main.startSpeed = 0f;
        main.startSize = 0.1f * VisualSizeScale;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = Mathf.RoundToInt(80f * HailFallParticleUtility.MaxParticlesScale);
        main.gravityModifier = 0f;

        var em = ps.emission;
        em.rateOverTime = 28f * EmissionRateScale;
    }

    /// <summary>
    /// Wide circle at the base, upward flow + inward radial velocity (inverted cone silhouette).
    /// Each particle picks one of the five elemental tints (RandomColor palette).
    /// </summary>
    private static void ConfigureBossRisingCone(ParticleSystem ps)
    {
        var palette = new Gradient();
        palette.SetKeys(
            new[]
            {
                new GradientColorKey(ElementColorFirePrimary, 0f),
                new GradientColorKey(ElementColorWaterPrimary, 0.25f),
                new GradientColorKey(ElementColorWindPrimary, 0.5f),
                new GradientColorKey(ElementColorEarthPrimary, 0.75f),
                new GradientColorKey(ElementColorLightningPrimary, 1f)
            },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        var main = ps.main;
        var startColor = new ParticleSystem.MinMaxGradient
        {
            mode = ParticleSystemGradientMode.RandomColor,
            gradient = palette
        };
        main.startColor = startColor;
        main.startLifetime = 0.7f;

        var col = ps.colorOverLifetime;
        col.enabled = false;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.38f * VisualAreaScale;
        shape.position = new Vector3(0f, -0.18f * VisualAreaScale, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.y = new ParticleSystem.MinMaxCurve(2.4f * VelocityScale);
        vel.radial = new ParticleSystem.MinMaxCurve(-0.52f * VelocityScale);
    }

    /// <summary>
    /// Second layer: dense black particles emitted from an inverted-triangle footprint (XZ),
    /// with high random direction blend so streams peel off in many directions.
    /// </summary>
    private void AddBossBlackTriangleDispersal()
    {
        var holder = new GameObject("BossBlackTriangleDispersal");
        holder.transform.SetParent(transform, false);
        holder.transform.localPosition = Vector3.zero;

        var ps = holder.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear(true);

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.38f, 0.82f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f * VelocityScale, 3.6f * VelocityScale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.045f * VisualSizeScale, 0.078f * VisualSizeScale);
        main.startColor = BossBlackParticle;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 440;
        main.gravityModifier = 0f;

        var em = ps.emission;
        em.rateOverTime = 102f * EmissionRateScale;

        runtimeBossBlackTriangleMesh = CreateBossInvertedTriangleMesh(VisualAreaScale);
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Mesh;
        shape.mesh = runtimeBossBlackTriangleMesh;
        shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        shape.randomDirectionAmount = 0.96f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(BossBlackParticle, 0f), new GradientColorKey(BossBlackParticle, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = fade;

        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.Shape.Triangle);
    }

    private static Mesh CreateBossInvertedTriangleMesh(float scale)
    {
        var halfW = 0.5f * scale;
        var yPlane = -0.18f * scale;
        var zBack = -0.2f * scale;
        var zFront = 0.24f * scale;

        var mesh = new Mesh { name = "BossInvertedTriangleEmitter" };
        mesh.vertices = new[]
        {
            new Vector3(-halfW, yPlane, zBack),
            new Vector3(halfW, yPlane, zBack),
            new Vector3(0f, yPlane, zFront)
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
