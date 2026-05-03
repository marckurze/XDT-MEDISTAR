namespace XdtDeviceBridge.Infrastructure;

public sealed record FolderSafetyValidationIssue(
    FolderSafetyValidationIssueSeverity Severity,
    string Message,
    string Path);
