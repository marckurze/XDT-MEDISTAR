namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentImportCandidateDisplayRow(
    string FileName,
    string Extension,
    string FullPath,
    long SizeBytes,
    DateTime LastWriteTimeUtc,
    bool IsSupported,
    string Status);
