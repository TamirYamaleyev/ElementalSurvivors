public interface IPoolable
{
    void OnAcquired();
    void OnReleased();
}
