namespace XdtDeviceBridge.Infrastructure;

public sealed record FailedFileCopyResult(
    IReadOnlyList<string> CopiedFiles,
    IReadOnlyList<string> Issues,
    bool HasErrors);
