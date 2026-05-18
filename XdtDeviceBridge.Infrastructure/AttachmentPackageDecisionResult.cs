namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentPackageDecisionResult(
    bool CanProcessAttachment,
    bool CanContinueWithoutAttachment,
    bool ShouldWait,
    bool ShouldBlock,
    AttachmentImportFileCandidate? SelectedCandidate,
    IReadOnlyList<AttachmentImportFileCandidate> SelectedCandidates,
    AttachmentPackageDecisionReason Reason,
    string Message);
