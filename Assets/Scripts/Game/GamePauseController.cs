public static class GamePauseController
{
    public enum PauseReason
    {
        None,
        LevelUp,
        PauseMenu
    }

    private static PauseReason activeReason = PauseReason.None;

    public static bool IsPaused => activeReason != PauseReason.None;

    public static bool CanOpenPauseMenu =>
        activeReason == PauseReason.None || activeReason == PauseReason.PauseMenu;

    public static void RequestPause(PauseReason reason)
    {
        if (reason == PauseReason.PauseMenu && activeReason == PauseReason.LevelUp)
            return;

        activeReason = reason;
        UnityEngine.Time.timeScale = 0f;
    }

    public static void ReleasePause(PauseReason reason)
    {
        if (activeReason != reason)
            return;

        activeReason = PauseReason.None;
        UnityEngine.Time.timeScale = 1f;
    }

    public static void ForceResume()
    {
        activeReason = PauseReason.None;
        UnityEngine.Time.timeScale = 1f;
    }
}
