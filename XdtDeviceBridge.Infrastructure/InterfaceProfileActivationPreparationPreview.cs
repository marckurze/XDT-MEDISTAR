namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationPreparationPreview(
    string Title,
    string ProfileName,
    string StatusText,
    string CanActivateText,
    int BlockerCount,
    int WarningCount,
    int InfoCount,
    IReadOnlyList<string> ImportantBlockers,
    IReadOnlyList<string> ImportantWarnings,
    string GuardDecisionText,
    string GuardCanProceedText,
    string GuardMessage,
    IReadOnlyList<string> GuardReasons,
    string SummaryMessage,
    string SafetyNotice,
    string MessageText);
