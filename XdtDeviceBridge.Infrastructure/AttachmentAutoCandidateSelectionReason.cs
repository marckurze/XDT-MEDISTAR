namespace XdtDeviceBridge.Infrastructure;

public enum AttachmentAutoCandidateSelectionReason
{
    None,
    ScanError,
    NoSupportedAttachment,
    SingleSupportedAttachment,
    MultipleSupportedAttachments
}
