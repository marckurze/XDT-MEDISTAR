using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record TemplatePackageImportPreviewDisplay(
    TemplatePackageImportPreviewSummary Summary,
    IReadOnlyList<TemplatePackageImportPreviewRow> Rows,
    IReadOnlyList<TemplatePackageImportDependencyPreviewRow> DependencyRows,
    IReadOnlyList<string> Messages,
    IReadOnlyList<string> Warnings);

public sealed record TemplatePackageImportPreviewSummary(
    string PackageId,
    string PackageName,
    int TotalProfiles,
    int ImportableProfiles,
    int PlannedImportAsCopy,
    int PlannedKeepOrSkip,
    int BlockedProfiles,
    int WarningCount,
    bool HasBlockingItems,
    string SummaryText);

public sealed class TemplatePackageImportPreviewRow
{
    public TemplatePackageImportPreviewRow(
        ProfileKind profileKindValue,
        string profileKind,
        string importedProfileName,
        string importedProfileId,
        TemplatePackageImportAction selectedAction,
        IReadOnlyList<TemplatePackageImportPreviewActionOption> availableActions,
        bool isActionSelectionEnabled,
        string plannedAction,
        string targetProfileName,
        string targetProfileId,
        string conflict,
        string status,
        string message)
    {
        ProfileKindValue = profileKindValue;
        ProfileKind = profileKind;
        ImportedProfileName = importedProfileName;
        ImportedProfileId = importedProfileId;
        SelectedAction = selectedAction;
        AvailableActions = availableActions;
        IsActionSelectionEnabled = isActionSelectionEnabled;
        PlannedAction = plannedAction;
        TargetProfileName = targetProfileName;
        TargetProfileId = targetProfileId;
        Conflict = conflict;
        Status = status;
        Message = message;
    }

    public ProfileKind ProfileKindValue { get; }

    public string ProfileKind { get; }

    public string ImportedProfileName { get; }

    public string ImportedProfileId { get; }

    public TemplatePackageImportAction SelectedAction { get; set; }

    public IReadOnlyList<TemplatePackageImportPreviewActionOption> AvailableActions { get; }

    public bool IsActionSelectionEnabled { get; }

    public string PlannedAction { get; }

    public string TargetProfileName { get; }

    public string TargetProfileId { get; }

    public string Conflict { get; }

    public string Status { get; }

    public string Message { get; }
}

public sealed record TemplatePackageImportPreviewActionOption(
    TemplatePackageImportAction Action,
    string DisplayName);

public sealed record TemplatePackageImportDependencyPreviewRow(
    string InterfaceProfileName,
    string DependencyKind,
    string OriginalProfileName,
    string OriginalProfileId,
    string TargetProfileName,
    string TargetProfileId,
    string Resolution,
    string Status,
    string Message);
