namespace ShutdownTimer.App.Application;

public static class CountdownVisualState
{
    private static readonly TimeSpan FinalSecondsThreshold = TimeSpan.FromSeconds(30);

    public static bool IsFinalSecondsWarningActive(TimeSpan remaining) =>
        remaining > TimeSpan.Zero && remaining <= FinalSecondsThreshold;
}
