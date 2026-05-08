namespace XdtDeviceBridge.Infrastructure;

public sealed record AutoImportPairProcessingResult(
    string PairKey,
    string AisFilePath,
    string DeviceFilePath,
    bool WasProcessed,
    bool WasSkipped,
    bool Success,
    string Status,
    string? ExportFilePath,
    InterfaceProfileManualProcessingResult? ManualProcessingResult,
    IReadOnlyList<string> Messages,
    AttachmentProcessingStatus? AttachmentStatus = null);
