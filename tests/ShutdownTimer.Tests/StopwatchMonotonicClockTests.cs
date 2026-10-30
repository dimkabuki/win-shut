using ShutdownTimer.App.Application;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class StopwatchMonotonicClockTests
{
    [Fact]
    public void ElapsedAdvancesWithoutAnExplicitStartCall()
    {
        var clock = new StopwatchMonotonicClock();
        var initialElapsed = clock.Elapsed;

        var advanced = SpinWait.SpinUntil(
            () => clock.Elapsed > initialElapsed,
            TimeSpan.FromSeconds(1));

        Assert.True(advanced, "The production clock must start when it is constructed.");
        Assert.True(clock.Elapsed >= initialElapsed);
    }
}
