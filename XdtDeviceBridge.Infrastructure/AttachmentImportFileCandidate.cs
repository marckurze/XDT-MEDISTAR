namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentImportFileCandidate(
    string FileName,
    string Extension,
    string FullPath,
    long SizeBytes,
    DateTime LastWriteTimeUtc,
    bool IsSupported,
    string StableStatus,
    string? ErrorMessage,
    bool IsStable = false);
