public enum RunMode
{
    Standard = 0,
    Endless = 1
}

/// <summary>
/// Cross-scene launch flag set by Main Menu before loading SampleScene.
/// Consumed once on gameplay bootstrap.
/// </summary>
public static class RunLaunchContext
{
    public static RunMode PendingMode { get; set; } = RunMode.Standard;

    public static RunMode ConsumePendingMode()
    {
        var mode = PendingMode;
        PendingMode = RunMode.Standard;
        return mode;
    }
}
