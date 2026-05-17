namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileMonitoringResetResult(
    string InterfaceProfileId,
    int IgnoredFileCount,
    bool FileOperationsPerformed,
    IReadOnlyList<string> Messages);
