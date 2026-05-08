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
    AttachmentWait,
    AttachmentOptionalTimeoutContinueWithoutAttachment,
    AttachmentRequiredTimeoutBlock,
    MultipleAttachmentsAmbiguous,
    PreparationSucceeded,
    PreparationFailed
}
