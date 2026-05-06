using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class FileStabilityServiceTests
{
    private readonly FileStabilityService _service = new();

    [Fact]
    public async Task CheckAsync_ShouldThrowArgumentExceptionForEmptyPath()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CheckAsync(string.Empty, TimeSpan.FromMilliseconds(50)));
    }

    [Fact]
    public async Task CheckAsync_ShouldReturnMissingResultForMissingFile()
    {
        var filePath = CreateTempFilePath();

        var result = await _service.CheckAsync(filePath, TimeSpan.FromMilliseconds(10));

        Assert.False(result.Exists);
        Assert.False(result.IsReadable);
        Assert.False(result.IsStable);
        Assert.Equal("Datei existiert nicht.", result.Message);
    }

    [Fact]
    public async Task CheckAsync_ShouldMarkExistingReadableFileAsStable()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "stable");

        var result = await _service.CheckAsync(filePath, TimeSpan.FromMilliseconds(50));

        Assert.True(result.Exists);
        Assert.True(result.IsReadable);
        Assert.True(result.IsStable);
        Assert.Equal(6, result.FileSizeBytes);
    }

    [Fact]
    public async Task CheckAsync_ShouldDetectSizeChangeDuringStabilityDuration()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "one");

        var checkTask = _service.CheckAsync(filePath, TimeSpan.FromMilliseconds(100));
        await Task.Delay(TimeSpan.FromMilliseconds(25));
        await File.AppendAllTextAsync(filePath, "two");

        var result = await checkTask;

        Assert.True(result.Exists);
        Assert.True(result.IsReadable);
        Assert.False(result.IsStable);
        Assert.Equal("Dateigröße hat sich während der Prüfung geändert.", result.Message);
    }

    [Fact]
    public async Task WaitUntilStableAsync_ShouldWaitUntilFileIsStable()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "start");

        var modifierTask = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            await File.AppendAllTextAsync(filePath, "1");
            await Task.Delay(TimeSpan.FromMilliseconds(70));
            await File.AppendAllTextAsync(filePath, "2");
        });

        var result = await _service.WaitUntilStableAsync(
            filePath,
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromMilliseconds(20));

        await modifierTask;

        Assert.True(result.Exists);
        Assert.True(result.IsReadable);
        Assert.True(result.IsStable);
    }

    [Fact]
    public async Task WaitUntilStableAsync_ShouldReturnLastUnstableResultAfterTimeout()
    {
        var filePath = CreateTempFilePath();

        var result = await _service.WaitUntilStableAsync(
            filePath,
            TimeSpan.FromMilliseconds(10),
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(20));

        Assert.False(result.Exists);
        Assert.False(result.IsStable);
        Assert.Equal("Datei existiert nicht.", result.Message);
    }

    [Fact]
    public async Task CheckAsync_ShouldMarkLockedFileAsUnreadable()
    {
        var filePath = CreateTempFilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "locked");

        await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var result = await _service.CheckAsync(filePath, TimeSpan.FromMilliseconds(10));

        Assert.True(result.Exists);
        Assert.False(result.IsReadable);
        Assert.False(result.IsStable);
        Assert.Contains("Datei ist nicht lesbar", result.Message);
    }

    [Fact]
    public async Task WaitUntilStableAsync_ShouldRespectCancellationToken()
    {
        var filePath = CreateTempFilePath();
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.WaitUntilStableAsync(
                filePath,
                TimeSpan.FromMilliseconds(10),
                TimeSpan.FromSeconds(5),
                TimeSpan.FromMilliseconds(200),
                cancellationTokenSource.Token));
    }

    private static string CreateTempFilePath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "XdtDeviceBridgeTests",
            Guid.NewGuid().ToString("N"),
            "stability.xdt");
    }
}
