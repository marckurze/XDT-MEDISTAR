namespace XdtDeviceBridge.Infrastructure;

public enum AttachmentCompletionDecisionReason
{
    NotAttachmentOnly,
    NoStableFiles,
    ContainsUnstableFiles,
    QuietPeriodStarted,
    QuietPeriodRestarted,
    QuietPeriodWaiting,
    QuietPeriodComplete,
    ManualConfirmationRequired
}
