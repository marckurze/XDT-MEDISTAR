namespace XdtDeviceBridge.Infrastructure;

public sealed record AttachmentAutoCandidateSelectionResult(
    bool Success,
    bool CanProcessAutomatically,
    AttachmentImportFileCandidate? SelectedCandidate,
    IReadOnlyList<AttachmentImportFileCandidate> SelectedCandidates,
    IReadOnlyList<AttachmentImportFileCandidate> SupportedCandidates,
    IReadOnlyList<AttachmentImportFileCandidate> UnsupportedCandidates,
    AttachmentAutoCandidateSelectionReason Reason,
    string? ErrorMessage);
