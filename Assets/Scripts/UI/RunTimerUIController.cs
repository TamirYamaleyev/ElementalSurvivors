using TMPro;
using UnityEngine;

public sealed class RunTimerUIController : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private RunSessionController runSession;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text endlessLabel;

    private int lastShownSeconds = -1;
    private bool lastEndlessVisible;

    private void Awake()
    {
        if (spawner == null)
            spawner = FindFirstObjectByType<EnemySpawner>();

        if (runSession == null)
            runSession = FindFirstObjectByType<RunSessionController>();

        if (endlessLabel != null)
        {
            endlessLabel.gameObject.SetActive(false);
            lastEndlessVisible = false;
        }
    }

    private void Start()
    {
        Render(0);
        RefreshEndlessLabel(force: true);
    }

    // Live HUD timer requires a per-frame check; text is only rebuilt when the whole-second value changes.
    private void Update()
    {
        RefreshEndlessLabel(force: false);

        if (spawner == null || timerText == null)
            return;

        int seconds = Mathf.FloorToInt(spawner.ElapsedTime);
        if (seconds == lastShownSeconds)
            return;

        Render(seconds);
    }

    private void RefreshEndlessLabel(bool force)
    {
        if (endlessLabel == null)
            return;

        bool show = runSession != null && runSession.IsEndless;
        if (!force && show == lastEndlessVisible)
            return;

        lastEndlessVisible = show;
        endlessLabel.gameObject.SetActive(show);
        if (show)
            endlessLabel.text = "Endless";
    }

    private void Render(int totalSeconds)
    {
        lastShownSeconds = totalSeconds;
        timerText.text = EndlessBestTimeStore.FormatTime(totalSeconds);
    }
}
