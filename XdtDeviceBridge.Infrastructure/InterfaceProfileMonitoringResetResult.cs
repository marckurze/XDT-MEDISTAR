namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileMonitoringResetResult(
    string InterfaceProfileId,
    int IgnoredFileCount,
    bool FileOperationsPerformed,
    InterfaceProfileInputFolderResetResult? InputFolderResetResult,
    IReadOnlyList<string> Messages);
