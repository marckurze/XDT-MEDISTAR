namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPlanStep(
    string Code,
    string Title,
    string Description,
    bool WouldExecuteLater,
    bool IsBlocked,
    InterfaceProfileActivationSeverity Severity);
