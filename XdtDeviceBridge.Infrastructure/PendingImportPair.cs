namespace XdtDeviceBridge.Infrastructure;

public sealed record PendingImportPair(
    PendingImportFile AisFile,
    PendingImportFile DeviceFile,
    bool IsReady);
