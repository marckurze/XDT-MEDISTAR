namespace XdtDeviceBridge.Core;

public sealed record GdtParseResult(
    IReadOnlyList<FieldRecord> Records,
    IReadOnlyList<GdtParseIssue> Issues)
{
    public bool HasErrors => Issues.Any(i => i.Severity == GdtParseIssueSeverity.Error);
}
