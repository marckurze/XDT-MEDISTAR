namespace XdtDeviceBridge.Infrastructure;

public sealed record TerminalBlockedImportFileHandlingResult(
    IReadOnlyList<string> HandledFiles,
    IReadOnlyList<string> Messages,
    bool HasErrors);
