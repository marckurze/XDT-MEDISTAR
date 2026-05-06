namespace XdtDeviceBridge.Infrastructure;

public sealed record AutoImportPairProcessingBatchResult(
    int ProcessedCount,
    int SkippedAlreadyProcessedCount,
    int ErrorCount,
    IReadOnlyList<AutoImportPairProcessingResult> Results);
