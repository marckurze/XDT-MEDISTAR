namespace XdtDeviceBridge.Infrastructure;

public sealed class DirectorySnapshotService
{
    private static readonly StringComparer FilePathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public DirectorySnapshotResult GetSnapshot(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path must not be empty.", nameof(directoryPath));
        }

        if (!Directory.Exists(directoryPath))
        {
            return new DirectorySnapshotResult(
                DirectoryPath: directoryPath,
                Exists: false,
                Files: Array.Empty<DirectoryFileSnapshot>(),
                Message: "Ordner existiert nicht.");
        }

        var directoryInfo = new DirectoryInfo(directoryPath);
        var files = directoryInfo
            .EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .Select(file => new DirectoryFileSnapshot(
                FilePath: file.FullName,
                FileName: file.Name,
                FileSizeBytes: file.Length,
                LastWriteTimeUtc: file.LastWriteTimeUtc,
                Extension: file.Extension.ToLowerInvariant()))
            .OrderBy(file => file.LastWriteTimeUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, FilePathComparer)
            .ToList();

        return new DirectorySnapshotResult(
            DirectoryPath: directoryPath,
            Exists: true,
            Files: files,
            Message: null);
    }

    public IReadOnlyList<DirectoryFileSnapshot> GetNewFiles(
        DirectorySnapshotResult oldSnapshot,
        DirectorySnapshotResult newSnapshot)
    {
        ArgumentNullException.ThrowIfNull(oldSnapshot);
        ArgumentNullException.ThrowIfNull(newSnapshot);

        var oldFilePaths = oldSnapshot.Files
            .Select(file => file.FilePath)
            .ToHashSet(FilePathComparer);

        return newSnapshot.Files
            .Where(file => !oldFilePaths.Contains(file.FilePath))
            .OrderBy(file => file.LastWriteTimeUtc)
            .ThenBy(file => file.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(file => file.FilePath, FilePathComparer)
            .ToList();
    }
}
