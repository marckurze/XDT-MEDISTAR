namespace XdtDeviceBridge.Infrastructure;

public sealed record AutoImportScanResult(
    string InterfaceProfileId,
    int AisFilesDetected,
    int DeviceFilesDetected,
    int FilesQueued,
    int ReadyPairs,
    IReadOnlyList<string> Messages,
    PendingImportQueue Queue);
