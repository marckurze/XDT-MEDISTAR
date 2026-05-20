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
    AttachmentQuietPeriodWait,
    AttachmentManualConfirmationWait,
    AttachmentOptionalTimeoutContinueWithoutAttachment,
    AttachmentRequiredTimeoutBlock,
    MultipleAttachmentsAmbiguous,
    PreparationSucceeded,
    PreparationFailed
}
