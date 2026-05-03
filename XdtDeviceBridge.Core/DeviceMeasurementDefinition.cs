namespace XdtDeviceBridge.Core;

public sealed record DeviceMeasurementDefinition(
    string Id,
    string DisplayName,
    string SourcePath,
    string Group,
    string Eye,
    string Unit,
    bool IsRequired,
    string? Description);
