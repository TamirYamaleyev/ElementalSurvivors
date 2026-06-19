/// <summary>
/// Optional presentation hook for <see cref="EnemyStatusController"/> (particles, UI, etc.).
/// </summary>
public interface IEnemyStatusVisualSink
{
    void OnStatusApplied(StatusType type);
    void OnStatusRemoved(StatusType type);
}
