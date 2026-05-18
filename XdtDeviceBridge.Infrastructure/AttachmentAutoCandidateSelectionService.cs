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
                selectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                supportedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                unsupportedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                reason: AttachmentAutoCandidateSelectionReason.ScanError,
                errorMessage: "XDT-Anhang Scan-Ergebnis fehlt.");
        }

        var supportedCandidates = scanResult.Candidates
            .Where(candidate => candidate.IsSupported)
            .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var stableSupportedCandidates = supportedCandidates
            .Where(candidate => candidate.IsStable)
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
                selectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
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
                selectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                supportedCandidates: supportedCandidates,
                unsupportedCandidates: unsupportedCandidates,
                reason: AttachmentAutoCandidateSelectionReason.NoSupportedAttachment,
                errorMessage: "Keine unterstützte XDT-Anhang-Datei gefunden.");
        }

        if (stableSupportedCandidates.Count == supportedCandidates.Count)
        {
            return CreateResult(
                success: true,
                canProcessAutomatically: true,
                selectedCandidate: stableSupportedCandidates[0],
                selectedCandidates: stableSupportedCandidates,
                supportedCandidates: supportedCandidates,
                unsupportedCandidates: unsupportedCandidates,
                reason: stableSupportedCandidates.Count == 1
                    ? AttachmentAutoCandidateSelectionReason.SingleSupportedAttachment
                    : AttachmentAutoCandidateSelectionReason.MultipleStableAttachments,
                errorMessage: null);
        }

        if (stableSupportedCandidates.Count == 0)
        {
            return CreateResult(
                success: true,
                canProcessAutomatically: false,
                selectedCandidate: null,
                selectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                supportedCandidates: supportedCandidates,
                unsupportedCandidates: unsupportedCandidates,
                reason: AttachmentAutoCandidateSelectionReason.NoStableAttachment,
                errorMessage: supportedCandidates.Count == 1
                    ? "XDT-Anhang-Datei ist noch nicht stabil und wird später erneut geprüft."
                    : "XDT-Anhang-Dateien sind noch nicht stabil und werden später erneut geprüft.");
        }

        return CreateResult(
            success: true,
            canProcessAutomatically: false,
            selectedCandidate: null,
            selectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
            supportedCandidates: supportedCandidates,
            unsupportedCandidates: unsupportedCandidates,
            reason: AttachmentAutoCandidateSelectionReason.MultipleSupportedAttachments,
            errorMessage: "Mindestens eine unterstützte XDT-Anhang-Datei ist noch nicht stabil; automatische Verarbeitung wartet.");
    }

    private static AttachmentAutoCandidateSelectionResult CreateResult(
        bool success,
        bool canProcessAutomatically,
        AttachmentImportFileCandidate? selectedCandidate,
        IReadOnlyList<AttachmentImportFileCandidate> selectedCandidates,
        IReadOnlyList<AttachmentImportFileCandidate> supportedCandidates,
        IReadOnlyList<AttachmentImportFileCandidate> unsupportedCandidates,
        AttachmentAutoCandidateSelectionReason reason,
        string? errorMessage)
    {
        return new AttachmentAutoCandidateSelectionResult(
            Success: success,
            CanProcessAutomatically: canProcessAutomatically,
            SelectedCandidate: selectedCandidate,
            SelectedCandidates: selectedCandidates,
            SupportedCandidates: supportedCandidates,
            UnsupportedCandidates: unsupportedCandidates,
            Reason: reason,
            ErrorMessage: errorMessage);
    }
}
