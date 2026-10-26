namespace ShutdownTimer.App.WindowsIntegration;

public sealed record ShutdownScheduleResult(
    bool Succeeded,
    int? ExitCode = null,
    string? ErrorMessage = null);
