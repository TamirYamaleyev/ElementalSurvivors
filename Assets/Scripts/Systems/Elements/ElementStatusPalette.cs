using UnityEngine;

public static class ElementStatusPalette
{
    // Debuff VFX / damage tint: Fire red, Water light blue, Lightning dark blue, Wind light green.
    public static readonly Color FirePrimary = new(1f, 0.12f, 0.08f);
    public static readonly Color WaterPrimary = new(0.55f, 0.82f, 1f);
    public static readonly Color WindPrimary = new(0.55f, 0.95f, 0.55f);
    public static readonly Color EarthPrimary = new(0.35f, 0.28f, 0.2f);
    public static readonly Color EarthDamageNumber = new(0.75f, 0.62f, 0.45f);
    public static readonly Color LightningPrimary = new(0.12f, 0.22f, 0.72f);

    public static Color GetPrimaryColor(StatusType type)
    {
        return type switch
        {
            StatusType.Fire => FirePrimary,
            StatusType.Water => WaterPrimary,
            StatusType.Wind => WindPrimary,
            StatusType.Earth => EarthPrimary,
            StatusType.Lightning => LightningPrimary,
            _ => Color.white
        };
    }

    public static Color GetDamageNumberColor(StatusType type)
    {
        if (type == StatusType.Earth)
            return EarthDamageNumber;

        return GetPrimaryColor(type);
    }
}
