using UnityEngine;

[System.Serializable]
public class WeaponLevelData
{
    public float damage;
    public float cooldown;
    public float range;
    [Tooltip("Multiplier")] public float width;
    [Tooltip("Multiplier")] public float height;
    public float lifetime;
    public float speed;
    public int projectileCount;
    public float statusDuration;

    public Sprite visualSprite;
}
