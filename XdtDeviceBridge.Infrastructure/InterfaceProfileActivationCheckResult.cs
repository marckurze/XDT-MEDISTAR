namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationCheckResult(
    string Area,
    string Code,
    string Message,
    InterfaceProfileActivationSeverity Severity,
    string? Detail = null);
