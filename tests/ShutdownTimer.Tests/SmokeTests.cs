using Xunit;

namespace ShutdownTimer.Tests;

public sealed class SmokeTests
{
    [Fact]
    public void Test_runner_is_configured()
    {
        Assert.Equal(2, 1 + 1);
    }
}
