using System.ComponentModel;
using System.Diagnostics;
using ShutdownTimer.App.Application;
using ShutdownTimer.App.WindowsIntegration;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class CountdownControllerTests
{
    [Fact]
    public async Task StartsCountdownOnlyAfterSuccessfulSchedule()
    {
        var clock = new FakeClock();
        var launcher = new FakeProcessLauncher();
        var controller = CreateController(clock, launcher);

        var started = await controller.StartAsync(15);

        Assert.True(started);
        Assert.Equal(CountdownControllerState.ActiveCountdown, controller.State);
        Assert.Equal(TimeSpan.FromMinutes(15), controller.Remaining);
        Assert.Null(controller.ErrorMessage);
    }

    [Fact]
    public async Task StartsCountdownForCustomWholeMinuteDuration()
    {
        var clock = new FakeClock();
        var controller = CreateController(clock, new FakeProcessLauncher());

        var started = await controller.StartAsync(37);

        Assert.True(started);
        Assert.Equal(TimeSpan.FromMinutes(37), controller.Remaining);
    }

    [Fact]
    public async Task FailedScheduleLeavesControllerIdleWithoutCountdown()
    {
        var clock = new FakeClock();
        var launcher = new FakeProcessLauncher { ExitCode = 1 };
        var controller = CreateController(clock, launcher);

        var started = await controller.StartAsync(30);

        Assert.False(started);
        Assert.Equal(CountdownControllerState.Idle, controller.State);
        Assert.Equal(TimeSpan.Zero, controller.Remaining);
        Assert.Contains("exit code 1", controller.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task LaunchExceptionLeavesControllerIdle()
    {
        var clock = new FakeClock();
        var launcher = new FakeProcessLauncher
        {
            ExceptionToThrow = new Win32Exception("launch denied")
        };
        var controller = CreateController(clock, launcher);

        var started = await controller.StartAsync(15);

        Assert.False(started);
        Assert.Equal(CountdownControllerState.Idle, controller.State);
        Assert.Contains("Could not start", controller.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnexpectedSchedulingExceptionLeavesControllerIdleAndReportsFailure()
    {
        var clock = new FakeClock();
        var launcher = new FakeProcessLauncher
        {
            ExceptionToThrow = new NotSupportedException("process launcher unavailable")
        };
        var controller = CreateController(clock, launcher);

        var started = await controller.StartAsync(15);

        Assert.False(started);
        Assert.Equal(CountdownControllerState.Idle, controller.State);
        Assert.Contains("Could not schedule Windows shutdown", controller.ErrorMessage, StringComparison.Ordinal);
        Assert.Contains("process launcher unavailable", controller.ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RemainingUsesElapsedTimeRatherThanTickCount()
    {
        var clock = new FakeClock();
        var controller = CreateController(clock, new FakeProcessLauncher());

        Assert.True(await controller.StartAsync(5));
        clock.Advance(TimeSpan.FromSeconds(17));
        Assert.Equal(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(17), controller.Remaining);

        clock.Advance(TimeSpan.FromSeconds(43));
        Assert.Equal(TimeSpan.FromMinutes(4), controller.Remaining);
    }

    [Fact]
    public async Task RefreshAtZeroMovesToStableZeroState()
    {
        var clock = new FakeClock();
        var controller = CreateController(clock, new FakeProcessLauncher());

        Assert.True(await controller.StartAsync(5));
        clock.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(10));

        Assert.Equal(TimeSpan.Zero, controller.Remaining);
        controller.Refresh();

        Assert.Equal(CountdownControllerState.ZeroReached, controller.State);
        Assert.Equal(TimeSpan.Zero, controller.Remaining);
        controller.Refresh();
        Assert.Equal(CountdownControllerState.ZeroReached, controller.State);
    }

    [Fact]
    public async Task RejectsDuplicateStartWhileStartingOrActive()
    {
        var clock = new FakeClock();
        var launcher = new FakeProcessLauncher { Pending = true };
        var controller = CreateController(clock, launcher);

        var firstStart = controller.StartAsync(15);
        Assert.Equal(CountdownControllerState.Starting, controller.State);
        Assert.False(await controller.StartAsync(30));
        Assert.Equal(1, launcher.InvocationCount);

        launcher.Complete(0);
        Assert.True(await firstStart);
        Assert.False(await controller.StartAsync(30));
        Assert.Equal(1, launcher.InvocationCount);
    }

    [Fact]
    public async Task InvalidDurationLeavesControllerIdle()
    {
        var clock = new FakeClock();
        var launcher = new FakeProcessLauncher();
        var controller = CreateController(clock, launcher);

        Assert.False(await controller.StartAsync(4));

        Assert.Equal(CountdownControllerState.Idle, controller.State);
        Assert.NotNull(controller.ErrorMessage);
        Assert.Equal(0, launcher.InvocationCount);
    }

    private static CountdownController CreateController(FakeClock clock, FakeProcessLauncher launcher) =>
        new(new ShutdownScheduler(launcher), clock);

    private sealed class FakeClock : IMonotonicClock
    {
        public TimeSpan Elapsed { get; private set; }

        public void Advance(TimeSpan duration) => Elapsed += duration;
    }

    private sealed class FakeProcessLauncher : IProcessLauncher
    {
        private readonly TaskCompletionSource<int> _completion = new();

        public int ExitCode { get; init; }

        public Exception? ExceptionToThrow { get; init; }

        public bool Pending { get; init; }

        public int InvocationCount { get; private set; }

        public Task<int> RunAsync(ProcessStartInfo startInfo)
        {
            InvocationCount++;
            if (ExceptionToThrow is not null)
            {
                return Task.FromException<int>(ExceptionToThrow);
            }

            return Pending ? _completion.Task : Task.FromResult(ExitCode);
        }

        public void Complete(int exitCode) => _completion.SetResult(exitCode);
    }
}
