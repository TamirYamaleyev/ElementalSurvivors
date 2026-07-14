using UnityEngine;

public static class ElementStatusPalette
{
    // Debuff VFX tints — keep high contrast on brown arena ground.
    public static readonly Color FirePrimary = new(1f, 0.12f, 0.08f);

    // Water: #0080ff + #004a94
    public static readonly Color WaterPrimary = Hex(0x0080ff);
    public static readonly Color WaterSecondary = Hex(0x004a94);

    public static readonly Color WindPrimary = new(0.45f, 1f, 0.5f);
    public static readonly Color EarthPrimary = new(0.82f, 0.62f, 0.28f);
    public static readonly Color EarthDamageNumber = new(0.85f, 0.68f, 0.35f);

    // Lightning: #0800ff + #040075
    public static readonly Color LightningPrimary = Hex(0x0800ff);
    public static readonly Color LightningSecondary = Hex(0x040075);

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

    static Color Hex(int rgb)
    {
        return new Color(
            ((rgb >> 16) & 0xff) / 255f,
            ((rgb >> 8) & 0xff) / 255f,
            (rgb & 0xff) / 255f,
            1f);
    }
}
