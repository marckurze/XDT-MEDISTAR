namespace XdtDeviceBridge.Infrastructure;

public sealed record DuplicateImportFileHandlingResult(
    string Status,
    IReadOnlyList<string> Messages,
    ProcessedFileArchiveResult? ArchiveResult);
