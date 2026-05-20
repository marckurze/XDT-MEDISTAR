namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentCompletionDecision(
    bool CanComplete,
    bool ShouldWait,
    bool RequiresManualConfirmation,
    AttachmentCompletionDecisionReason Reason,
    string Message,
    IReadOnlyList<AttachmentImportFileCandidate> SelectedCandidates,
    TimeSpan? RemainingQuietPeriod = null);
