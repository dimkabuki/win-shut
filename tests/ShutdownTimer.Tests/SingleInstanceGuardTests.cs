using ShutdownTimer.App.WindowsIntegration;

namespace ShutdownTimer.Tests;

public sealed class SingleInstanceGuardTests
{
    [Fact]
    public void TryAcquireRejectsSecondInstanceAndReleasesMutexOnDispose()
    {
        var mutexName = $@"Local\win-shut-test-{Guid.NewGuid():N}";
        var first = SingleInstanceGuard.TryAcquire(mutexName);

        Assert.NotNull(first);
        Assert.Null(SingleInstanceGuard.TryAcquire(mutexName));

        first.Dispose();

        using var reacquired = SingleInstanceGuard.TryAcquire(mutexName);
        Assert.NotNull(reacquired);
    }

    [Fact]
    public void TryAcquireRejectsBlankMutexName()
    {
        Assert.Throws<ArgumentException>(() => SingleInstanceGuard.TryAcquire(" "));
    }
}
