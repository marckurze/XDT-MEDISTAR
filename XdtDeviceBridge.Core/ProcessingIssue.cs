namespace XdtDeviceBridge.Core;

public enum ProcessingIssueSeverity
{
    Warning,
    Error
}

public sealed record ProcessingIssue(
    ProcessingIssueSeverity Severity,
    ProcessingStage Stage,
    string Message);
