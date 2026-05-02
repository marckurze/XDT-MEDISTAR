namespace XdtDeviceBridge.Core;

public enum GdtParseIssueSeverity
{
    Warning,
    Error
}

public sealed record GdtParseIssue(
    int LineNumber,
    GdtParseIssueSeverity Severity,
    string Message,
    string RawLine);
