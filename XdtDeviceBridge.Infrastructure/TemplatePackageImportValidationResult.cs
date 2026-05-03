namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportValidationResult(
    IReadOnlyList<TemplatePackageImportValidationIssue> Issues)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Error);

    public bool HasWarnings => Issues.Any(issue => issue.Severity == TemplatePackageImportValidationIssueSeverity.Warning);
}
