namespace XdtDeviceBridge.Core;

public sealed record ProcessingPipelineResult(
    PatientData? Patient,
    IReadOnlyList<MeasurementValue> Measurements,
    IReadOnlyList<ExportFieldRecord> ExportRecords,
    string ExportContent,
    IReadOnlyList<ProcessingIssue> Issues)
{
    public bool HasErrors => Issues.Any(i => i.Severity == ProcessingIssueSeverity.Error);
}
