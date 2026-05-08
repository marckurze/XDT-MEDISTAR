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
    MultipleAttachmentsAmbiguous,
    MissingPatientNumber,
    MissingAttachmentFolders,
    ScanError
}
