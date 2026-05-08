namespace XdtDeviceBridge.Infrastructure;

public enum AttachmentAutoCandidateSelectionReason
{
    None,
    ScanError,
    NoSupportedAttachment,
    NoStableAttachment,
    SingleSupportedAttachment,
    MultipleSupportedAttachments,
    MultipleStableAttachments
}
