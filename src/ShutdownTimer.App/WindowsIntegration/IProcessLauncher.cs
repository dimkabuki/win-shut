using System.Diagnostics;

namespace ShutdownTimer.App.WindowsIntegration;

public interface IProcessLauncher
{
    Task<int> RunAsync(ProcessStartInfo startInfo);
}
