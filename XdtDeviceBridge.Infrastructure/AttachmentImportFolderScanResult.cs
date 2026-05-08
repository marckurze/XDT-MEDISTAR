namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentImportFolderScanResult(
    bool Success,
    string ScannedFolder,
    IReadOnlyList<AttachmentImportFileCandidate> Candidates,
    string? ErrorMessage)
{
    public int SupportedCount => Candidates.Count(candidate => candidate.IsSupported);

    public int UnsupportedCount => Candidates.Count(candidate => !candidate.IsSupported);

    public int StableSupportedCount => Candidates.Count(candidate => candidate.IsSupported && candidate.IsStable);
}
