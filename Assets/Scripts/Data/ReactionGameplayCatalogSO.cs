using System;
using UnityEngine;

public enum ReactionGameplayMode
{
    Instant,
    Sustained,
}

[Serializable]
public class ReactionGameplayDefinition
{
    public bool enabled = true;
    public ReactionGameplayMode mode = ReactionGameplayMode.Instant;
    public GameObject vfxPrefab;

    [Header("Area")]
    public float radius = 2.5f;
    public float duration = 2f;

    [Header("Damage")]
    public float flatDamage = 12f;
    public float damageMultiplier = 1f;
    public float contactDps = 18f;
    public float tickInterval = 0.25f;

    [Header("Knockback / Pull")]
    public float knockbackImpulse = 6f;
    public float pullSpeed = 3.5f;

    [Header("Scorching Wind")]
    public int laserCount = 3;
    public float laserLength = 5f;
    public float laserHalfWidth = 0.35f;

    [Header("Hail")]
    public float stunDuration = 1.2f;
    public float hailImmunityGain = 2f;
}

[CreateAssetMenu(fileName = "ReactionGameplayCatalog", menuName = "Elemental Survivors/Reaction Gameplay Catalog")]
public class ReactionGameplayCatalogSO : ScriptableObject
{
    [SerializeField] private ReactionGameplayDefinition vaporize = new() { mode = ReactionGameplayMode.Sustained, radius = 2.2f, duration = 3f, contactDps = 22f };
    [SerializeField] private ReactionGameplayDefinition crystallize = new() { enabled = false };
    [SerializeField] private ReactionGameplayDefinition scorchingWind = new() { laserCount = 3, laserLength = 6f, flatDamage = 10f, damageMultiplier = 1f };
    [SerializeField] private ReactionGameplayDefinition explosion = new() { radius = 2.8f, flatDamage = 20f, knockbackImpulse = 7f };
    [SerializeField] private ReactionGameplayDefinition hail = new() { mode = ReactionGameplayMode.Sustained, radius = 2.5f, duration = 2.4f, stunDuration = 1.2f, hailImmunityGain = 2f };
    [SerializeField] private ReactionGameplayDefinition growth = new() { enabled = false };
    [SerializeField] private ReactionGameplayDefinition electrowetting = new() { radius = 4f, damageMultiplier = 1f };
    [SerializeField] private ReactionGameplayDefinition dustSandStorm = new() { enabled = false };
    [SerializeField] private ReactionGameplayDefinition magnetism = new() { mode = ReactionGameplayMode.Sustained, radius = 3f, duration = 2.5f, pullSpeed = 3.5f };
    [SerializeField] private ReactionGameplayDefinition staticCharge = new() { enabled = false };

    public bool TryGetDefinition(StatusType a, StatusType b, out ReactionGameplayDefinition definition)
    {
        definition = GetDefinition(a, b);
        return definition != null && definition.enabled;
    }

    public ReactionGameplayDefinition GetDefinition(StatusType a, StatusType b)
    {
        ReactionVfxCatalogSO.NormalizePair(a, b, out var x, out var y);
        if (x == y)
            return null;

        return (x, y) switch
        {
            (StatusType.Fire, StatusType.Water) => vaporize,
            (StatusType.Fire, StatusType.Earth) => crystallize,
            (StatusType.Fire, StatusType.Wind) => scorchingWind,
            (StatusType.Fire, StatusType.Lightning) => explosion,
            (StatusType.Water, StatusType.Wind) => hail,
            (StatusType.Water, StatusType.Earth) => growth,
            (StatusType.Water, StatusType.Lightning) => electrowetting,
            (StatusType.Wind, StatusType.Earth) => dustSandStorm,
            (StatusType.Wind, StatusType.Lightning) => magnetism,
            (StatusType.Earth, StatusType.Lightning) => staticCharge,
            _ => null,
        };
    }
}
