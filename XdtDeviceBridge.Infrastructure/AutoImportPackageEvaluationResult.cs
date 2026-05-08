namespace XdtDeviceBridge.Infrastructure;

public sealed record AutoImportPackageEvaluationResult(
    IReadOnlyList<PendingImportPair> ReadyPairs,
    IReadOnlyList<string> Messages,
    IReadOnlyList<PendingImportFile> ReplacedAisFiles,
    IReadOnlyList<PendingImportFile> ExpiredAisFiles,
    AutoImportPackageStateReason Reason);
