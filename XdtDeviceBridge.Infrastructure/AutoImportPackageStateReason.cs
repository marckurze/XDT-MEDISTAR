namespace XdtDeviceBridge.Infrastructure;

public enum AutoImportPackageStateReason
{
    None,
    NoImportFiles,
    WaitingForAisFile,
    WaitingForDeviceFile,
    AisFileReplaced,
    AisFileExpired,
    ReadyForProcessing
}
