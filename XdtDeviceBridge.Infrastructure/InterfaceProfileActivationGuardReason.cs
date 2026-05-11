namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationGuardReason(
    InterfaceProfileActivationSeverity Severity,
    string Code,
    string Message,
    string? Detail = null);
