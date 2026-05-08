using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportProfileDecision(
    ProfileKind ProfileKind,
    string ImportedProfileId,
    string ImportedProfileName,
    string? ImportedProfileVersion,
    string? ExistingProfileId,
    string? ExistingProfileName,
    TemplatePackageImportExistingProfileSource ExistingProfileSource,
    string? ExistingProfileVersion,
    TemplatePackageImportConflictType ConflictType,
    TemplatePackageImportAction SuggestedAction,
    bool IsBlocking,
    string Message);
