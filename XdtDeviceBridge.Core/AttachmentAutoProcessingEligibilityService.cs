namespace XdtDeviceBridge.Core;

public sealed class AttachmentAutoProcessingEligibilityService
{
    public AttachmentAutoProcessingEligibilityResult Evaluate(
        InterfaceProfileDefinition? interfaceProfile,
        PatientData? patient,
        bool isMonitoringRunning,
        bool isGlobalAutomaticProcessingEnabled)
    {
        var reasons = new List<string>();

        if (interfaceProfile is null)
        {
            reasons.Add("Kein Schnittstellenprofil vorhanden.");
            return new AttachmentAutoProcessingEligibilityResult(false, reasons);
        }

        if (!interfaceProfile.IsActive)
        {
            reasons.Add("Schnittstellenprofil ist nicht aktiv.");
        }

        if (!interfaceProfile.FolderOptions.IsAttachmentProcessingEnabled)
        {
            reasons.Add("XDT-Anhang-Verarbeitung ist im Schnittstellenprofil nicht aktiviert.");
        }

        if (!isGlobalAutomaticProcessingEnabled)
        {
            reasons.Add("Globale automatische Verarbeitung ist nicht aktiviert.");
        }

        if (!isMonitoringRunning)
        {
            reasons.Add("Überwachung läuft nicht.");
        }

        if (patient is null || string.IsNullOrWhiteSpace(patient.PatientNumber))
        {
            reasons.Add("AIS-Patientennummer fehlt.");
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentImportFolder))
        {
            reasons.Add("XDT-Anhang Importordner fehlt.");
        }

        if (string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExportFolder))
        {
            reasons.Add("XDT-Anhang Exportordner fehlt.");
        }

        return new AttachmentAutoProcessingEligibilityResult(reasons.Count == 0, reasons);
    }
}
