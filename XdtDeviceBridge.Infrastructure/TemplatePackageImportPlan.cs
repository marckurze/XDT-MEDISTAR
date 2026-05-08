namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportPlan(
    string? PackageId,
    string? PackageName,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<TemplatePackageImportProfilePlan> ProfilePlans,
    bool HasBlockingItems,
    IReadOnlyList<TemplatePackageImportProfilePlan> BlockingItems,
    IReadOnlyList<string> Warnings,
    int TotalProfiles,
    int PlannedImportAsNew,
    int PlannedImportAsCopy,
    int PlannedReplaceExisting,
    int PlannedKeepExisting,
    int PlannedSkip,
    int PlannedBlocked);
