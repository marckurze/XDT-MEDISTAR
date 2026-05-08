namespace XdtDeviceBridge.Core;

public sealed record ExternalAisLinkXdtFieldAdapterResult(
    bool Success,
    IReadOnlyList<ExportFieldRecord> Fields,
    string? ErrorMessage);
