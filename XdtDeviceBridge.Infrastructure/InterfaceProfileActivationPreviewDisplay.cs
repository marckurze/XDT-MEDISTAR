namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPreviewDisplay(
    string StatusText,
    string CanActivateText,
    int BlockerCount,
    int WarningCount,
    int InfoCount,
    string SummaryText,
    string HintText,
    IReadOnlyList<InterfaceProfileActivationPreviewRow> Rows);

public sealed record InterfaceProfileActivationPreviewRow(
    string Severity,
    string Area,
    string Message,
    string Detail);
