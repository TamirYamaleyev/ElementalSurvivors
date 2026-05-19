using System;

[Serializable]
public struct PlayerStatModifier : IEquatable<PlayerStatModifier>
{
    public PlayerStatType stat;
    public bool isMultiplier;
    public float value;

    public PlayerStatModifier(PlayerStatType stat, bool isMultiplier, float value)
    {
        this.stat = stat;
        this.isMultiplier = isMultiplier;
        this.value = value;
    }

    public bool Equals(PlayerStatModifier other)
    {
        return stat == other.stat
            && isMultiplier == other.isMultiplier
            && value.Equals(other.value);
    }

    public override bool Equals(object obj) => obj is PlayerStatModifier other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(stat, isMultiplier, value);
}
