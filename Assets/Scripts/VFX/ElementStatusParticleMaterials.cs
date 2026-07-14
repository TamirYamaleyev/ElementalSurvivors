using UnityEngine;

/// <summary>
/// Particle materials for elemental status VFX only. Not used by reaction burst VFX.
/// Delegates to <see cref="ElementalVfxParticleMaterials"/> so startColor tints reliably.
/// </summary>
public static class ElementStatusParticleMaterials
{
    public static void ApplyBillboardMaterial(ParticleSystem ps, VfxParticleShapeLibrary.Shape shape)
    {
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, shape);
    }

    public static void ApplyBillboardMaterial(ParticleSystem ps, Sprite particleSprite)
    {
        ElementalVfxParticleMaterials.ApplyBillboardMaterial(ps, particleSprite);
    }
}
