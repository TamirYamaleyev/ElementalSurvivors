using UnityEngine;

public static class ElementStatusPalette
{
    public static readonly Color FirePrimary = new(1f, 0.15f, 0.08f);
    public static readonly Color WaterPrimary = new(0.2f, 0.45f, 1f);
    public static readonly Color WindPrimary = new(0.92f, 0.95f, 1f);
    public static readonly Color EarthPrimary = new(0.35f, 0.28f, 0.2f);
    public static readonly Color EarthDamageNumber = new(0.75f, 0.62f, 0.45f);
    public static readonly Color LightningPrimary = new(1f, 0.92f, 0.15f);

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
