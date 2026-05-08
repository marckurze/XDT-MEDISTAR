namespace XdtDeviceBridge.Infrastructure;

public enum AttachmentProcessingStatusReason
{
    None,
    EligibilityNotMet,
    ScanError,
    NoSupportedAttachment,
    MultipleSupportedAttachments,
    PreparationSucceeded,
    PreparationFailed
}
