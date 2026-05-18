namespace XdtDeviceBridge.Infrastructure;

public enum AttachmentPackageDecisionReason
{
    None,
    AttachmentDisabled,
    AttachmentOptionalWait,
    AttachmentRequiredWait,
    AttachmentOptionalTimeoutContinueWithoutAttachment,
    AttachmentRequiredTimeoutBlock,
    AttachmentNotStableWait,
    SingleAttachmentReady,
    MultipleAttachmentsReady,
    MultipleAttachmentsAmbiguous,
    MissingPatientNumber,
    MissingAttachmentFolders,
    ScanError
}
