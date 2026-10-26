using ShutdownTimer.App.Application;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class CountdownVisualStateTests
{
    [Theory]
    [InlineData(31, false)]
    [InlineData(30, true)]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void ClassifiesFinalSecondsWarning(int seconds, bool expected)
    {
        Assert.Equal(expected, CountdownVisualState.IsFinalSecondsWarningActive(TimeSpan.FromSeconds(seconds)));
    }
}
