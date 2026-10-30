using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using ShutdownTimer.App.WindowsIntegration;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class ShutdownSchedulerTests
{
    [Theory]
    [InlineData(5, 300)]
    [InlineData(6, 360)]
    [InlineData(15, 900)]
    [InlineData(30, 1800)]
    [InlineData(37, 2220)]
    [InlineData(45, 2700)]
    [InlineData(179, 10740)]
    [InlineData(180, 10800)]
    public async Task BuildsSystemShutdownCommandWithSeparateArguments(int durationMinutes, int durationSeconds)
    {
        var launcher = new FakeProcessLauncher();
        var scheduler = new ShutdownScheduler(launcher);

        var result = await scheduler.ScheduleAsync(durationMinutes);

        Assert.True(result.Succeeded);
        Assert.Equal(0, result.ExitCode);
        Assert.Null(result.ErrorMessage);

        var startInfo = Assert.IsType<ProcessStartInfo>(launcher.CapturedStartInfo);
        Assert.Equal(Path.Combine(Environment.SystemDirectory, "shutdown.exe"), startInfo.FileName);
        Assert.False(startInfo.UseShellExecute);
        Assert.True(startInfo.CreateNoWindow);
        Assert.Equal(
            new[] { "/s", "/f", "/t", durationSeconds.ToString(CultureInfo.InvariantCulture) },
            startInfo.ArgumentList);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(181)]
    public async Task RejectsDurationsOutsideSupportedRangeBeforeLaunching(int durationMinutes)
    {
        var launcher = new FakeProcessLauncher();
        var scheduler = new ShutdownScheduler(launcher);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => scheduler.ScheduleAsync(durationMinutes));

        Assert.Null(launcher.CapturedStartInfo);
    }

    [Fact]
    public async Task ReportsNonZeroExitCodeAsFailure()
    {
        var launcher = new FakeProcessLauncher { ExitCode = 5 };
        var scheduler = new ShutdownScheduler(launcher);

        var result = await scheduler.ScheduleAsync(30);

        Assert.False(result.Succeeded);
        Assert.Equal(5, result.ExitCode);
        Assert.Contains("exit code 5", result.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ConvertsProcessLaunchExceptionToFailureResult()
    {
        var launcher = new FakeProcessLauncher
        {
            ExceptionToThrow = new Win32Exception("process launch failed")
        };
        var scheduler = new ShutdownScheduler(launcher);

        var result = await scheduler.ScheduleAsync(15);

        Assert.False(result.Succeeded);
        Assert.Null(result.ExitCode);
        Assert.Contains("Could not start the Windows shutdown command", result.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("process launch failed", result.ErrorMessage, StringComparison.Ordinal);
    }

    private sealed class FakeProcessLauncher : IProcessLauncher
    {
        public ProcessStartInfo? CapturedStartInfo { get; private set; }

        public int ExitCode { get; init; }

        public Exception? ExceptionToThrow { get; init; }

        public Task<int> RunAsync(ProcessStartInfo startInfo)
        {
            CapturedStartInfo = startInfo;

            var exception = ExceptionToThrow;
            return exception is not null
                ? Task.FromException<int>(exception)
                : Task.FromResult(ExitCode);
        }
    }
}
