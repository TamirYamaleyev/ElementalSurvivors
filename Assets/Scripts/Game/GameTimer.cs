using UnityEngine;

public class GameTimer : MonoBehaviour
{
    public static GameTimer Instance { get; private set; }

    public float TimeAlive { get; private set; }

    private bool running = true;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddTime(float amount)
    {
        TimeAlive += amount;
    }

    void Update()
    {
        if (!running)
            return;

        TimeAlive += Time.deltaTime;
    }

    private void StopTimer()
    {
        running = false;
    }

    public void ContinueTimer()
    {
        running = true;
    }

    public void ResetTimer()
    {
        TimeAlive = 0f;
        running = true;
    }
}
