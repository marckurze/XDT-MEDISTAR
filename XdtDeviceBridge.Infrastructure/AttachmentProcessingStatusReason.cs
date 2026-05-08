namespace XdtDeviceBridge.Infrastructure;

public enum AttachmentProcessingStatusReason
{
    None,
    EligibilityNotMet,
    ScanError,
    NoSupportedAttachment,
    NoStableAttachment,
    MultipleSupportedAttachments,
    MultipleStableAttachments,
    PreparationSucceeded,
    PreparationFailed
}
