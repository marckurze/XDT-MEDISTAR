namespace XdtDeviceBridge.Infrastructure;

public sealed record FileExportResult(
    string? FilePath,
    IReadOnlyList<FileExportIssue> Issues)
{
    public bool HasErrors => Issues.Any(i => i.Severity == FileExportIssueSeverity.Error);
}
