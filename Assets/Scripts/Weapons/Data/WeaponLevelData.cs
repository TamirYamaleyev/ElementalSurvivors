using UnityEngine;

[System.Serializable]
public class WeaponLevelData
{
    public float damage;
    public float cooldown;
    public float range;
    public float knockback;
    [Tooltip("Multiplier")] public float width;
    [Tooltip("Multiplier")] public float height;
    public float lifetime;
    public float speed;
    public int projectileCount;
    public float spreadAngle;
    public float volleySpacing;
    public float statusDuration;

    public string levelupDescription;

    public AudioClip fireSFX;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    public Sprite[] visualSpriteArr;
}
