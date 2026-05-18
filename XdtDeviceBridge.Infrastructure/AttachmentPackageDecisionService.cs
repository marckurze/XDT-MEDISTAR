using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentPackageDecisionService
{
    public AttachmentPackageDecisionResult Decide(
        InterfaceProfileDefinition? interfaceProfile,
        PatientData? patient,
        AttachmentImportFolderScanResult? scanResult,
        bool isMonitoringRunning,
        bool isGlobalAutomaticProcessingEnabled,
        bool hasWaitTimedOut)
    {
        if (interfaceProfile is null
            || !interfaceProfile.IsActive
            || !isMonitoringRunning
            || !isGlobalAutomaticProcessingEnabled
            || !interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled)
        {
            return ContinueWithoutAttachment(
                AttachmentPackageDecisionReason.AttachmentDisabled,
                "XDT-Anhang-Verarbeitung ist nicht aktiv.");
        }

        if (patient is null || string.IsNullOrWhiteSpace(patient.PatientNumber))
        {
            return Block(
                AttachmentPackageDecisionReason.MissingPatientNumber,
                "AIS-Patientennummer fehlt; XDT-Anhang kann nicht sicher zugeordnet werden.");
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentImportFolder)
            || string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExportFolder))
        {
            return Block(
                AttachmentPackageDecisionReason.MissingAttachmentFolders,
                "XDT-Anhang Import- oder Exportordner fehlt.");
        }

        if (scanResult is null || !scanResult.Success)
        {
            return Block(
                AttachmentPackageDecisionReason.ScanError,
                scanResult?.ErrorMessage ?? "XDT-Anhang Importordner konnte nicht geprüft werden.");
        }

        var supportedCandidates = scanResult.Candidates
            .Where(candidate => candidate.IsSupported)
            .OrderBy(candidate => candidate.FileName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var stableSupportedCandidates = supportedCandidates
            .Where(candidate => candidate.IsStable)
            .ToList();

        if (supportedCandidates.Count > 0 && stableSupportedCandidates.Count == supportedCandidates.Count)
        {
            return new AttachmentPackageDecisionResult(
                CanProcessAttachment: true,
                CanContinueWithoutAttachment: false,
                ShouldWait: false,
                ShouldBlock: false,
                SelectedCandidate: stableSupportedCandidates[0],
                SelectedCandidates: stableSupportedCandidates,
                Reason: stableSupportedCandidates.Count == 1
                    ? AttachmentPackageDecisionReason.SingleAttachmentReady
                    : AttachmentPackageDecisionReason.MultipleAttachmentsReady,
                Message: stableSupportedCandidates.Count == 1
                    ? "Genau ein stabiler unterstützter XDT-Anhang ist bereit."
                    : $"{stableSupportedCandidates.Count} stabile unterstützte XDT-Anhänge sind bereit.");
        }

        if (supportedCandidates.Count > 0 && stableSupportedCandidates.Count < supportedCandidates.Count)
        {
            if (!hasWaitTimedOut)
            {
                return new AttachmentPackageDecisionResult(
                    CanProcessAttachment: false,
                    CanContinueWithoutAttachment: false,
                    ShouldWait: true,
                    ShouldBlock: false,
                    SelectedCandidate: null,
                    SelectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                    Reason: AttachmentPackageDecisionReason.AttachmentNotStableWait,
                    Message: supportedCandidates.Count == 1
                        ? "XDT-Anhang ist noch nicht stabil; wird später erneut geprüft."
                        : "Mindestens ein XDT-Anhang ist noch nicht stabil; wird später erneut geprüft.");
            }

            if (interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required)
            {
                return Block(
                    AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock,
                    "Pflicht-XDT-Anhang wurde innerhalb der Wartezeit nicht stabil.");
            }

            return ContinueWithoutAttachment(
                AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment,
                "Optionaler XDT-Anhang wurde innerhalb der Wartezeit nicht stabil; Messwerte werden ohne Anhang fortgesetzt.");
        }

        if (!hasWaitTimedOut)
        {
            return new AttachmentPackageDecisionResult(
                CanProcessAttachment: false,
                CanContinueWithoutAttachment: false,
                ShouldWait: true,
                ShouldBlock: false,
                SelectedCandidate: null,
                SelectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
                Reason: interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required
                    ? AttachmentPackageDecisionReason.AttachmentRequiredWait
                    : AttachmentPackageDecisionReason.AttachmentOptionalWait,
                Message: "XDT-Anhang noch nicht gefunden; Wartezeit läuft.");
        }

        if (interfaceProfile.FolderOptions.AttachmentRequirementMode == AttachmentRequirementMode.Required)
        {
            return Block(
                AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock,
                "Pflicht-XDT-Anhang wurde innerhalb der Wartezeit nicht eindeutig gefunden.");
        }

        return ContinueWithoutAttachment(
            AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment,
            "Optionaler XDT-Anhang wurde innerhalb der Wartezeit nicht gefunden; Messwerte werden ohne Anhang fortgesetzt.");
    }

    private static AttachmentPackageDecisionResult ContinueWithoutAttachment(
        AttachmentPackageDecisionReason reason,
        string message)
    {
        return new AttachmentPackageDecisionResult(
            CanProcessAttachment: false,
            CanContinueWithoutAttachment: true,
            ShouldWait: false,
            ShouldBlock: false,
            SelectedCandidate: null,
            SelectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
            Reason: reason,
            Message: message);
    }

    private static AttachmentPackageDecisionResult Block(
        AttachmentPackageDecisionReason reason,
        string message)
    {
        return new AttachmentPackageDecisionResult(
            CanProcessAttachment: false,
            CanContinueWithoutAttachment: false,
            ShouldWait: false,
            ShouldBlock: true,
            SelectedCandidate: null,
            SelectedCandidates: Array.Empty<AttachmentImportFileCandidate>(),
            Reason: reason,
            Message: message);
    }
}
