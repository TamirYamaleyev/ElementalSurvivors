using UnityEngine;

/// <summary>
/// Shared URP/builtin particle material setup for elemental status and reaction burst VFX.
/// </summary>
public static class ElementalVfxParticleMaterials
{
    public static void ApplyBillboardMaterial(ParticleSystem ps, VfxParticleShapeLibrary.Shape shape)
    {
        ApplyBillboardMaterial(ps, VfxParticleShapeLibrary.GetSprite(shape));
    }

    public static void ApplyBillboardMaterial(ParticleSystem ps, Sprite particleSprite)
    {
        var rnd = ps.GetComponent<ParticleSystemRenderer>();
        if (rnd == null)
            return;

        rnd.renderMode = ParticleSystemRenderMode.Billboard;
        rnd.enableGPUInstancing = false;

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

        var shader = Shader.Find("Unlit/Texture");
        if (shader == null)
            return;

        var fallback = new Material(shader);
        fallback.mainTexture = tex;
        rnd.material = fallback;
    }

    public static Material CreateParticleMaterial(Texture tex)
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

            if (m.HasProperty("_Surface"))
                m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend"))
                m.SetFloat("_Blend", 0f);
            if (m.HasProperty("_ZWrite"))
                m.SetFloat("_ZWrite", 0f);
            if (m.HasProperty("_Cull"))
                m.SetFloat("_Cull", 2f);

            if (m.HasProperty("_BaseColor"))
                m.SetColor("_BaseColor", Color.white);
            if (m.HasProperty("_Color"))
                m.SetColor("_Color", Color.white);

            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
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
