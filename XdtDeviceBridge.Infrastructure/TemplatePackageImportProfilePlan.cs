using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportProfilePlan(
    ProfileKind ProfileKind,
    string ImportedProfileId,
    string ImportedProfileName,
    string? ExistingProfileId,
    string? ExistingProfileName,
    TemplatePackageImportExistingProfileSource ExistingProfileSource,
    TemplatePackageImportConflictType ConflictType,
    TemplatePackageImportAction PlannedAction,
    bool IsBlocking,
    bool RequiresUserDecision,
    bool RequiresRename,
    string? ProposedProfileId,
    string? ProposedProfileName,
    string Message);
