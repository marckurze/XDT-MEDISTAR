using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportUserSelection(
    ProfileKind ProfileKind,
    string ImportedProfileId,
    TemplatePackageImportAction SelectedAction,
    string? TargetProfileId,
    string? TargetProfileName,
    bool IsValid,
    string? ValidationMessage);
