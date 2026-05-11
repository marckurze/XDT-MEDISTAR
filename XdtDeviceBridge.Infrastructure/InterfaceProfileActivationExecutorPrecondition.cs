namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationExecutorPrecondition(
    string Code,
    string Title,
    string Description,
    bool IsRequired,
    bool IsSatisfied,
    InterfaceProfileActivationSeverity Severity);
