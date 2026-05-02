namespace XdtDeviceBridge.Core;

public enum DeviceParseIssueSeverity
{
    Warning,
    Error
}

public sealed record DeviceParseIssue(
    DeviceParseIssueSeverity Severity,
    string Message,
    string SourcePath,
    string? RawValue);
