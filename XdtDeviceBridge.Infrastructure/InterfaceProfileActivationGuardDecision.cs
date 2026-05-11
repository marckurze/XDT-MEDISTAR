namespace XdtDeviceBridge.Infrastructure;

public enum InterfaceProfileActivationGuardDecision
{
    Allowed,
    AllowedWithWarnings,
    RequiresWarningConfirmation,
    Blocked,
    Unknown
}
