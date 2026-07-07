using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBaseStats", menuName = "Elemental Survivors/Player Base Stats")]
public class PlayerBaseStatsSO : ScriptableObject
{
    [Header("Health")]
    public float baseMaxHealth = 100f;

    [Header("Movement")]
    public float baseMoveSpeed = 8f;

    [Header("Combat")]
    public float baseDamageMultiplier = 1f;
    public float baseAttackSpeed = 1f;
    public float baseProjectileSpeedMultiplier = 1f;

    [Header("Pickup")]
    public float baseCollectRadius = 1.3f;
}
