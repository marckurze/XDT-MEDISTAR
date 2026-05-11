namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationGuardResult(
    bool CanProceed,
    InterfaceProfileActivationGuardDecision Decision,
    IReadOnlyList<InterfaceProfileActivationGuardReason> BlockerReasons,
    IReadOnlyList<InterfaceProfileActivationGuardReason> WarningReasons,
    IReadOnlyList<InterfaceProfileActivationGuardReason> InfoReasons,
    string Message);
