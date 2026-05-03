namespace XdtDeviceBridge.Infrastructure;

public sealed record FolderSafetyValidationResult(
    IReadOnlyList<FolderSafetyValidationIssue> Issues)
{
    public bool HasErrors => Issues.Any(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error);

    public bool HasWarnings => Issues.Any(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Warning);
}
