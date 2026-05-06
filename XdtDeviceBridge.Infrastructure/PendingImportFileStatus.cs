namespace XdtDeviceBridge.Infrastructure;

public enum PendingImportFileStatus
{
    Detected,
    Stable,
    WaitingForPair,
    ReadyForProcessing,
    Failed
}
