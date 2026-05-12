using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageExportSelectionResult(
    bool Success,
    TemplatePackageExportRequest? Request,
    string SuggestedFileName,
    IReadOnlyList<string> Messages,
    string? ErrorMessage);
