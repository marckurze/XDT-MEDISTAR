using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportDryRunItem(
    ProfileKind ProfileKind,
    string ImportedProfileId,
    string ImportedProfileName,
    TemplatePackageImportAction PlannedAction,
    string? TargetProfileId,
    string? TargetProfileName,
    string? ExistingProfileId,
    string? ExistingProfileName,
    bool IsBlocking,
    bool WouldWrite,
    bool WouldReplace,
    bool WouldKeepExisting,
    bool WouldSkip,
    bool RequiresDependencyRemap,
    IReadOnlyList<TemplatePackageImportDependencyRemap> DependencyRemaps,
    IReadOnlyList<string> DependencyRemapWarnings,
    string Message);
