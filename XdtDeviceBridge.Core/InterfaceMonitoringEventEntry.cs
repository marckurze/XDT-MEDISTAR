namespace XdtDeviceBridge.Core;

public sealed record InterfaceMonitoringEventEntry(
    DateTime Timestamp,
    string ScopeId,
    string EventKey,
    string Message,
    InterfaceMonitoringEventSeverity Severity);
