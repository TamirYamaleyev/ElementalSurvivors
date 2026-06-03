using UnityEngine;

/// <summary>
/// Strongly distinct <see cref="ParticleSystem"/> silhouettes per <see cref="StatusType"/> for DoT read.
/// Resets shape and auxiliary modules before each apply so Unity does not keep stale parameters from the previous profile.
/// </summary>
public static class ElementDotEmitterPresets
{
    public static void Apply(ParticleSystem ps, StatusType element)
    {
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 128;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 0f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = false;

        var colorBySpeed = ps.colorBySpeed;
        colorBySpeed.enabled = false;

        ResetAuxiliaryModules(ps);

        var shape = ps.shape;
        ResetShapeNeutral(ref shape);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        SetVelocityThreeAxisTwoConstants(ref vel, 0f, 0f, 0f, 0f, 0f, 0f);
        ResetVelocityOrbitalAndRadial(ref vel);

        switch (element)
        {
            case StatusType.Fire:
                ApplyFire(ps, ref main, ref emission, ref shape, ref vel);
                break;
            case StatusType.Water:
                ApplyWater(ps, ref main, ref emission, ref shape, ref vel);
                break;
            case StatusType.Wind:
                ApplyWind(ps, ref main, ref emission, ref shape, ref vel);
                break;
            case StatusType.Earth:
                ApplyEarth(ps, ref main, ref emission, ref shape, ref vel);
                break;
            case StatusType.Lightning:
                ApplyLightning(ps, ref main, ref emission, ref shape, ref vel);
                break;
            default:
                ApplyDefault(ref main, ref emission, ref shape, ref vel);
                break;
        }
    }

