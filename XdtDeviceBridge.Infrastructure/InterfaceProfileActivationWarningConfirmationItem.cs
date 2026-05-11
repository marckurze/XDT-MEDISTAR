namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationWarningConfirmationItem(
    string Area,
    string Code,
    string Title,
    string? Detail,
    InterfaceProfileActivationSeverity Severity,
    bool IsRequiredForActivation);
