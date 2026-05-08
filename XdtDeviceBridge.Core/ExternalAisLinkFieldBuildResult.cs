namespace XdtDeviceBridge.Core;

public sealed record ExternalAisLinkFieldBuildResult(
    bool Success,
    ExternalAisLinkFieldSet? FieldSet,
    string? ErrorMessage);
