namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportPreviewDisplay(
    TemplatePackageImportPreviewSummary Summary,
    IReadOnlyList<TemplatePackageImportPreviewRow> Rows,
    IReadOnlyList<TemplatePackageImportDependencyPreviewRow> DependencyRows,
    IReadOnlyList<string> Messages,
    IReadOnlyList<string> Warnings);

public sealed record TemplatePackageImportPreviewSummary(
    string PackageId,
    string PackageName,
    int TotalProfiles,
    int ImportableProfiles,
    int PlannedImportAsCopy,
    int PlannedKeepOrSkip,
    int BlockedProfiles,
    int WarningCount,
    bool HasBlockingItems,
    string SummaryText);

public sealed record TemplatePackageImportPreviewRow(
    string ProfileKind,
    string ImportedProfileName,
    string ImportedProfileId,
    string PlannedAction,
    string TargetProfileName,
    string TargetProfileId,
    string Conflict,
    string Status,
    string Message);

public sealed record TemplatePackageImportDependencyPreviewRow(
    string InterfaceProfileName,
    string DependencyKind,
    string OriginalProfileName,
    string OriginalProfileId,
    string TargetProfileName,
    string TargetProfileId,
    string Resolution,
    string Status,
    string Message);
