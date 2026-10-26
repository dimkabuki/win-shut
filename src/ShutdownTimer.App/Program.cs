using ShutdownTimer.App.Application;
using ShutdownTimer.App.UI;
using ShutdownTimer.App.WindowsIntegration;
using System.Windows.Forms;

namespace ShutdownTimer.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var durationModel = new DurationSelectionModel();
        var scheduler = new ShutdownScheduler(new SystemProcessLauncher());
        var controller = new CountdownController(scheduler, new StopwatchMonotonicClock());

        System.Windows.Forms.Application.Run(new SetupForm(durationModel, controller));
    }
}
