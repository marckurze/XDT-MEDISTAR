namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportDependencyRemap(
    TemplatePackageImportDependencyKind DependencyKind,
    string OriginalProfileId,
    string? OriginalProfileName,
    string? TargetProfileId,
    string? TargetProfileName,
    TemplatePackageImportDependencyResolution Resolution,
    string Message);
