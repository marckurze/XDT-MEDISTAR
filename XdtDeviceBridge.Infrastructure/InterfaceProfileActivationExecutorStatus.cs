namespace XdtDeviceBridge.Infrastructure;

public enum InterfaceProfileActivationExecutorStatus
{
    NotAvailable,
    NotImplemented,
    Blocked,
    RequiresWarningConfirmation,
    ReadyButNotExecuted,
    WouldExecute,
    Failed,
    Success
}
