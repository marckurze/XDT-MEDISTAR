namespace XdtDeviceBridge.Core;

public sealed record XdtExportResult(
    string Content,
    IReadOnlyList<XdtExportIssue> Issues)
{
    public bool HasErrors => Issues.Any(i => i.Severity == XdtExportIssueSeverity.Error);
}
