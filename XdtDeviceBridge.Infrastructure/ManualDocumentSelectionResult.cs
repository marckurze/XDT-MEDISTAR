namespace XdtDeviceBridge.Infrastructure;

public sealed record ManualDocumentSelectionResult(
    IReadOnlyList<AttachmentImportFileCandidate> AcceptedFiles,
    IReadOnlyList<string> RejectedMessages);
