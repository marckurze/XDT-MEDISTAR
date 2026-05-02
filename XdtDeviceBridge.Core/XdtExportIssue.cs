namespace XdtDeviceBridge.Core;

public enum XdtExportIssueSeverity
{
    Warning,
    Error
}

public sealed record XdtExportIssue(
    XdtExportIssueSeverity Severity,
    string Message,
    string FieldCode,
    string? Value);
