using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security;
using ShutdownTimer.App.Application;

namespace ShutdownTimer.App.WindowsIntegration;

public sealed class ShutdownScheduler
{
    private readonly IProcessLauncher _processLauncher;

    public ShutdownScheduler(IProcessLauncher processLauncher)
    {
        _processLauncher = processLauncher ?? throw new ArgumentNullException(nameof(processLauncher));
    }

    public async Task<ShutdownScheduleResult> ScheduleAsync(int durationMinutes)
    {
        if (durationMinutes < DurationSelectionModel.MinimumMinutes
            || durationMinutes > DurationSelectionModel.MaximumMinutes
            || durationMinutes % DurationSelectionModel.StepMinutes != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMinutes),
                durationMinutes,
                $"Duration must be between {DurationSelectionModel.MinimumMinutes} and "
                + $"{DurationSelectionModel.MaximumMinutes} minutes in "
                + $"{DurationSelectionModel.StepMinutes}-minute steps.");
        }

        var durationSeconds = checked(durationMinutes * 60);
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, "shutdown.exe"),
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("/s");
        startInfo.ArgumentList.Add("/f");
        startInfo.ArgumentList.Add("/t");
        startInfo.ArgumentList.Add(durationSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));

        try
        {
            var exitCode = await _processLauncher.RunAsync(startInfo).ConfigureAwait(false);
            return exitCode == 0
                ? new ShutdownScheduleResult(Succeeded: true, ExitCode: exitCode)
                : new ShutdownScheduleResult(
                    Succeeded: false,
                    ExitCode: exitCode,
                    ErrorMessage: $"Windows shutdown command failed with exit code {exitCode}.");
        }
        catch (Exception exception) when (
            exception is Win32Exception
                or InvalidOperationException
                or IOException
                or UnauthorizedAccessException
                or SecurityException)
        {
            return new ShutdownScheduleResult(
                Succeeded: false,
                ErrorMessage: $"Could not start the Windows shutdown command: {exception.Message}");
        }
    }
}
