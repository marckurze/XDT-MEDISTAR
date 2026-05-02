namespace XdtDeviceBridge.Core;

public sealed record MeasurementValue(
    string SourcePath,
    string DisplayName,
    string Value,
    string? Unit,
    string? Eye,
    string? Group);
