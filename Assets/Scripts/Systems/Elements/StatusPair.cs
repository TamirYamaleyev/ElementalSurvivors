using System;

/// <summary>Normalized elemental pair (First &lt; Second). Equal types are invalid for reactions.</summary>
public readonly struct StatusPair : IEquatable<StatusPair>
{
    public readonly StatusType First;
    public readonly StatusType Second;

    public StatusPair(StatusType a, StatusType b)
    {
        ReactionVfxCatalogSO.NormalizePair(a, b, out First, out Second);
    }

    public bool IsValid => First != Second;

    public bool Equals(StatusPair other) => First == other.First && Second == other.Second;

    public override bool Equals(object obj) => obj is StatusPair other && Equals(other);

    public override int GetHashCode() => ((int)First * 397) ^ (int)Second;

    public static bool operator ==(StatusPair left, StatusPair right) => left.Equals(right);

    public static bool operator !=(StatusPair left, StatusPair right) => !left.Equals(right);
}
