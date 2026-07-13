using UnityEngine;

public static class EndlessBestTimeStore
{
    const string BestTimeKey = "EndlessBestTimeSeconds";

    public static int GetBestSeconds()
    {
        return Mathf.Max(0, PlayerPrefs.GetInt(BestTimeKey, 0));
    }

    /// <summary>
    /// Updates stored best if <paramref name="seconds"/> is greater. Returns true on new record.
    /// </summary>
    public static bool TryUpdateBest(int seconds)
    {
        seconds = Mathf.Max(0, seconds);
        int best = GetBestSeconds();
        if (seconds <= best)
            return false;

        PlayerPrefs.SetInt(BestTimeKey, seconds);
        PlayerPrefs.Save();
        return true;
    }

    public static string FormatTime(int totalSeconds)
    {
        totalSeconds = Mathf.Max(0, totalSeconds);
        int minutes = totalSeconds / 60;
        int secs = totalSeconds % 60;
        return $"{minutes:00}:{secs:00}";
    }
}
