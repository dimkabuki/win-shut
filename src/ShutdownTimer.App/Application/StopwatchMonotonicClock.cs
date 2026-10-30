using System.Diagnostics;

namespace ShutdownTimer.App.Application;

public sealed class StopwatchMonotonicClock : IMonotonicClock
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public TimeSpan Elapsed => _stopwatch.Elapsed;
}
