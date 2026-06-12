using UnityEngine;

/// <summary>
/// Attached to each elemental status VFX prefab root. Adds/configures <see cref="ParticleSystem"/> on first Awake.
/// </summary>
[DisallowMultipleComponent]
public class ElementalParticleBootstrap : MonoBehaviour
{
    /// <summary>Scales emission shape radius / offsets (wider cloud).</summary>
    private const float VisualAreaScale = 2f;

    /// <summary>Scales particle size in <see cref="ApplyCommon"/>.</summary>
    private const float VisualSizeScale = 1.75f;

    private const float EmissionRateScale = 1.55f;
    private const float MaxParticlesScale = 1.5f;
    /// <summary>Extra horizontal width for Fire/Water vertical streams (box emission).</summary>
    private const float FireWaterStripWidth = 0.8f;

    private const float VelocityScale = 1f;

    private static readonly Color ElementColorFire = new(1f, 0.2f, 0.1f);
    private static readonly Color ElementColorWater = new(0.2f, 0.45f, 1f);
    private static readonly Color ElementColorWind = Color.white;
    private static readonly Color ElementColorEarth = new(0.12f, 0.12f, 0.12f);
    private static readonly Color ElementColorLightning = new(1f, 0.95f, 0.2f);
    private static readonly Color BossBlackParticle = new(0.02f, 0.02f, 0.025f);

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

        ApplyCommon(ps);
        switch (kind)
        {
            case PresetKind.Fire:
                ConfigureFire(ps);
                break;
            case PresetKind.Water:
                ConfigureWater(ps);
                break;
            case PresetKind.Wind:
                ConfigureWind(ps);
                break;
            case PresetKind.Earth:
                ConfigureEarth(ps);
                break;
            case PresetKind.Lightning:
                ConfigureLightning(ps);
                break;
            case PresetKind.BossRisingCone:
                ConfigureBossRisingCone(ps);
                AddBossBlackTriangleDispersal();
                break;
        }

        ApplyParticleMaterial(ps);
    }

    private void OnDestroy()
    {
        if (runtimeBossBlackTriangleMesh != null)
        {
            Destroy(runtimeBossBlackTriangleMesh);
            runtimeBossBlackTriangleMesh = null;
        }
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
        main.maxParticles = Mathf.RoundToInt(80f * MaxParticlesScale);
        main.gravityModifier = 0f;

        var em = ps.emission;
        em.rateOverTime = 28f * EmissionRateScale;
    }

    private static void ConfigureFire(ParticleSystem ps)
    {
        var c = ElementColorFire;
        var main = ps.main;
        main.startColor = c;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(FireWaterStripWidth * VisualAreaScale, 0.14f, 0.2f);
        shape.position = new Vector3(0f, -0.32f * VisualAreaScale, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.y = new ParticleSystem.MinMaxCurve(2.2f * VelocityScale);

        EnableSolidColorOverLifetime(ps, c);
    }

    private static void ConfigureWater(ParticleSystem ps)
    {
        var c = ElementColorWater;
        var main = ps.main;
        main.startColor = c;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(FireWaterStripWidth * VisualAreaScale, 0.14f, 0.2f);
        shape.position = new Vector3(0f, 0.32f * VisualAreaScale, 0f);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.y = new ParticleSystem.MinMaxCurve(-2f * VelocityScale);

        EnableSolidColorOverLifetime(ps, c);
    }

    private static void ConfigureWind(ParticleSystem ps)
    {
        var c = ElementColorWind;
        var main = ps.main;
        main.startColor = c;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.22f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(-5.5f * VelocityScale);
        vel.radial = new ParticleSystem.MinMaxCurve(0.15f * VelocityScale);

        EnableSolidColorOverLifetime(ps, c);
    }

    private static void ConfigureEarth(ParticleSystem ps)
    {
        var c = ElementColorEarth;
        var main = ps.main;
        main.startColor = c;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.28f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(2.8f * VelocityScale);
        vel.radial = new ParticleSystem.MinMaxCurve(-0.25f * VelocityScale);

        EnableSolidColorOverLifetime(ps, c);
    }

    private static void ConfigureLightning(ParticleSystem ps)
    {
        var c = ElementColorLightning;
        var main = ps.main;
        main.startColor = c;
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f * VelocityScale, 3.2f * VelocityScale);
        main.startLifetime = 0.35f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.04f * VisualAreaScale;

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.radial = new ParticleSystem.MinMaxCurve(0.8f * VelocityScale, 2.4f * VelocityScale);

        EnableSolidColorOverLifetime(ps, c);
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
                new GradientColorKey(ElementColorFire, 0f),
                new GradientColorKey(ElementColorWater, 0.25f),
                new GradientColorKey(ElementColorWind, 0.5f),
                new GradientColorKey(ElementColorEarth, 0.75f),
                new GradientColorKey(ElementColorLightning, 1f)
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

        ApplyParticleMaterialToRenderer(ps, particleSprite);
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

    private static void EnableSolidColorOverLifetime(ParticleSystem ps, Color color)
    {
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
            new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
    }

    private void ApplyParticleMaterial(ParticleSystem ps)
    {
        ApplyParticleMaterialToRenderer(ps, particleSprite);
    }

    private static void ApplyParticleMaterialToRenderer(ParticleSystem ps, Sprite particleSprite)
    {
        var rnd = ps.GetComponent<ParticleSystemRenderer>();
        rnd.renderMode = ParticleSystemRenderMode.Billboard;

        Texture tex = null;
        if (particleSprite != null)
            tex = particleSprite.texture;

        var mat = CreateParticleMaterial(tex);
        if (mat != null)
        {
            rnd.material = mat;
            return;
        }

        if (tex == null)
            return;

        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Texture");
        if (shader == null)
            return;

        var fallback = new Material(shader);
        fallback.mainTexture = tex;
        rnd.material = fallback;
    }

    private static Material CreateParticleMaterial(Texture tex)
    {
        var shader =
            Shader.Find("Universal Render Pipeline/Particles/Unlit")
            ?? Shader.Find("Particles/Standard Unlit")
            ?? Shader.Find("Particles/Unlit");

        if (shader != null)
        {
            var m = new Material(shader);
            if (tex != null)
            {
                if (m.HasProperty("_BaseMap"))
                    m.SetTexture("_BaseMap", tex);
                if (m.HasProperty("_MainTex"))
                    m.SetTexture("_MainTex", tex);
            }

            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", Color.white);
            if (m.HasProperty("_Color"))
                m.SetColor("_Color", Color.white);

            return m;
        }

        var builtin = Resources.GetBuiltinResource<Material>("Default-Particle.mat");
        if (builtin == null)
            return null;

        var clone = new Material(builtin);
        if (tex != null && clone.HasProperty("_MainTex"))
            clone.SetTexture("_MainTex", tex);
        return clone;
    }
}
