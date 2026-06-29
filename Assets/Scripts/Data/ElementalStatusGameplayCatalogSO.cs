using System;
using UnityEngine;

[Serializable]
public class ElementalStatusGameplayDefinition
{
    public float damagePerTick = 2f;
    public float tickInterval = 0.5f;

    [Header("DoT damage number colors")]
    public Color fireDamageColor = ElementStatusPalette.FirePrimary;
    public Color waterDamageColor = ElementStatusPalette.WaterPrimary;
    public Color windDamageColor = ElementStatusPalette.WindPrimary;
    public Color earthDamageColor = ElementStatusPalette.EarthDamageNumber;
    public Color lightningDamageColor = ElementStatusPalette.LightningPrimary;
}

[CreateAssetMenu(
    fileName = "ElementalStatusGameplayCatalog",
    menuName = "Elemental Survivors/Elemental Status Gameplay Catalog")]
public class ElementalStatusGameplayCatalogSO : ScriptableObject
{
    [SerializeField] private ElementalStatusGameplayDefinition settings = new();

    public float DamagePerTick => settings.damagePerTick;
    public float TickInterval => settings.tickInterval;

    public Color GetDamageColor(StatusType type)
    {
        return type switch
        {
            StatusType.Fire => settings.fireDamageColor,
            StatusType.Water => settings.waterDamageColor,
            StatusType.Wind => settings.windDamageColor,
            StatusType.Earth => settings.earthDamageColor,
            StatusType.Lightning => settings.lightningDamageColor,
            _ => Color.white
        };
    }
}
