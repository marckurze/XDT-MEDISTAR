using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProcessedFileArchiveServiceTests
{
    private static readonly DateTime ProcessedAtUtc = new(2026, 5, 3, 14, 15, 0, DateTimeKind.Utc);

    private readonly ProcessedFileArchiveService _service = new();

    [Fact]
    public void ArchiveProcessedFiles_ShouldThrowArgumentExceptionForEmptyArchiveFolder()
    {
        var files = CreateSourceFiles();

        Assert.Throws<ArgumentException>(() => _service.ArchiveProcessedFiles(
            string.Empty,
            "MEDISTAR NIDEK ARK1S",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false));
    }

    [Fact]
    public void ArchiveProcessedFiles_ShouldCopyFilesToExpectedArchiveStructure()
    {
        var archiveFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var result = _service.ArchiveProcessedFiles(
            archiveFolder,
            "MEDISTAR + NIDEK ARK1S",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false);

        Assert.False(result.HasErrors);
        Assert.Equal(2, result.ArchivedFiles.Count);
        Assert.Contains(Path.Combine(archiveFolder, "2026", "05", "03", "MEDISTAR_NIDEK_ARK1S", "AIS", "TestPatient.gdt"), result.ArchivedFiles);
        Assert.Contains(Path.Combine(archiveFolder, "2026", "05", "03", "MEDISTAR_NIDEK_ARK1S", "Device", "ARK1S.xml"), result.ArchivedFiles);
        Assert.All(result.ArchivedFiles, filePath => Assert.True(File.Exists(filePath)));
    }

    [Fact]
    public void ArchiveProcessedFiles_ShouldCreateTargetFolders()
    {
        var archiveFolder = Path.Combine(CreateTempFolder(), "archive", "nested");
        var files = CreateSourceFiles();

        var result = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false);

        Assert.False(result.HasErrors);
        Assert.True(Directory.Exists(Path.Combine(archiveFolder, "2026", "05", "03", "Profil", "AIS")));
        Assert.True(Directory.Exists(Path.Combine(archiveFolder, "2026", "05", "03", "Profil", "Device")));
    }

    [Fact]
    public void ArchiveProcessedFiles_ShouldUseSuffixWhenTargetFileExists()
    {
        var archiveFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var first = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false);
        var second = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false);

        Assert.False(first.HasErrors);
        Assert.False(second.HasErrors);
        Assert.Contains(second.ArchivedFiles, filePath => filePath.EndsWith("TestPatient_1.gdt", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(second.ArchivedFiles, filePath => filePath.EndsWith("ARK1S_1.xml", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ArchiveProcessedFiles_CopyModeShouldKeepOriginalFiles()
    {
        var archiveFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var result = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false);

        Assert.False(result.HasErrors);
        Assert.True(File.Exists(files.AisFilePath));
        Assert.True(File.Exists(files.DeviceFilePath));
    }

    [Fact]
    public void ArchiveProcessedFiles_MoveModeShouldMoveOriginalFiles()
    {
        var archiveFolder = CreateTempFolder();
        var files = CreateSourceFiles();

        var result = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: true);

        Assert.False(result.HasErrors);
        Assert.False(File.Exists(files.AisFilePath));
        Assert.False(File.Exists(files.DeviceFilePath));
        Assert.All(result.ArchivedFiles, filePath => Assert.True(File.Exists(filePath)));
    }

    [Fact]
    public void ArchiveProcessedFiles_MoveModeShouldUseSuffixWhenTargetFileExists()
    {
        var archiveFolder = CreateTempFolder();
        var firstFiles = CreateSourceFiles("TestPatient.gdt", "ARK1S.xml");
        var secondFiles = CreateSourceFiles("TestPatient.gdt", "ARK1S.xml");

        var first = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            firstFiles.AisFilePath,
            firstFiles.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: true);
        var second = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            secondFiles.AisFilePath,
            secondFiles.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: true);

        Assert.False(first.HasErrors);
        Assert.False(second.HasErrors);
        Assert.Contains(second.ArchivedFiles, filePath => filePath.EndsWith("TestPatient_1.gdt", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(second.ArchivedFiles, filePath => filePath.EndsWith("ARK1S_1.xml", StringComparison.OrdinalIgnoreCase));
        Assert.False(File.Exists(secondFiles.AisFilePath));
        Assert.False(File.Exists(secondFiles.DeviceFilePath));
    }

    [Fact]
    public void ArchiveProcessedFiles_ShouldReturnErrorForMissingSourceFile()
    {
        var archiveFolder = CreateTempFolder();
        var files = CreateSourceFiles();
        File.Delete(files.DeviceFilePath);

        var result = _service.ArchiveProcessedFiles(
            archiveFolder,
            "Profil",
            files.AisFilePath,
            files.DeviceFilePath,
            ProcessedAtUtc,
            moveFiles: false);

        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, issue => issue.Contains("Quelldatei fehlt", StringComparison.Ordinal));
        Assert.Single(result.ArchivedFiles);
    }

    private static SourceFiles CreateSourceFiles(string aisFileName = "TestPatient.gdt", string deviceFileName = "ARK1S.xml")
    {
        var folder = CreateTempFolder();
        var aisFilePath = Path.Combine(folder, aisFileName);
        var deviceFilePath = Path.Combine(folder, deviceFileName);
        File.WriteAllText(aisFilePath, "gdt");
        File.WriteAllText(deviceFilePath, "xml");
        return new SourceFiles(aisFilePath, deviceFilePath);
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed record SourceFiles(string AisFilePath, string DeviceFilePath);
}
