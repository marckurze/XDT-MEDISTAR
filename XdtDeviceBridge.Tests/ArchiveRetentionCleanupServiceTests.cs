using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ArchiveRetentionCleanupServiceTests
{
    private static readonly DateTime NowUtc = new(2026, 5, 6, 12, 0, 0, DateTimeKind.Utc);

    private readonly ArchiveRetentionCleanupService _service = new();

    [Fact]
    public void PreviewCleanup_ShouldFindOldFiles()
    {
        var archiveFolder = CreateTempFolder();
        var oldFile = CreateFile(archiveFolder, "old.xdt", NowUtc.AddDays(-31));
        CreateFile(archiveFolder, "new.xdt", NowUtc.AddDays(-5));

        var preview = _service.PreviewCleanup(archiveFolder, retentionDays: 30, NowUtc);

        Assert.False(preview.HasErrors);
        Assert.Contains(oldFile, preview.FilesEligibleForDeletion);
        Assert.Single(preview.FilesEligibleForDeletion);
    }

    [Fact]
    public void PreviewCleanup_ShouldIgnoreNewFiles()
    {
        var archiveFolder = CreateTempFolder();
        var newFile = CreateFile(archiveFolder, "new.xdt", NowUtc.AddDays(-5));

        var preview = _service.PreviewCleanup(archiveFolder, retentionDays: 30, NowUtc);

        Assert.DoesNotContain(newFile, preview.FilesEligibleForDeletion);
        Assert.Empty(preview.FilesEligibleForDeletion);
    }

    [Fact]
    public void PreviewCleanup_ShouldNotDeleteFiles()
    {
        var archiveFolder = CreateTempFolder();
        var oldFile = CreateFile(archiveFolder, "old.xdt", NowUtc.AddDays(-31));

        var preview = _service.PreviewCleanup(archiveFolder, retentionDays: 30, NowUtc);

        Assert.Contains(oldFile, preview.FilesEligibleForDeletion);
        Assert.True(File.Exists(oldFile));
    }

    [Fact]
    public void DeleteExpiredArchiveFiles_DryRunShouldNotDeleteFiles()
    {
        var archiveFolder = CreateTempFolder();
        var oldFile = CreateFile(archiveFolder, "old.xdt", NowUtc.AddDays(-31));

        var preview = _service.DeleteExpiredArchiveFiles(archiveFolder, retentionDays: 30, NowUtc, dryRun: true);

        Assert.False(preview.HasErrors);
        Assert.Contains(oldFile, preview.FilesEligibleForDeletion);
        Assert.True(File.Exists(oldFile));
    }

    [Fact]
    public void DeleteExpiredArchiveFiles_ShouldDeleteOnlyOldFiles()
    {
        var archiveFolder = CreateTempFolder();
        var oldFile = CreateFile(archiveFolder, "old.xdt", NowUtc.AddDays(-31));
        var newFile = CreateFile(archiveFolder, "new.xdt", NowUtc.AddDays(-5));

        var preview = _service.DeleteExpiredArchiveFiles(archiveFolder, retentionDays: 30, NowUtc, dryRun: false);

        Assert.False(preview.HasErrors);
        Assert.False(File.Exists(oldFile));
        Assert.True(File.Exists(newFile));
    }

    [Fact]
    public void PreviewCleanup_ShouldThrowArgumentExceptionForEmptyArchiveFolder()
    {
        Assert.Throws<ArgumentException>(() => _service.PreviewCleanup(string.Empty, retentionDays: 30, NowUtc));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void PreviewCleanup_ShouldThrowArgumentExceptionForInvalidRetentionDays(int retentionDays)
    {
        Assert.Throws<ArgumentException>(() => _service.PreviewCleanup(CreateTempFolder(), retentionDays, NowUtc));
    }

    [Fact]
    public void DeleteExpiredArchiveFiles_ShouldRejectDangerousFolder()
    {
        var root = Path.GetPathRoot(Path.GetTempPath())!;

        var preview = _service.DeleteExpiredArchiveFiles(root, retentionDays: 30, NowUtc, dryRun: false);

        Assert.True(preview.HasErrors);
        Assert.Contains(preview.Issues, issue => issue == "Folder path must not be a drive or share root.");
    }

    [Fact]
    public void PreviewCleanup_ShouldReportMissingArchiveFolderWithoutError()
    {
        var archiveFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "missing");

        var preview = _service.PreviewCleanup(archiveFolder, retentionDays: 30, NowUtc);

        Assert.False(preview.HasErrors);
        Assert.Contains("Archivordner existiert nicht.", preview.Issues);
        Assert.Empty(preview.FilesEligibleForDeletion);
    }

    private static string CreateFile(string folder, string fileName, DateTime lastWriteTimeUtc)
    {
        var path = Path.Combine(folder, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "archive");
        File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        return path;
    }

    private static string CreateTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
