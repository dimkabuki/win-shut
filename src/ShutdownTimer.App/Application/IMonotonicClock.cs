namespace ShutdownTimer.App.Application;

public interface IMonotonicClock
{
    TimeSpan Elapsed { get; }
}
