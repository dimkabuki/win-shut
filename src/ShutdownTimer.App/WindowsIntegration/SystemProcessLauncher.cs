using System.Diagnostics;

namespace ShutdownTimer.App.WindowsIntegration;

public sealed class SystemProcessLauncher : IProcessLauncher
{
    public async Task<int> RunAsync(ProcessStartInfo startInfo)
    {
        ArgumentNullException.ThrowIfNull(startInfo);

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("The process could not be started.");

        await process.WaitForExitAsync().ConfigureAwait(false);
        return process.ExitCode;
    }
}
