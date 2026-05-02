namespace XdtDeviceBridge.Core;

public sealed record MappingResult(
    IReadOnlyList<ExportFieldRecord> Records,
    IReadOnlyList<MappingIssue> Issues)
{
    public bool HasErrors => Issues.Any(i => i.Severity == MappingIssueSeverity.Error);
}
