using UnityEngine;

/// <summary>URP/Legacy particle material so <see cref="ParticleSystem.MainModule.startColor"/> is visible.</summary>
public static class ElementalParticleVfxUtil
{
    private static Material _sharedParticleMaterial;

    public static void EnsureColorParticleMaterial(ParticleSystemRenderer renderer)
    {
        if (renderer == null)
            return;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Particles/Simple Lit");
        if (shader == null)
            shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (shader == null)
            return;

        if (_sharedParticleMaterial == null || _sharedParticleMaterial.shader != shader)
        {
            if (_sharedParticleMaterial != null && Application.isPlaying)
                Object.Destroy(_sharedParticleMaterial);

            _sharedParticleMaterial = new Material(shader)
            {
                name = "ElementalParticles_RuntimeShared",
                hideFlags = HideFlags.DontSave
            };
        }

        renderer.sharedMaterial = _sharedParticleMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }

    public static void ApplySortingFromSprite(ParticleSystemRenderer particleRenderer, SpriteRenderer spriteRenderer, int orderOffset = 1)
    {
        if (particleRenderer == null || spriteRenderer == null)
            return;

        particleRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        particleRenderer.sortingOrder = spriteRenderer.sortingOrder + orderOffset;
    }

    public static void ApplyFallbackSorting(ParticleSystemRenderer particleRenderer, int sortingOrder = 500)
    {
        if (particleRenderer == null)
            return;

        int layerId = SortingLayer.NameToID("Default");
        if (layerId == -1 && SortingLayer.layers.Length > 0)
            layerId = SortingLayer.layers[0].id;

        particleRenderer.sortingLayerID = layerId;
        particleRenderer.sortingOrder = sortingOrder;
    }
}
