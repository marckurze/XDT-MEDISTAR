using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentAutoCandidateSelectionServiceTests
{
    private readonly AttachmentAutoCandidateSelectionService _service = new();

    [Fact]
    public void SelectCandidate_ShouldBlockWhenScanResultFailed()
    {
        var scanResult = CreateScanResult(
            success: false,
            candidates: Array.Empty<AttachmentImportFileCandidate>(),
            errorMessage: "Importordner existiert nicht.");

        var result = _service.SelectCandidate(scanResult);

        Assert.False(result.Success);
        Assert.False(result.CanProcessAutomatically);
        Assert.Null(result.SelectedCandidate);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.ScanError, result.Reason);
        Assert.Contains("existiert nicht", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SelectCandidate_ShouldBlockWhenNoSupportedCandidatesExist()
    {
        var scanResult = CreateScanResult(
            success: true,
            candidates: new[] { CreateCandidate("tool.exe", isSupported: false) });

        var result = _service.SelectCandidate(scanResult);

        Assert.True(result.Success);
        Assert.False(result.CanProcessAutomatically);
        Assert.Null(result.SelectedCandidate);
        Assert.Empty(result.SupportedCandidates);
        Assert.Single(result.UnsupportedCandidates);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.NoSupportedAttachment, result.Reason);
    }

    [Fact]
    public void SelectCandidate_ShouldSelectExactlyOneSupportedCandidate()
    {
        var candidate = CreateCandidate("report.pdf", isSupported: true);
        var scanResult = CreateScanResult(success: true, candidates: new[] { candidate });

        var result = _service.SelectCandidate(scanResult);

        Assert.True(result.Success);
        Assert.True(result.CanProcessAutomatically);
        Assert.Same(candidate, result.SelectedCandidate);
        Assert.Single(result.SelectedCandidates);
        Assert.Single(result.SupportedCandidates);
        Assert.Empty(result.UnsupportedCandidates);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.SingleSupportedAttachment, result.Reason);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void SelectCandidate_ShouldBlockWhenOnlySupportedCandidateIsNotStable()
    {
        var candidate = CreateCandidate("report.pdf", isSupported: true, isStable: false);
        var scanResult = CreateScanResult(success: true, candidates: new[] { candidate });

        var result = _service.SelectCandidate(scanResult);

        Assert.True(result.Success);
        Assert.False(result.CanProcessAutomatically);
        Assert.Null(result.SelectedCandidate);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.NoStableAttachment, result.Reason);
        Assert.Contains("noch nicht stabil", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SelectCandidate_ShouldSelectMultipleStableSupportedCandidates()
    {
        var image = CreateCandidate("image.jpg", isSupported: true);
        var report = CreateCandidate("report.pdf", isSupported: true);
        var scanResult = CreateScanResult(
            success: true,
            candidates: new[]
            {
                report,
                image
            });

        var result = _service.SelectCandidate(scanResult);

        Assert.True(result.Success);
        Assert.True(result.CanProcessAutomatically);
        Assert.Same(image, result.SelectedCandidate);
        Assert.Equal(new[] { "image.jpg", "report.pdf" }, result.SelectedCandidates.Select(candidate => candidate.FileName));
        Assert.Equal(2, result.SupportedCandidates.Count);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.MultipleStableAttachments, result.Reason);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void SelectCandidate_ShouldNeverSelectUnsupportedCandidate()
    {
        var unsupported = CreateCandidate("tool.exe", isSupported: false);
        var scanResult = CreateScanResult(success: true, candidates: new[] { unsupported });

        var result = _service.SelectCandidate(scanResult);

        Assert.False(result.CanProcessAutomatically);
        Assert.NotSame(unsupported, result.SelectedCandidate);
        Assert.Null(result.SelectedCandidate);
    }

    [Fact]
    public void SelectCandidate_ShouldSelectOneSupportedCandidateEvenWhenUnsupportedFilesExist()
    {
        var supported = CreateCandidate("report.pdf", isSupported: true);
        var scanResult = CreateScanResult(
            success: true,
            candidates: new[]
            {
                CreateCandidate("tool.exe", isSupported: false),
                supported,
                CreateCandidate("notes.tmp", isSupported: false)
            });

        var result = _service.SelectCandidate(scanResult);

        Assert.True(result.CanProcessAutomatically);
        Assert.Same(supported, result.SelectedCandidate);
        Assert.Single(result.SelectedCandidates);
        Assert.Single(result.SupportedCandidates);
        Assert.Equal(2, result.UnsupportedCandidates.Count);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.SingleSupportedAttachment, result.Reason);
    }

    [Fact]
    public void SelectCandidate_ShouldSelectMultipleStableSupportedCandidatesEvenWithUnsupportedFiles()
    {
        var scanResult = CreateScanResult(
            success: true,
            candidates: new[]
            {
                CreateCandidate("report.pdf", isSupported: true),
                CreateCandidate("tool.exe", isSupported: false),
                CreateCandidate("image.png", isSupported: true)
            });

        var result = _service.SelectCandidate(scanResult);

        Assert.True(result.CanProcessAutomatically);
        Assert.NotNull(result.SelectedCandidate);
        Assert.Equal(new[] { "image.png", "report.pdf" }, result.SelectedCandidates.Select(candidate => candidate.FileName));
        Assert.Equal(2, result.SupportedCandidates.Count);
        Assert.Single(result.UnsupportedCandidates);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.MultipleStableAttachments, result.Reason);
    }

    [Fact]
    public void SelectCandidate_ShouldWaitWhenMultipleSupportedCandidatesAreNotAllStable()
    {
        var scanResult = CreateScanResult(
            success: true,
            candidates: new[]
            {
                CreateCandidate("a.pdf", isSupported: true),
                CreateCandidate("b.pdf", isSupported: true, isStable: false)
            });

        var result = _service.SelectCandidate(scanResult);

        Assert.Null(result.SelectedCandidate);
        Assert.Empty(result.SelectedCandidates);
        Assert.False(result.CanProcessAutomatically);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.MultipleSupportedAttachments, result.Reason);
    }

    [Fact]
    public void SelectCandidate_ShouldReturnControlledScanErrorForNullScanResult()
    {
        var result = _service.SelectCandidate(null);

        Assert.False(result.Success);
        Assert.False(result.CanProcessAutomatically);
        Assert.Null(result.SelectedCandidate);
        Assert.Equal(AttachmentAutoCandidateSelectionReason.ScanError, result.Reason);
        Assert.Contains("fehlt", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SelectCandidate_ShouldNotTouchFiles()
    {
        var folder = CreateTempFolder();
        var filePath = Path.Combine(folder, "report.pdf");
        File.WriteAllText(filePath, "unchanged");
        var lastWrite = new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(filePath, lastWrite);
        var candidate = CreateCandidate("report.pdf", isSupported: true) with
        {
            FullPath = filePath,
            SizeBytes = new FileInfo(filePath).Length,
            LastWriteTimeUtc = lastWrite
        };

        var result = _service.SelectCandidate(CreateScanResult(success: true, candidates: new[] { candidate }));

        Assert.True(result.CanProcessAutomatically);
        Assert.Single(result.SelectedCandidates);
        Assert.True(File.Exists(filePath));
        Assert.Equal("unchanged", File.ReadAllText(filePath));
        Assert.Equal(lastWrite, File.GetLastWriteTimeUtc(filePath));
    }

    private static AttachmentImportFolderScanResult CreateScanResult(
        bool success,
        IReadOnlyList<AttachmentImportFileCandidate> candidates,
        string? errorMessage = null)
    {
        return new AttachmentImportFolderScanResult(
            Success: success,
            ScannedFolder: @"C:\Import\Attachments",
            Candidates: candidates,
            ErrorMessage: errorMessage);
    }

    private static AttachmentImportFileCandidate CreateCandidate(string fileName, bool isSupported)
    {
        return CreateCandidate(fileName, isSupported, isStable: isSupported);
    }

    private static AttachmentImportFileCandidate CreateCandidate(string fileName, bool isSupported, bool isStable)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return new AttachmentImportFileCandidate(
            FileName: fileName,
            Extension: extension,
            FullPath: Path.Combine(@"C:\Import\Attachments", fileName),
            SizeBytes: 123,
            LastWriteTimeUtc: new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc),
            IsSupported: isSupported,
            StableStatus: isStable ? "Stabil." : "Noch nicht stabil.",
            ErrorMessage: isSupported ? null : "Dateityp wird für XDT-Anhänge nicht unterstützt.",
            IsStable: isStable);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(
            Path.GetTempPath(),
            "XdtDeviceBridgeTests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }
}
