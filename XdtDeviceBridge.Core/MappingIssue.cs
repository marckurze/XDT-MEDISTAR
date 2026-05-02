namespace XdtDeviceBridge.Core;

public enum MappingIssueSeverity
{
    Warning,
    Error
}

public sealed record MappingIssue(
    MappingIssueSeverity Severity,
    string Message,
    string SourcePath,
    string TargetFieldCode);
