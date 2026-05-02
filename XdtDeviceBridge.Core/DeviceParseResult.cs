namespace XdtDeviceBridge.Core;

public sealed record DeviceParseResult(
    IReadOnlyList<MeasurementValue> Measurements,
    IReadOnlyList<DeviceParseIssue> Issues)
{
    public bool HasErrors => Issues.Any(i => i.Severity == DeviceParseIssueSeverity.Error);
}