    private static void ApplyFire(
        ParticleSystem ps,
        ref ParticleSystem.MainModule main,
        ref ParticleSystem.EmissionModule emission,
        ref ParticleSystem.ShapeModule shape,
        ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        // Narrow cone at the feet; upward read reinforced by positive linear Y over lifetime.
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 16f;
        shape.radius = 0.04f;
        shape.rotation = Vector3.zero;
        shape.position = new Vector3(0f, -0.16f, 0f);
        shape.randomDirectionAmount = 0.04f;
        shape.sphericalDirectionAmount = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.44f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.32f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.09f);
        main.gravityModifier = -0.06f;
        emission.rateOverTime = 28f;
        SetVelocityThreeAxisTwoConstants(ref vel, -0.06f, 0.06f, 0.42f, 1.15f, -0.06f, 0.06f);

        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.separateAxes = false;
        sol.size = new ParticleSystem.MinMaxCurve(1f, ShrinkOverLifetimeCurve());
    }

    private static void ApplyWater(
        ParticleSystem ps,
        ref ParticleSystem.MainModule main,
        ref ParticleSystem.EmissionModule emission,
        ref ParticleSystem.ShapeModule shape,
        ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        // Thin sheet above the unit; strong downward drift (rain / pour).
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.78f, 0.05f, 0.02f);
        shape.position = new Vector3(0f, 0.52f, 0f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.85f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.18f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.17f);
        main.gravityModifier = 0.38f;
        emission.rateOverTime = 14f;
        SetVelocityThreeAxisTwoConstants(ref vel, -0.08f, 0.08f, -1.05f, -2.05f, -0.04f, 0.04f);

        var lim = ps.limitVelocityOverLifetime;
        lim.enabled = true;
        lim.space = ParticleSystemSimulationSpace.Local;
        lim.drag = new ParticleSystem.MinMaxCurve(0.48f, 0.78f);
        lim.multiplyDragByParticleSize = true;
        lim.multiplyDragByParticleVelocity = true;

        var noise = ps.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.strength = new ParticleSystem.MinMaxCurve(0.05f, 0.12f);
        noise.frequency = 0.55f;
        noise.scrollSpeed = 0.12f;
        noise.damping = true;
    }

    private static void ApplyWind(
        ParticleSystem ps,
        ref ParticleSystem.MainModule main,
        ref ParticleSystem.EmissionModule emission,
        ref ParticleSystem.ShapeModule shape,
        ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        // Ring emitter + orbital Y (swirl around vertical axis); light radial for volume.
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.28f;
        shape.radiusThickness = 1f;
        shape.arc = 360f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        shape.randomDirectionAmount = 0f;
        shape.sphericalDirectionAmount = 0f;
        shape.position = new Vector3(0f, 0.26f, 0f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.48f, 0.72f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.12f, 0.38f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.032f, 0.085f);
        main.gravityModifier = 0f;
        emission.rateOverTime = 22f;
        SetVelocityThreeAxisTwoConstants(ref vel, -0.03f, 0.03f, -0.06f, 0.06f, -0.03f, 0.03f);

        vel.orbitalX = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.orbitalY = new ParticleSystem.MinMaxCurve(0.82f, 1.38f);
        vel.orbitalZ = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.orbitalXMultiplier = 1f;
        vel.orbitalYMultiplier = 1f;
        vel.orbitalZMultiplier = 1f;
        vel.radial = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
        vel.radialMultiplier = 1f;

        var noise = ps.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.strength = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        noise.frequency = 0.85f;
        noise.scrollSpeed = 0.22f;
        noise.damping = true;
    }

    private static void ApplyEarth(
        ParticleSystem ps,
        ref ParticleSystem.MainModule main,
        ref ParticleSystem.EmissionModule emission,
        ref ParticleSystem.ShapeModule shape,
        ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        // Low dust shell: slight radial push + settle downward (distinct from vertical rain).
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.18f;
        shape.radiusThickness = 0.55f;
        shape.position = new Vector3(0f, 0.08f, 0f);
        shape.randomDirectionAmount = 0.22f;
        shape.sphericalDirectionAmount = 0.12f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 0.68f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.06f, 0.28f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.15f);
        main.gravityModifier = 0.62f;
        emission.rateOverTime = 15f;
        SetVelocityThreeAxisTwoConstants(ref vel, -0.12f, 0.12f, -0.22f, -0.55f, -0.12f, 0.12f);
        vel.radial = new ParticleSystem.MinMaxCurve(0.1f, 0.26f);
        vel.radialMultiplier = 1f;

        var noise = ps.noise;
        noise.enabled = true;
        noise.quality = ParticleSystemNoiseQuality.Medium;
        noise.strength = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        noise.frequency = 0.75f;
        noise.scrollSpeed = 0.12f;
        noise.damping = true;
    }

    private static void ApplyLightning(
        ParticleSystem _,
        ref ParticleSystem.MainModule main,
        ref ParticleSystem.EmissionModule emission,
        ref ParticleSystem.ShapeModule shape,
        ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.12f;
        shape.radiusThickness = 0.2f;
        shape.arc = 360f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        shape.randomDirectionAmount = 0.92f;
        shape.sphericalDirectionAmount = 0.78f;
        shape.position = new Vector3(0f, 0.3f, 0f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.22f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.1f, 3.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.022f, 0.06f);
        main.gravityModifier = 0f;
        emission.rateOverTime = 36f;
        SetVelocityThreeAxisTwoConstants(ref vel, -1.35f, 1.35f, -1.35f, 1.35f, -1.35f, 1.35f);
    }

    private static void ApplyDefault(
        ref ParticleSystem.MainModule main,
        ref ParticleSystem.EmissionModule emission,
        ref ParticleSystem.ShapeModule shape,
        ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.45f, 0.08f, 0.01f);
        shape.position = new Vector3(0f, 0.32f, 0f);
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0f, 0.15f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        main.gravityModifier = 0.35f;
        emission.rateOverTime = 16f;
        SetVelocityThreeAxisTwoConstants(ref vel, 0f, 0f, -0.75f, -1.35f, 0f, 0f);
    }

    private static AnimationCurve ShrinkOverLifetimeCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 1f, 0f, -0.35f),
            new Keyframe(1f, 0.52f, -0.65f, 0f));
    }

    private static void ResetVelocityOrbitalAndRadial(ref ParticleSystem.VelocityOverLifetimeModule vel)
    {
        // Unity requires orbitalX, orbitalY, and orbitalZ to use the same MinMaxCurve mode.
        // Use RandomBetweenTwoConstants everywhere so presets can use two-constant orbitalY (Wind).
        var orbitalFlat = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.orbitalX = orbitalFlat;
        vel.orbitalY = orbitalFlat;
        vel.orbitalZ = orbitalFlat;
        vel.orbitalXMultiplier = 1f;
        vel.orbitalYMultiplier = 1f;
        vel.orbitalZMultiplier = 1f;
        var offsetFlat = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.orbitalOffsetX = offsetFlat;
        vel.orbitalOffsetY = offsetFlat;
        vel.orbitalOffsetZ = offsetFlat;
        vel.orbitalOffsetXMultiplier = 1f;
        vel.orbitalOffsetYMultiplier = 1f;
        vel.orbitalOffsetZMultiplier = 1f;
        vel.radial = orbitalFlat;
        vel.radialMultiplier = 1f;
        vel.speedModifier = new ParticleSystem.MinMaxCurve(1f);
        vel.speedModifierMultiplier = 1f;
    }

    private static void ResetAuxiliaryModules(ParticleSystem ps)
    {
        var noise = ps.noise;
        noise.enabled = false;
        noise.strength = new ParticleSystem.MinMaxCurve(0f, 0f);
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0f;
        noise.damping = false;

        var rot = ps.rotationOverLifetime;
        rot.enabled = false;
        rot.separateAxes = false;
        rot.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        var lim = ps.limitVelocityOverLifetime;
        lim.enabled = false;
        lim.drag = new ParticleSystem.MinMaxCurve(0f, 0f);
        lim.multiplyDragByParticleSize = false;
        lim.multiplyDragByParticleVelocity = false;

        var sol = ps.sizeOverLifetime;
        sol.enabled = false;
        sol.separateAxes = false;
        sol.size = new ParticleSystem.MinMaxCurve(1f, 1f);
    }

    private static void ResetShapeNeutral(ref ParticleSystem.ShapeModule shape)
    {
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.01f;
        shape.radiusThickness = 0f;
        shape.angle = 25f;
        shape.arc = 360f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        shape.randomDirectionAmount = 0f;
        shape.sphericalDirectionAmount = 0f;
        shape.alignToDirection = false;
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;
        shape.scale = Vector3.one;
    }

    private static void SetVelocityThreeAxisTwoConstants(
        ref ParticleSystem.VelocityOverLifetimeModule vel,
        float xmin, float xmax,
        float ymin, float ymax,
        float zmin, float zmax)
    {
        vel.x = new ParticleSystem.MinMaxCurve(xmin, xmax);
        vel.y = new ParticleSystem.MinMaxCurve(ymin, ymax);
        vel.z = new ParticleSystem.MinMaxCurve(zmin, zmax);
    }
}
