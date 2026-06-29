/// <summary>
/// Presentation hook for <see cref="EnemyStatusController"/> (particles, UI, etc.).
/// </summary>
public interface IEnemyStatusVisualSink
{
    void RefreshStatusVisuals(StatusVfxPlan plan);

    void ResetForPool();
}
