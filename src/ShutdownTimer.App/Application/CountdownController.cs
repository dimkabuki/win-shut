using ShutdownTimer.App.WindowsIntegration;

namespace ShutdownTimer.App.Application;

public sealed class CountdownController
{
    private readonly ShutdownScheduler _shutdownScheduler;
    private readonly IMonotonicClock _clock;
    private TimeSpan _duration;
    private TimeSpan _startedAtElapsed;

    public CountdownController(ShutdownScheduler shutdownScheduler, IMonotonicClock clock)
    {
        _shutdownScheduler = shutdownScheduler ?? throw new ArgumentNullException(nameof(shutdownScheduler));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public CountdownControllerState State { get; private set; } = CountdownControllerState.Idle;

    public string? ErrorMessage { get; private set; }

    public TimeSpan Remaining
    {
        get
        {
            if (State == CountdownControllerState.ZeroReached)
            {
                return TimeSpan.Zero;
            }

            if (State != CountdownControllerState.ActiveCountdown)
            {
                return TimeSpan.Zero;
            }

            var elapsedSinceStart = _clock.Elapsed - _startedAtElapsed;
            var remaining = _duration - elapsedSinceStart;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    public async Task<bool> StartAsync(int durationMinutes)
    {
        if (State != CountdownControllerState.Idle)
        {
            return false;
        }

        State = CountdownControllerState.Starting;
        ErrorMessage = null;

        ShutdownScheduleResult result;
        try
        {
            result = await _shutdownScheduler.ScheduleAsync(durationMinutes).ConfigureAwait(false);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            State = CountdownControllerState.Idle;
            ErrorMessage = exception.Message;
            return false;
        }

        if (!result.Succeeded)
        {
            State = CountdownControllerState.Idle;
            ErrorMessage = result.ErrorMessage ?? "Windows could not schedule the shutdown.";
            return false;
        }

        _duration = TimeSpan.FromMinutes(durationMinutes);
        _startedAtElapsed = _clock.Elapsed;
        State = CountdownControllerState.ActiveCountdown;
        return true;
    }

    public void Refresh()
    {
        if (State == CountdownControllerState.ActiveCountdown && Remaining <= TimeSpan.Zero)
        {
            State = CountdownControllerState.ZeroReached;
        }
    }
}
