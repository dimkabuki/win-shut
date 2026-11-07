using System.Globalization;
using System.Text;
using ShutdownTimer.App.Application;

namespace ShutdownTimer.App.WindowsIntegration;

public sealed class ShutdownHistoryWriter
{
    private static readonly UTF8Encoding Utf8WithoutBom = new(encoderShouldEmitUTF8Identifier: false);

    private readonly string _filePath;
    private readonly Func<DateTimeOffset> _getLocalTimestamp;

    public ShutdownHistoryWriter(string filePath, Func<DateTimeOffset>? getLocalTimestamp = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        _filePath = filePath;
        _getLocalTimestamp = getLocalTimestamp ?? (() => DateTimeOffset.Now);
    }

    public static string GetDefaultFilePath()
    {
        var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(documentsDirectory))
        {
            throw new InvalidOperationException("The Windows Documents folder could not be resolved.");
        }

        return Path.Combine(documentsDirectory, "win-shut.log");
    }

    public async Task AppendAsync(int durationMinutes)
    {
        if (durationMinutes < DurationSelectionModel.MinimumMinutes
            || durationMinutes > DurationSelectionModel.MaximumMinutes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMinutes),
                durationMinutes,
                $"Duration must be between {DurationSelectionModel.MinimumMinutes} and "
                + $"{DurationSelectionModel.MaximumMinutes} whole minutes.");
        }

        var timestamp = _getLocalTimestamp().ToString("O", CultureInfo.InvariantCulture);
        var record = string.Concat(
            timestamp,
            ",",
            durationMinutes.ToString(CultureInfo.InvariantCulture),
            Environment.NewLine);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.AppendAllTextAsync(_filePath, record, Utf8WithoutBom).ConfigureAwait(false);
    }
}
