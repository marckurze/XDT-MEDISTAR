namespace XdtDeviceBridge.Infrastructure;

public enum FileExportIssueSeverity
{
    Warning,
    Error
}

public sealed record FileExportIssue(
    FileExportIssueSeverity Severity,
    string Message,
    string? FilePath);
