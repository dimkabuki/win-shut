namespace ShutdownTimer.App.Application;

public sealed class DurationSelectionModel
{
    private static readonly IReadOnlyList<int> SupportedPresets = Array.AsReadOnly(new[] { 15, 30, 45 });

    public const int DefaultMinutes = 30;
    public const int MinimumMinutes = 5;
    public const int MaximumMinutes = 180;
    public const int StepMinutes = 5;

    public int SelectedMinutes { get; private set; } = DefaultMinutes;

    public bool CanIncrement => SelectedMinutes < MaximumMinutes;

    public bool CanDecrement => SelectedMinutes > MinimumMinutes;

    public static IReadOnlyList<int> Presets => SupportedPresets;

    public void SelectPreset(int minutes)
    {
        if (!SupportedPresets.Contains(minutes))
        {
            throw new ArgumentOutOfRangeException(
                nameof(minutes),
                minutes,
                "Only 15, 30, and 45 minute presets are supported.");
        }

        SetMinutes(minutes);
    }

    public void SetMinutes(int minutes)
    {
        if (minutes < MinimumMinutes || minutes > MaximumMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minutes),
                minutes,
                $"Duration must be between {MinimumMinutes} and {MaximumMinutes} whole minutes.");
        }

        SelectedMinutes = minutes;
    }

    public void Increment()
    {
        if (CanIncrement)
        {
            SetMinutes(Math.Min(SelectedMinutes + StepMinutes, MaximumMinutes));
        }
    }

    public void Decrement()
    {
        if (CanDecrement)
        {
            SetMinutes(Math.Max(SelectedMinutes - StepMinutes, MinimumMinutes));
        }
    }
}
