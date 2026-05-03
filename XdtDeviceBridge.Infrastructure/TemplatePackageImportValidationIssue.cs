using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportValidationIssue(
    TemplatePackageImportValidationIssueSeverity Severity,
    string Message,
    string? ProfileId,
    ProfileKind? ProfileKind);
