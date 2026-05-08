namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportDryRunResult(
    bool Success,
    string? PackageId,
    string? PackageName,
    IReadOnlyList<TemplatePackageImportDryRunItem> Items,
    IReadOnlyList<TemplatePackageImportDryRunItem> BlockingItems,
    IReadOnlyList<string> Warnings,
    string? ErrorMessage,
    int TotalItems,
    int WouldImportAsNew,
    int WouldImportAsCopy,
    int WouldReplaceExisting,
    int WouldKeepExisting,
    int WouldSkip,
    int WouldBlock);
