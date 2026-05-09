using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportExecutionItem(
    ProfileKind ProfileKind,
    string SourceProfileId,
    string SourceProfileName,
    string? TargetProfileId,
    string? TargetProfileName,
    TemplatePackageImportAction Action,
    bool Success,
    bool WasWritten,
    bool WasSkipped,
    bool WasBlocked,
    string Message);
