using ShutdownTimer.App.Application;
using ShutdownTimer.App.UI;
using ShutdownTimer.App.WindowsIntegration;
using System.Windows.Forms;

namespace ShutdownTimer.App;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Local\dimkabuki.win-shut";

    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var instanceGuard = SingleInstanceGuard.TryAcquire(SingleInstanceMutexName);
        if (instanceGuard is null)
        {
            MessageBox.Show(
                "win-shut is already running. Use the existing notification-area icon to access it.",
                "win-shut",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var durationModel = new DurationSelectionModel();
        var scheduler = new ShutdownScheduler(new SystemProcessLauncher());
        var controller = new CountdownController(scheduler, new StopwatchMonotonicClock());

        using var setupForm = new SetupForm(durationModel, controller);
        using var overlayForm = new CountdownOverlayForm();
        using var trayController = new TrayController(setupForm, overlayForm, controller);
        System.Windows.Forms.Application.Run(trayController);
    }
}
