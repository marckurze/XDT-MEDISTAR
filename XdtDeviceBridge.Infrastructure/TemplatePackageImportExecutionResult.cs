namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportExecutionResult(
    bool Success,
    IReadOnlyList<TemplatePackageImportExecutionItem> ImportedProfiles,
    IReadOnlyList<TemplatePackageImportExecutionItem> SkippedProfiles,
    IReadOnlyList<TemplatePackageImportExecutionItem> BlockedProfiles,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage,
    int ImportedAsNew,
    int ImportedAsCopy,
    int Skipped,
    int Blocked,
    int Failed);
