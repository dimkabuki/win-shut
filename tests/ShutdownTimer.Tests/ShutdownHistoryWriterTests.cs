using ShutdownTimer.App.WindowsIntegration;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class ShutdownHistoryWriterTests
{
    [Fact]
    public async Task CreatesMissingFileAndWritesLocalTimestampAndDuration()
    {
        var directory = CreateTemporaryDirectory();
        var filePath = Path.Combine(directory, "nested", "win-shut.log");
        var timestamp = new DateTimeOffset(2026, 11, 7, 22, 15, 30, TimeSpan.FromHours(1));
        var writer = new ShutdownHistoryWriter(filePath, () => timestamp);

        try
        {
            await writer.AppendAsync(37);

            Assert.True(File.Exists(filePath));
            var lines = await File.ReadAllLinesAsync(filePath);
            Assert.Equal(new[] { $"{timestamp:O},37" }, lines);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task AppendsRecordsWithoutReplacingExistingEntries()
    {
        var directory = CreateTemporaryDirectory();
        var filePath = Path.Combine(directory, "win-shut.log");
        var timestamps = new Queue<DateTimeOffset>(
        [
            new DateTimeOffset(2026, 11, 7, 22, 15, 30, TimeSpan.FromHours(1)),
            new DateTimeOffset(2026, 11, 7, 22, 16, 45, TimeSpan.FromHours(1))
        ]);
        var writer = new ShutdownHistoryWriter(filePath, () => timestamps.Dequeue());

        try
        {
            await writer.AppendAsync(15);
            await writer.AppendAsync(180);

            var lines = await File.ReadAllLinesAsync(filePath);
            Assert.Equal(
                new[]
                {
                    $"{new DateTimeOffset(2026, 11, 7, 22, 15, 30, TimeSpan.FromHours(1)):O},15",
                    $"{new DateTimeOffset(2026, 11, 7, 22, 16, 45, TimeSpan.FromHours(1)):O},180"
                },
                lines);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(4)]
    [InlineData(181)]
    public async Task RejectsOutOfRangeDurationWithoutCreatingFile(int durationMinutes)
    {
        var directory = CreateTemporaryDirectory();
        var filePath = Path.Combine(directory, "win-shut.log");
        var writer = new ShutdownHistoryWriter(filePath);

        try
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => writer.AppendAsync(durationMinutes));

            Assert.False(File.Exists(filePath));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void DefaultFilePathUsesDocumentsFolder()
    {
        var documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Assert.Equal(
            Path.Combine(documentsDirectory, "win-shut.log"),
            ShutdownHistoryWriter.GetDefaultFilePath());
    }

    private static string CreateTemporaryDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), $"win-shut-history-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
