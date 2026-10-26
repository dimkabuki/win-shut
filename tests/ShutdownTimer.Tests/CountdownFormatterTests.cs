using ShutdownTimer.App.Application;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class CountdownFormatterTests
{
    [Theory]
    [InlineData(181, "4 min")]
    [InlineData(240, "4 min")]
    [InlineData(180, "03:00")]
    [InlineData(61, "01:01")]
    [InlineData(60, "01:00")]
    [InlineData(59, "00:59")]
    [InlineData(30, "00:30")]
    [InlineData(1, "00:01")]
    [InlineData(0, "00:00")]
    [InlineData(-1, "00:00")]
    public void FormatsRemainingTimeAsSpecified(int seconds, string expected)
    {
        Assert.Equal(expected, CountdownFormatter.Format(TimeSpan.FromSeconds(seconds)));
    }

    [Fact]
    public void SwitchesToRoundedMinutesImmediatelyAbove180Seconds()
    {
        Assert.Equal("4 min", CountdownFormatter.Format(TimeSpan.FromMilliseconds(180_001)));
        Assert.Equal("03:00", CountdownFormatter.Format(TimeSpan.FromSeconds(180)));
    }

    [Theory]
    [InlineData(181, "4 min")]
    [InlineData(239, "4 min")]
    [InlineData(240, "4 min")]
    [InlineData(241, "5 min")]
    [InlineData(299, "5 min")]
    [InlineData(300, "5 min")]
    [InlineData(301, "6 min")]
    public void RoundsDurationsAbove180SecondsUpToWholeMinutes(int seconds, string expected)
    {
        Assert.Equal(expected, CountdownFormatter.Format(TimeSpan.FromSeconds(seconds)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-60)]
    [InlineData(-3600)]
    public void NeverFormatsNegativeTime(int seconds)
    {
        var display = CountdownFormatter.Format(TimeSpan.FromSeconds(seconds));

        Assert.Equal("00:00", display);
        Assert.DoesNotContain("-", display);
    }
}
