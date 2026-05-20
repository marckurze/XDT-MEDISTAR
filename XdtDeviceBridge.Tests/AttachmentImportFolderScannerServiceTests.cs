using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentImportFolderScannerServiceTests
{
    private readonly AttachmentImportFolderScannerService _service = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Scan_ShouldReturnControlledErrorForEmptyFolder(string folder)
    {
        var result = _service.Scan(folder);

        Assert.False(result.Success);
        Assert.Empty(result.Candidates);
        Assert.Contains("nicht konfiguriert", result.ErrorMessage);
    }

    [Fact]
    public void Scan_ShouldReturnControlledErrorForNullFolder()
    {
        var result = _service.Scan((string)null!);

        Assert.False(result.Success);
        Assert.Empty(result.Candidates);
        Assert.Contains("nicht konfiguriert", result.ErrorMessage);
    }

    [Fact]
    public void Scan_ShouldReturnControlledErrorForMissingFolder()
    {
        var folder = CreateTempFolderPath();

        var result = _service.Scan(folder);

        Assert.False(result.Success);
        Assert.Empty(result.Candidates);
        Assert.Equal(folder, result.ScannedFolder);
        Assert.Contains("existiert nicht", result.ErrorMessage);
    }

    [Fact]
    public void Scan_ShouldRejectUnsafeFolder()
    {
        var root = Path.GetPathRoot(Path.GetTempPath())!;

        var result = _service.Scan(root);

        Assert.False(result.Success);
        Assert.Empty(result.Candidates);
        Assert.Contains("unsicher", result.ErrorMessage);
    }

    [Theory]
    [InlineData("befund.pdf", ".pdf")]
    [InlineData("bild.jpg", ".jpg")]
    [InlineData("bild.jpeg", ".jpeg")]
    [InlineData("bild.png", ".png")]
    [InlineData("scan.tif", ".tif")]
    [InlineData("scan.tiff", ".tiff")]
    [InlineData("dicom.dcm", ".dcm")]
    [InlineData("hinweis.txt", ".txt")]
    [InlineData("befund.xml", ".xml")]
    [InlineData("video.mp4", ".mp4")]
    [InlineData("audio.mp3", ".mp3")]
    [InlineData("signal.wav", ".wav")]
    [InlineData("gross.PDF", ".pdf")]
    public void Scan_ShouldRecognizeSupportedFiles(string fileName, string expectedExtension)
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, fileName);
        File.WriteAllText(filePath, "attachment");

        var result = _service.Scan(folder);

        Assert.True(result.Success);
        var candidate = Assert.Single(result.Candidates);
        Assert.True(candidate.IsSupported);
        Assert.Null(candidate.ErrorMessage);
        Assert.Equal(fileName, candidate.FileName);
        Assert.Equal(expectedExtension, candidate.Extension);
        Assert.Equal(filePath, candidate.FullPath);
        Assert.Equal(10, candidate.SizeBytes);
        Assert.Equal("Nicht geprüft.", candidate.StableStatus);
        Assert.False(candidate.IsStable);
        Assert.True(candidate.LastWriteTimeUtc <= DateTime.UtcNow);
        Assert.Equal(1, result.SupportedCount);
        Assert.Equal(0, result.UnsupportedCount);
    }

    [Fact]
    public void Scan_ShouldMarkUnsupportedFiles()
    {
        var folder = CreateTempFolder();
        File.WriteAllText(Path.Combine(folder, "unknown.exe"), "nope");

        var result = _service.Scan(folder);

        var candidate = Assert.Single(result.Candidates);
        Assert.True(result.Success);
        Assert.False(candidate.IsSupported);
        Assert.Equal(".exe", candidate.Extension);
        Assert.Contains("nicht unterstützt", candidate.ErrorMessage);
        Assert.Equal(0, result.SupportedCount);
        Assert.Equal(1, result.UnsupportedCount);
    }

    [Fact]
    public void Scan_ShouldIgnoreFolders()
    {
        var folder = CreateTempFolder();
        Directory.CreateDirectory(Path.Combine(folder, "subfolder"));
        File.WriteAllText(Path.Combine(folder, "root.pdf"), "root");

        var result = _service.Scan(folder);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("root.pdf", candidate.FileName);
    }

    [Fact]
    public void Scan_ShouldNotSearchRecursively()
    {
        var folder = CreateTempFolder();
        var nestedFolder = Path.Combine(folder, "nested");
        Directory.CreateDirectory(nestedFolder);
        File.WriteAllText(Path.Combine(nestedFolder, "nested.pdf"), "nested");
        File.WriteAllText(Path.Combine(folder, "root.pdf"), "root");

        var result = _service.Scan(folder);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("root.pdf", candidate.FileName);
    }

    [Fact]
    public void Scan_ShouldReturnMetadata()
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, "meta.DCM");
        File.WriteAllText(filePath, "12345");
        var lastWrite = new DateTime(2026, 5, 8, 12, 30, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(filePath, lastWrite);

        var result = _service.Scan(folder);

        var candidate = Assert.Single(result.Candidates);
        Assert.Equal("meta.DCM", candidate.FileName);
        Assert.Equal(".dcm", candidate.Extension);
        Assert.Equal(filePath, candidate.FullPath);
        Assert.Equal(5, candidate.SizeBytes);
        Assert.Equal(lastWrite, candidate.LastWriteTimeUtc);
    }

    [Fact]
    public void Scan_ShouldNotChangeMoveOrDeleteFiles()
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, "keep.pdf");
        File.WriteAllText(filePath, "unchanged");
        var lastWrite = new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(filePath, lastWrite);

        var result = _service.Scan(folder);

        Assert.True(result.Success);
        Assert.True(File.Exists(filePath));
        Assert.Equal("unchanged", File.ReadAllText(filePath));
        Assert.Equal(lastWrite, File.GetLastWriteTimeUtc(filePath));
    }

    [Fact]
    public void Scan_ShouldUseAttachmentImportFolderFromOptions()
    {
        var folder = CreateTempFolder();
        File.WriteAllText(Path.Combine(folder, "report.txt"), "text");
        var options = CreateOptions(folder, attachmentFileStabilityWaitSeconds: 0);

        var result = _service.Scan(options);

        Assert.True(result.Success);
        Assert.Equal(folder, result.ScannedFolder);
        var candidate = Assert.Single(result.Candidates);
        Assert.True(candidate.IsStable);
        Assert.Equal("Stabil.", candidate.StableStatus);
        Assert.Equal(1, result.StableSupportedCount);
    }

    [Fact]
    public void Scan_WithOptionsShouldMarkLockedSupportedFileAsNotStable()
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, "locked.pdf");
        File.WriteAllText(filePath, "locked");
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var options = CreateOptions(folder, attachmentFileStabilityWaitSeconds: 0);

        var result = _service.Scan(options);

        var candidate = Assert.Single(result.Candidates);
        Assert.True(candidate.IsSupported);
        Assert.False(candidate.IsStable);
        Assert.Contains("nicht lesbar", candidate.StableStatus);
        Assert.Equal(0, result.StableSupportedCount);
    }

    private static string CreateTempFolder()
    {
        var folder = CreateTempFolderPath();
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static string CreateTempFolderPath()
    {
        return Path.Combine(
            Path.GetTempPath(),
            "XdtDeviceBridgeTests",
            Guid.NewGuid().ToString("N"));
    }

    private static InterfaceFolderOptions CreateOptions(
        string attachmentImportFolder,
        int attachmentFileStabilityWaitSeconds = 0)
    {
        return new InterfaceFolderOptions(
            AisImportFolder: string.Empty,
            DeviceImportFolder: string.Empty,
            ExportFolder: string.Empty,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ClearAisImportFolderBeforeProcessing: false,
            ClearDeviceImportFolderBeforeProcessing: false,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: false,
            MoveFailedFilesToErrorFolder: false,
            AttachmentImportFolder: attachmentImportFolder,
            AttachmentFileStabilityWaitSeconds: attachmentFileStabilityWaitSeconds);
    }
}
