/// <summary>Immutable snapshot of one active status for VFX readers.</summary>
public readonly struct StatusSnapshot
{
    public readonly StatusType Type;
    public readonly float Remaining;

    public StatusSnapshot(StatusType type, float remaining)
    {
        Type = type;
        Remaining = remaining;
    }
}
