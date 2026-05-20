namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPreviewDisplay(
    string StatusText,
    string CanActivateText,
    int BlockerCount,
    int WarningCount,
    int InfoCount,
    string SummaryText,
    string HintText,
    IReadOnlyList<InterfaceProfileActivationFolderDisplay> FolderChecks,
    IReadOnlyList<InterfaceProfileActivationAttachmentDisplay> AttachmentChecks,
    IReadOnlyList<InterfaceProfileActivationPreviewRow> Rows);

public sealed record InterfaceProfileActivationPreviewRow(
    string Severity,
    string Area,
    string Message,
    string Detail);

public sealed record InterfaceProfileActivationFolderDisplay(
    string Label,
    string Path,
    string Status,
    string Reachability,
    string Severity,
    string Message);

public sealed record InterfaceProfileActivationAttachmentDisplay(
    string Label,
    string Value,
    string Status,
    string Severity,
    string Message);
