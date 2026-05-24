namespace XdtDeviceBridge.Infrastructure;

public sealed record AutoImportPairProcessingBatchResult(
    int ProcessedCount,
    int SkippedCount,
    int ErrorCount,
    IReadOnlyList<AutoImportPairProcessingResult> Results);
