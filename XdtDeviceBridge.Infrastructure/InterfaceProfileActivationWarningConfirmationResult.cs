namespace XdtDeviceBridge.Infrastructure;

public sealed record InterfaceProfileActivationWarningConfirmationResult(
    bool CanRequestConfirmation,
    InterfaceProfileActivationWarningConfirmationStatus Status,
    string ProfileId,
    string ProfileName,
    IReadOnlyList<InterfaceProfileActivationWarningConfirmationItem> Warnings,
    IReadOnlyList<InterfaceProfileActivationWarningConfirmationReason> BlockingReasons,
    string Message);
