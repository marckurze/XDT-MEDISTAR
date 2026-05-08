namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentAutoCandidateSelectionService
{
    public AttachmentAutoCandidateSelectionResult SelectCandidate(AttachmentImportFolderScanResult? scanResult)
    {
        if (scanResult is null)
        {
            return CreateResult(
                success: false,
                canProcessAutomatically: false,
                selectedCandidate: null,
                supportedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                unsupportedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                reason: AttachmentAutoCandidateSelectionReason.ScanError,
                errorMessage: "XDT-Anhang Scan-Ergebnis fehlt.");
        }

        var supportedCandidates = scanResult.Candidates
            .Where(candidate => candidate.IsSupported)
            .ToList();
        var unsupportedCandidates = scanResult.Candidates
            .Where(candidate => !candidate.IsSupported)
            .ToList();

        if (!scanResult.Success)
        {
            return CreateResult(
                success: false,
                canProcessAutomatically: false,
                selectedCandidate: null,
                supportedCandidates: supportedCandidates,
                unsupportedCandidates: unsupportedCandidates,
                reason: AttachmentAutoCandidateSelectionReason.ScanError,
                errorMessage: scanResult.ErrorMessage ?? "XDT-Anhang Importordner konnte nicht eingelesen werden.");
        }

        if (supportedCandidates.Count == 0)
        {
            return CreateResult(
                success: true,
                canProcessAutomatically: false,
                selectedCandidate: null,
                supportedCandidates: supportedCandidates,
                unsupportedCandidates: unsupportedCandidates,
                reason: AttachmentAutoCandidateSelectionReason.NoSupportedAttachment,
                errorMessage: "Keine unterstützte XDT-Anhang-Datei gefunden.");
        }

        if (supportedCandidates.Count == 1)
        {
            return CreateResult(
                success: true,
                canProcessAutomatically: true,
                selectedCandidate: supportedCandidates[0],
                supportedCandidates: supportedCandidates,
                unsupportedCandidates: unsupportedCandidates,
                reason: AttachmentAutoCandidateSelectionReason.SingleSupportedAttachment,
                errorMessage: null);
        }

        return CreateResult(
            success: true,
            canProcessAutomatically: false,
            selectedCandidate: null,
            supportedCandidates: supportedCandidates,
            unsupportedCandidates: unsupportedCandidates,
            reason: AttachmentAutoCandidateSelectionReason.MultipleSupportedAttachments,
            errorMessage: "Mehrere unterstützte XDT-Anhang-Dateien gefunden. Automatische Auswahl ist nicht eindeutig.");
    }

    private static AttachmentAutoCandidateSelectionResult CreateResult(
        bool success,
        bool canProcessAutomatically,
        AttachmentImportFileCandidate? selectedCandidate,
        IReadOnlyList<AttachmentImportFileCandidate> supportedCandidates,
        IReadOnlyList<AttachmentImportFileCandidate> unsupportedCandidates,
        AttachmentAutoCandidateSelectionReason reason,
        string? errorMessage)
    {
        return new AttachmentAutoCandidateSelectionResult(
            Success: success,
            CanProcessAutomatically: canProcessAutomatically,
            SelectedCandidate: selectedCandidate,
            SupportedCandidates: supportedCandidates,
            UnsupportedCandidates: unsupportedCandidates,
            Reason: reason,
            ErrorMessage: errorMessage);
    }
}
