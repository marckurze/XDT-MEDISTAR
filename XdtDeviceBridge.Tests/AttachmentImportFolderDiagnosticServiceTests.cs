using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentImportFolderDiagnosticServiceTests
{
    [Fact]
    public void Scan_ShouldReturnErrorWhenNoInterfaceProfileIsAvailable()
    {
        var service = new AttachmentImportFolderDiagnosticService(new RecordingScanner(CreateSuccessResult()));

        var result = service.Scan(null);

        Assert.False(result.Success);
        Assert.Contains("Kein Schnittstellenprofil", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void Scan_ShouldReturnErrorWhenImportFolderIsMissing()
    {
        var service = new AttachmentImportFolderDiagnosticService(new RecordingScanner(CreateSuccessResult()));

        var result = service.Scan(CreateProfile(attachmentImportFolder: ""));

        Assert.False(result.Success);
        Assert.Contains("Importordner ist nicht gesetzt", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void Scan_ShouldCallScannerWithConfiguredImportFolder()
    {
        var scanner = new RecordingScanner(CreateSuccessResult());
        var service = new AttachmentImportFolderDiagnosticService(scanner);
        var profile = CreateProfile(attachmentImportFolder: @"C:\Import\Attachments");

        var result = service.Scan(profile);

        Assert.True(result.Success);
        Assert.NotNull(scanner.LastOptions);
        Assert.Equal(@"C:\Import\Attachments", scanner.LastOptions!.AttachmentImportFolder);
    }

    [Fact]
    public void Scan_ShouldTranslateScannerError()
    {
        var scanner = new RecordingScanner(new AttachmentImportFolderScanResult(
            Success: false,
            ScannedFolder: @"C:\Missing",
            Candidates: Array.Empty<AttachmentImportFileCandidate>(),
            ErrorMessage: "XDT-Anhang Importordner existiert nicht."));
        var service = new AttachmentImportFolderDiagnosticService(scanner);

        var result = service.Scan(CreateProfile(attachmentImportFolder: @"C:\Missing"));

        Assert.False(result.Success);
        Assert.Contains("existiert nicht", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(result.Candidates);
    }

    [Fact]
    public void Scan_ShouldTranslateSupportedAndUnsupportedCandidates()
    {
        var candidates = new[]
        {
            CreateCandidate("report.pdf", ".pdf", isSupported: true),
            CreateCandidate("tool.exe", ".exe", isSupported: false)
        };
        var service = new AttachmentImportFolderDiagnosticService(new RecordingScanner(CreateSuccessResult(candidates)));

        var result = service.Scan(CreateProfile());

        Assert.True(result.Success);
        Assert.Equal(2, result.Candidates.Count);
        Assert.Contains("2 Dateien gefunden, davon 1 unterstützt", result.Message);
        Assert.Contains("Nicht unterstützte Dateien wurden nicht verarbeitet", result.Message);
        Assert.Contains(result.Candidates, row =>
            row.FileName == "report.pdf"
            && row.Extension == ".pdf"
            && row.IsSupported
            && row.IsStable
            && row.Status == "Unterstützt, stabil");
        Assert.Contains(result.Candidates, row =>
            row.FileName == "tool.exe"
            && row.Extension == ".exe"
            && !row.IsSupported
            && row.Status.Contains("Nicht unterstützt", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Scan_ShouldReturnNoSupportedMessageWhenOnlyUnsupportedCandidatesExist()
    {
        var service = new AttachmentImportFolderDiagnosticService(new RecordingScanner(CreateSuccessResult(
            new[] { CreateCandidate("tool.exe", ".exe", isSupported: false) })));

        var result = service.Scan(CreateProfile());

        Assert.True(result.Success);
        Assert.Contains("Keine unterstützten XDT-Anhänge gefunden", result.Message);
    }

    [Fact]
    public void Scan_ShouldReturnEmptyFolderMessageWhenNoCandidatesExist()
    {
        var service = new AttachmentImportFolderDiagnosticService(new RecordingScanner(CreateSuccessResult()));

        var result = service.Scan(CreateProfile());

        Assert.True(result.Success);
        Assert.Equal("Keine Dateien im XDT-Anhang Importordner gefunden.", result.Message);
        Assert.Empty(result.Candidates);
    }

    private static InterfaceProfileDefinition CreateProfile(
        string attachmentImportFolder = @"C:\Import\Attachments",
        string attachmentExportFolder = @"C:\Export\Attachments")
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        return profile with
        {
            IsActive = true,
            FolderOptions = profile.FolderOptions with
            {
                AttachmentImportFolder = attachmentImportFolder,
                AttachmentExportFolder = attachmentExportFolder
            }
        };
    }

    private static AttachmentImportFolderScanResult CreateSuccessResult(
        IReadOnlyList<AttachmentImportFileCandidate>? candidates = null)
    {
        return new AttachmentImportFolderScanResult(
            Success: true,
            ScannedFolder: @"C:\Import\Attachments",
            Candidates: candidates ?? Array.Empty<AttachmentImportFileCandidate>(),
            ErrorMessage: null);
    }

    private static AttachmentImportFileCandidate CreateCandidate(
        string fileName,
        string extension,
        bool isSupported)
    {
        return new AttachmentImportFileCandidate(
            FileName: fileName,
            Extension: extension,
            FullPath: Path.Combine(@"C:\Import\Attachments", fileName),
            SizeBytes: 123,
            LastWriteTimeUtc: new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc),
            IsSupported: isSupported,
            StableStatus: isSupported ? "Stabil." : "Nicht geprüft.",
            ErrorMessage: isSupported ? null : "Dateityp wird für XDT-Anhänge nicht unterstützt.",
            IsStable: isSupported);
    }

    private sealed class RecordingScanner : IAttachmentImportFolderScannerService
    {
        private readonly AttachmentImportFolderScanResult _result;

        public RecordingScanner(AttachmentImportFolderScanResult result)
        {
            _result = result;
        }

        public InterfaceFolderOptions? LastOptions { get; private set; }

        public string? LastFolder { get; private set; }

        public AttachmentImportFolderScanResult Scan(InterfaceFolderOptions folderOptions)
        {
            LastOptions = folderOptions;
            return _result;
        }

        public AttachmentImportFolderScanResult Scan(string attachmentImportFolder)
        {
            LastFolder = attachmentImportFolder;
            return _result;
        }
    }
}
