namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationWarningConfirmationReason(
    InterfaceProfileActivationSeverity Severity,
    string Code,
    string Message,
    string? Detail = null);
