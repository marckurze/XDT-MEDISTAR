namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportAnalysisResult(
    bool Success,
    string? PackageId,
    string? PackageName,
    IReadOnlyList<TemplatePackageImportProfileDecision> ProfileDecisions,
    IReadOnlyList<TemplatePackageImportProfileDecision> BlockingConflicts,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage,
    int TotalProfiles,
    int ImportableProfiles,
    int ConflictingProfiles,
    int BlockedProfiles);
