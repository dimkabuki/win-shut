using System.Globalization;

namespace ShutdownTimer.App.Application;

public static class CountdownFormatter
{
    private static readonly TimeSpan SecondsDisplayThreshold = TimeSpan.FromSeconds(180);

    public static string Format(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            return "00:00";
        }

        if (remaining > SecondsDisplayThreshold)
        {
            var wholeMinutes = Math.Ceiling(remaining.TotalMinutes);
            return string.Concat(wholeMinutes.ToString("0", CultureInfo.InvariantCulture), " min");
        }

        return remaining.ToString(@"mm\:ss", CultureInfo.InvariantCulture);
    }
}
