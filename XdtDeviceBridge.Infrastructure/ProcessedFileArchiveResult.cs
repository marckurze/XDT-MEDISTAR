namespace XdtDeviceBridge.Infrastructure;

public sealed record ProcessedFileArchiveResult(
    IReadOnlyList<string> ArchivedFiles,
    IReadOnlyList<string> Issues,
    bool HasErrors);
