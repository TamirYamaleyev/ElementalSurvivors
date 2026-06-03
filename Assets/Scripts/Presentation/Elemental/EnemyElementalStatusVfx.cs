using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DoT elemental particles: distinct silhouette per <see cref="StatusType"/> via <see cref="ElementDotEmitterPresets"/>, tint from <see cref="ElementVisualSetSO"/>.
/// </summary>
/// <remarks>
/// Manual: spawn or apply each of the five <see cref="StatusType"/> values on enemies and confirm five different emitter silhouettes;
/// switch element on one enemy and confirm shape resets cleanly (no hybrid sphere/box artifacts).
/// </remarks>
[DisallowMultipleComponent]
public sealed class EnemyElementalStatusVfx : MonoBehaviour
{
    [SerializeField] private EnemyStatusController statusController;
    [SerializeField] private ElementVisualSetSO visualSet;
    [SerializeField] private ParticleSystem dotParticles;
    [SerializeField] private SpriteRenderer bodySprite;

    private IElementVisualPalette palette;
    private readonly IActiveElementVisualPolicy policy = new MaxRemainingTimeElementPolicy();
    private readonly List<StatusSnapshot> snapshotBuffer = new();
    private Color spriteBaseColor;
    private bool hasSpriteBase;

    private void Awake()
    {
        if (bodySprite == null)
            bodySprite = GetComponent<SpriteRenderer>();
        if (bodySprite != null)
        {
            spriteBaseColor = bodySprite.color;
            hasSpriteBase = true;
        }

        if (statusController == null)
            statusController = GetComponent<EnemyStatusController>();

        if (visualSet == null)
            visualSet = Resources.Load<ElementVisualSetSO>("Elemental/DefaultElementVisualSet");

        palette = visualSet;

        if (dotParticles == null)
        {
            var child = new GameObject("ElementalDotParticles");
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            dotParticles = child.AddComponent<ParticleSystem>();
        }

        PrimeParticleSystem(dotParticles);
        SyncParticleRendererSorting();
    }

    private void OnEnable()
    {
        if (statusController != null)
            statusController.StatusesChanged += OnStatusesChanged;
        RefreshVisual();
    }

    private void OnDisable()
    {
        if (statusController != null)
            statusController.StatusesChanged -= OnStatusesChanged;
        StopParticlesAndClearTint();
    }

    private void OnStatusesChanged(IReadOnlyList<StatusSnapshot> snapshots)
    {
        RefreshVisualFromList(snapshots);
    }

    private void RefreshVisual()
    {
        if (statusController == null)
        {
            StopParticlesAndClearTint();
            return;
        }

        statusController.CopySnapshotsTo(snapshotBuffer);
        RefreshVisualFromList(snapshotBuffer);
    }

    private void RefreshVisualFromList(IReadOnlyList<StatusSnapshot> snapshots)
    {
        if (dotParticles == null || palette == null)
        {
            StopParticlesAndClearTint();
            return;
        }

        if (snapshots == null || snapshots.Count == 0)
        {
            StopParticlesAndClearTint();
            return;
        }

        if (!policy.TryPickPrimaryElement(snapshots, out StatusType primary))
        {
            StopParticlesAndClearTint();
            return;
        }

        dotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ElementDotEmitterPresets.Apply(dotParticles, primary);

        var renderer = dotParticles.GetComponent<ParticleSystemRenderer>();
        ElementalParticleVfxUtil.EnsureColorParticleMaterial(renderer);
        SyncParticleRendererSorting();

        Color c = palette.GetTint(primary);
        ApplySpriteTint(c);

        var main = dotParticles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(c);

        if (!dotParticles.isPlaying)
            dotParticles.Play();
    }

    private void StopParticlesAndClearTint()
    {
        ClearSpriteTint();
        if (dotParticles == null)
            return;
        dotParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void ApplySpriteTint(Color elementTint)
    {
        if (!hasSpriteBase || bodySprite == null)
            return;

        var blended = Color.Lerp(spriteBaseColor, elementTint, 0.65f);
        blended.a = spriteBaseColor.a;
        bodySprite.color = blended;
    }

    private void ClearSpriteTint()
    {
        if (!hasSpriteBase || bodySprite == null)
            return;
        bodySprite.color = spriteBaseColor;
    }

    private void SyncParticleRendererSorting()
    {
        if (dotParticles == null)
            return;

        var renderer = dotParticles.GetComponent<ParticleSystemRenderer>();
        if (bodySprite != null)
            ElementalParticleVfxUtil.ApplySortingFromSprite(renderer, bodySprite);
        else
            ElementalParticleVfxUtil.ApplyFallbackSorting(renderer);
    }

    private static void PrimeParticleSystem(ParticleSystem ps)
    {
        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.rateOverDistance = 0f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = false;

        var colorBySpeed = ps.colorBySpeed;
        colorBySpeed.enabled = false;

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = false;

        ElementalParticleVfxUtil.EnsureColorParticleMaterial(ps.GetComponent<ParticleSystemRenderer>());
    }
}
