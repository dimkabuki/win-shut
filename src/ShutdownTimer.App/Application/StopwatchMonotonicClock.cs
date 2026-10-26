using System.Diagnostics;

namespace ShutdownTimer.App.Application;

public sealed class StopwatchMonotonicClock : IMonotonicClock
{
    private readonly Stopwatch _stopwatch = new();

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Start() => _stopwatch.Start();

    public void Reset() => _stopwatch.Reset();
}
