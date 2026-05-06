using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class DirectorySnapshotServiceTests
{
    private readonly DirectorySnapshotService _service = new();

    [Fact]
    public void GetSnapshot_ShouldThrowArgumentExceptionForEmptyPath()
    {
        Assert.Throws<ArgumentException>(() => _service.GetSnapshot(string.Empty));
    }

    [Fact]
    public void GetSnapshot_ShouldReturnMissingResultForMissingDirectory()
    {
        var directoryPath = CreateTempDirectoryPath();

        var snapshot = _service.GetSnapshot(directoryPath);

        Assert.False(snapshot.Exists);
        Assert.Empty(snapshot.Files);
        Assert.Equal("Ordner existiert nicht.", snapshot.Message);
    }

    [Fact]
    public void GetSnapshot_ShouldReturnEmptyFilesForExistingEmptyDirectory()
    {
        var directoryPath = CreateTempDirectory();

        var snapshot = _service.GetSnapshot(directoryPath);

        Assert.True(snapshot.Exists);
        Assert.Empty(snapshot.Files);
        Assert.Null(snapshot.Message);
    }

    [Fact]
    public void GetSnapshot_ShouldListFilesWithNameExtensionAndSize()
    {
        var directoryPath = CreateTempDirectory();
        var filePath = Path.Combine(directoryPath, "SAMPLE.XML");
        File.WriteAllText(filePath, "abc");

        var snapshot = _service.GetSnapshot(directoryPath);

        var file = Assert.Single(snapshot.Files);
        Assert.Equal(filePath, file.FilePath);
        Assert.Equal("SAMPLE.XML", file.FileName);
        Assert.Equal(".xml", file.Extension);
        Assert.Equal(3, file.FileSizeBytes);
    }

    [Fact]
    public void GetSnapshot_ShouldIgnoreSubdirectories()
    {
        var directoryPath = CreateTempDirectory();
        var subDirectoryPath = Path.Combine(directoryPath, "sub");
        Directory.CreateDirectory(subDirectoryPath);
        File.WriteAllText(Path.Combine(subDirectoryPath, "nested.xml"), "nested");
        File.WriteAllText(Path.Combine(directoryPath, "root.xml"), "root");

        var snapshot = _service.GetSnapshot(directoryPath);

        var file = Assert.Single(snapshot.Files);
        Assert.Equal("root.xml", file.FileName);
    }

    [Fact]
    public void GetNewFiles_ShouldDetectAddedFile()
    {
        var directoryPath = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directoryPath, "old.xml"), "old");
        var oldSnapshot = _service.GetSnapshot(directoryPath);

        File.WriteAllText(Path.Combine(directoryPath, "new.xml"), "new");
        var newSnapshot = _service.GetSnapshot(directoryPath);

        var newFiles = _service.GetNewFiles(oldSnapshot, newSnapshot);

        var file = Assert.Single(newFiles);
        Assert.Equal("new.xml", file.FileName);
    }

    [Fact]
    public void GetNewFiles_ShouldReturnEmptyListForSameSnapshot()
    {
        var directoryPath = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directoryPath, "same.xml"), "same");
        var snapshot = _service.GetSnapshot(directoryPath);

        var newFiles = _service.GetNewFiles(snapshot, snapshot);

        Assert.Empty(newFiles);
    }

    [Fact]
    public void GetNewFiles_ShouldReturnNewFilesInStableOrder()
    {
        var directoryPath = CreateTempDirectory();
        File.WriteAllText(Path.Combine(directoryPath, "old.xml"), "old");
        var oldSnapshot = _service.GetSnapshot(directoryPath);

        var laterFilePath = Path.Combine(directoryPath, "later.xml");
        var earlierFilePath = Path.Combine(directoryPath, "earlier.xml");
        File.WriteAllText(laterFilePath, "later");
        File.WriteAllText(earlierFilePath, "earlier");
        File.SetLastWriteTimeUtc(laterFilePath, new DateTime(2026, 6, 1, 12, 0, 2, DateTimeKind.Utc));
        File.SetLastWriteTimeUtc(earlierFilePath, new DateTime(2026, 6, 1, 12, 0, 1, DateTimeKind.Utc));
        var newSnapshot = _service.GetSnapshot(directoryPath);

        var newFiles = _service.GetNewFiles(oldSnapshot, newSnapshot);

        Assert.Equal(new[] { "earlier.xml", "later.xml" }, newFiles.Select(file => file.FileName));
    }

    [Fact]
    public void GetNewFiles_ShouldThrowArgumentNullExceptionForNullSnapshots()
    {
        var snapshot = new DirectorySnapshotResult(
            DirectoryPath: "C:\\Temp",
            Exists: true,
            Files: Array.Empty<DirectoryFileSnapshot>(),
            Message: null);

        Assert.Throws<ArgumentNullException>(() => _service.GetNewFiles(null!, snapshot));
        Assert.Throws<ArgumentNullException>(() => _service.GetNewFiles(snapshot, null!));
    }

    private static string CreateTempDirectory()
    {
        var directoryPath = CreateTempDirectoryPath();
        Directory.CreateDirectory(directoryPath);
        return directoryPath;
    }

    private static string CreateTempDirectoryPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "XdtDeviceBridgeTests",
            Guid.NewGuid().ToString("N"));
    }
}
