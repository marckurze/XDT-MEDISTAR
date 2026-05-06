namespace XdtDeviceBridge.Infrastructure;

public sealed record PendingImportFile(
    string FilePath,
    string FileName,
    ImportFileKind Kind,
    PendingImportFileStatus Status,
    DateTime DetectedAtUtc,
    DateTime? StableAtUtc,
    string? Message);
