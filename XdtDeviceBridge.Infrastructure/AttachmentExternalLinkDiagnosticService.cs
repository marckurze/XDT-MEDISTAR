using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class AttachmentExternalLinkDiagnosticService
{
    private readonly IAttachmentExternalLinkPreparationService _preparationService;

    public AttachmentExternalLinkDiagnosticService()
        : this(new AttachmentExternalLinkPreparationService())
    {
    }

    public AttachmentExternalLinkDiagnosticService(IAttachmentExternalLinkPreparationService preparationService)
    {
        _preparationService = preparationService ?? throw new ArgumentNullException(nameof(preparationService));
    }

    public AttachmentExternalLinkDiagnosticResult Prepare(
        InterfaceProfileDefinition? interfaceProfile,
        PatientData? patient,
        string? sourceAttachmentPath,
        DateTime processingTimestamp)
    {
        if (interfaceProfile is null)
        {
            return Fail("Kein Schnittstellenprofil für den XDT-Anhang-Test ausgewählt.");
        }

        if (patient is null || string.IsNullOrWhiteSpace(patient.PatientNumber))
        {
            return Fail("Für den XDT-Anhang-Test muss zuerst eine AIS-GDT/XDT-Datei mit Patientennummer geladen werden.");
        }

        if (string.IsNullOrWhiteSpace(sourceAttachmentPath))
        {
            return Fail("Keine XDT-Anhang-Datei ausgewählt.");
        }

        var request = new AttachmentExternalLinkPreparationRequest(
            FolderOptions: interfaceProfile.FolderOptions,
            SourceAttachmentPath: sourceAttachmentPath.Trim(),
            Patient: patient,
            ProcessingTimestamp: processingTimestamp);
        var preparationResult = _preparationService.Prepare(request);
        if (!preparationResult.Success)
        {
            return new AttachmentExternalLinkDiagnosticResult(
                Success: false,
                Message: $"XDT-Anhang konnte nicht vorbereitet werden: {preparationResult.ErrorMessage}",
                PreparationResult: preparationResult);
        }

        return new AttachmentExternalLinkDiagnosticResult(
            Success: true,
            Message: "XDT-Anhang erfolgreich vorbereitet.",
            PreparationResult: preparationResult);
    }

    private static AttachmentExternalLinkDiagnosticResult Fail(string message)
    {
        return new AttachmentExternalLinkDiagnosticResult(
            Success: false,
            Message: message,
            PreparationResult: null);
    }
}
