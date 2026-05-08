using System.Diagnostics;

namespace XdtDeviceBridge.Infrastructure;

public sealed class FileStabilityService
{
    public async Task<FileStabilityResult> CheckAsync(
        string filePath,
        TimeSpan stabilityDuration,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }

        if (stabilityDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(stabilityDuration), "Stability duration must not be negative.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        var initialRead = TryReadFileMetadata(filePath);
        if (!initialRead.Result.Exists || !initialRead.Result.IsReadable)
        {
            return initialRead.Result;
        }

        await Task.Delay(stabilityDuration, cancellationToken).ConfigureAwait(false);

        var secondRead = TryReadFileMetadata(filePath);
        if (!secondRead.Result.Exists || !secondRead.Result.IsReadable)
        {
            return secondRead.Result;
        }

        if (initialRead.Result.FileSizeBytes != secondRead.Result.FileSizeBytes)
        {
            return new FileStabilityResult(
                FilePath: filePath,
                Exists: true,
                IsReadable: true,
                IsStable: false,
                FileSizeBytes: secondRead.Result.FileSizeBytes,
                Message: "Dateigröße hat sich während der Prüfung geändert.");
        }

        if (initialRead.LastWriteTimeUtc != secondRead.LastWriteTimeUtc)
        {
            return new FileStabilityResult(
                FilePath: filePath,
                Exists: true,
                IsReadable: true,
                IsStable: false,
                FileSizeBytes: secondRead.Result.FileSizeBytes,
                Message: "Änderungszeitpunkt hat sich während der Prüfung geändert.");
        }

        return new FileStabilityResult(
            FilePath: filePath,
            Exists: true,
            IsReadable: true,
            IsStable: true,
            FileSizeBytes: secondRead.Result.FileSizeBytes,
            Message: "Datei ist stabil und lesbar.");
    }

    public async Task<FileStabilityResult> WaitUntilStableAsync(
        string filePath,
        TimeSpan stabilityDuration,
        TimeSpan timeout,
        TimeSpan pollInterval,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }

        if (stabilityDuration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(stabilityDuration), "Stability duration must not be negative.");
        }

        if (timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be greater than zero.");
        }

        if (pollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval), "Poll interval must be greater than zero.");
        }

        var stopwatch = Stopwatch.StartNew();
        FileStabilityResult? lastResult = null;

        while (stopwatch.Elapsed < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lastResult = await CheckAsync(filePath, stabilityDuration, cancellationToken).ConfigureAwait(false);
            if (lastResult.IsStable)
            {
                return lastResult;
            }

            if (stopwatch.Elapsed >= timeout)
            {
                break;
            }

            var remainingTimeout = timeout - stopwatch.Elapsed;
            var delay = remainingTimeout < pollInterval ? remainingTimeout : pollInterval;
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }

        return lastResult ?? new FileStabilityResult(
            FilePath: filePath,
            Exists: File.Exists(filePath),
            IsReadable: false,
            IsStable: false,
            FileSizeBytes: null,
            Message: "Timeout erreicht, bevor die Datei stabil wurde.");
    }

    private static FileMetadataRead TryReadFileMetadata(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new FileMetadataRead(new FileStabilityResult(
                FilePath: filePath,
                Exists: false,
                IsReadable: false,
                IsStable: false,
                FileSizeBytes: null,
                Message: "Datei existiert nicht."), LastWriteTimeUtc: null);
        }

        try
        {
            var fileInfo = new FileInfo(filePath);
            using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete,
                bufferSize: 4096,
                FileOptions.Asynchronous);

            fileInfo.Refresh();
            return new FileMetadataRead(new FileStabilityResult(
                FilePath: filePath,
                Exists: true,
                IsReadable: true,
                IsStable: false,
                FileSizeBytes: stream.Length,
                Message: "Datei ist lesbar."), fileInfo.LastWriteTimeUtc);
        }
        catch (UnauthorizedAccessException ex)
        {
            return new FileMetadataRead(CreateUnreadableResult(filePath, ex), LastWriteTimeUtc: null);
        }
        catch (IOException ex)
        {
            return new FileMetadataRead(CreateUnreadableResult(filePath, ex), LastWriteTimeUtc: null);
        }
    }

    private static FileStabilityResult CreateUnreadableResult(string filePath, Exception exception)
    {
        return new FileStabilityResult(
            FilePath: filePath,
            Exists: File.Exists(filePath),
            IsReadable: false,
            IsStable: false,
            FileSizeBytes: null,
            Message: $"Datei ist nicht lesbar: {exception.Message}");
    }

    private sealed record FileMetadataRead(FileStabilityResult Result, DateTime? LastWriteTimeUtc);
}
