namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileConfigurationIssue(
    InterfaceProfileConfigurationIssueSeverity Severity,
    string Message,
    string? Path = null);
