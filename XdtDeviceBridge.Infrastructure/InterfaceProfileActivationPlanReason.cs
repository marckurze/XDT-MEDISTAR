namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPlanReason(
    InterfaceProfileActivationSeverity Severity,
    string Code,
    string Message,
    string? Detail = null);
