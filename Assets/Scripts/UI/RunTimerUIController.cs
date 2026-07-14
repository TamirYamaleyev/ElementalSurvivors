using TMPro;
using UnityEngine;

public sealed class RunTimerUIController : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TMP_Text timerText;

    private int lastShownSeconds = -1;

    private void Awake()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<EnemySpawner>();
    }

    private void Start()
    {
        Render(0);
    }

    // Live HUD timer requires a per-frame check; text is only rebuilt when the whole-second value changes.
    private void Update()
    {
        if (spawner == null || timerText == null)
            return;

        int seconds = Mathf.FloorToInt(spawner.ElapsedTime);
        if (seconds == lastShownSeconds)
            return;

        Render(seconds);
    }

    private void Render(int totalSeconds)
    {
        lastShownSeconds = totalSeconds;
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{secs:00}";
    }
}
