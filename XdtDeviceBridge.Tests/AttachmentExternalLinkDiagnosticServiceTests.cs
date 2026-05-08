using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentExternalLinkDiagnosticServiceTests
{
    private static readonly DateTime Timestamp = new(2026, 5, 7, 22, 17, 23, DateTimeKind.Utc);

    [Fact]
    public void Prepare_ShouldReturnErrorWhenNoPatientNumberIsAvailable()
    {
        var service = new AttachmentExternalLinkDiagnosticService(new RecordingPreparationService(CreateSuccessfulPreparation()));

        var result = service.Prepare(CreateInterfaceProfile(), CreatePatient(patientNumber: ""), @"C:\Import\Report.pdf", Timestamp);

        Assert.False(result.Success);
        Assert.Contains("Patientennummer", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.PreparationResult);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorWhenNoAttachmentFileIsSelected()
    {
        var service = new AttachmentExternalLinkDiagnosticService(new RecordingPreparationService(CreateSuccessfulPreparation()));

        var result = service.Prepare(CreateInterfaceProfile(), CreatePatient(), "", Timestamp);

        Assert.False(result.Success);
        Assert.Contains("Keine XDT-Anhang-Datei", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.PreparationResult);
    }

    [Fact]
    public void Prepare_ShouldReturnErrorWhenNoInterfaceProfileIsSelected()
    {
        var service = new AttachmentExternalLinkDiagnosticService(new RecordingPreparationService(CreateSuccessfulPreparation()));

        var result = service.Prepare(null, CreatePatient(), @"C:\Import\Report.pdf", Timestamp);

        Assert.False(result.Success);
        Assert.Contains("Kein Schnittstellenprofil", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(result.PreparationResult);
    }

    [Fact]
    public void Prepare_ShouldCallPreparationServiceWithExplicitSourceAttachmentPath()
    {
        var preparationService = new RecordingPreparationService(CreateSuccessfulPreparation());
        var service = new AttachmentExternalLinkDiagnosticService(preparationService);
        var sourcePath = @"C:\Import\Report.pdf";

        var result = service.Prepare(CreateInterfaceProfile(), CreatePatient(), sourcePath, Timestamp);

        Assert.True(result.Success);
        Assert.Equal(sourcePath, preparationService.LastRequest!.SourceAttachmentPath);
    }

    [Fact]
    public void Prepare_ShouldReturnPreparedFields()
    {
        var service = new AttachmentExternalLinkDiagnosticService(new RecordingPreparationService(CreateSuccessfulPreparation()));

        var result = service.Prepare(CreateInterfaceProfile(), CreatePatient(), @"C:\Import\Report.pdf", Timestamp);

        Assert.True(result.Success);
        Assert.NotNull(result.PreparationResult);
        Assert.Contains(result.PreparationResult!.ExportFields, field => field.FieldCode == "6302");
        Assert.Contains(result.PreparationResult.ExportFields, field => field.FieldCode == "6303");
        Assert.Contains(result.PreparationResult.ExportFields, field => field.FieldCode == "6304");
        Assert.Contains(result.PreparationResult.ExportFields, field => field.FieldCode == "6305");
    }

    [Fact]
    public void Prepare_ShouldForwardPreparationServiceError()
    {
        var preparationResult = new AttachmentExternalLinkPreparationResult(
            Success: false,
            SourcePath: @"C:\Import\Report.pdf",
            TargetPath: null,
            TargetFileName: null,
            TransferMode: AttachmentTransferMode.Move,
            ExternalAisLinkFieldSet: null,
            ExportFields: Array.Empty<ExportFieldRecord>(),
            ErrorMessage: "Transfer fehlgeschlagen.");
        var service = new AttachmentExternalLinkDiagnosticService(new RecordingPreparationService(preparationResult));

        var result = service.Prepare(CreateInterfaceProfile(), CreatePatient(), @"C:\Import\Report.pdf", Timestamp);

        Assert.False(result.Success);
        Assert.Contains("Transfer fehlgeschlagen", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Same(preparationResult, result.PreparationResult);
    }

    private static AttachmentExternalLinkPreparationResult CreateSuccessfulPreparation()
    {
        var fields = new[]
        {
            new ExportFieldRecord("6302", "PDF-Befund", 1),
            new ExportFieldRecord("6303", "PDF", 2),
            new ExportFieldRecord("6304", "Messprotokoll", 3),
            new ExportFieldRecord("6305", @"C:\Export\Report.pdf", 4)
        };

        return new AttachmentExternalLinkPreparationResult(
            Success: true,
            SourcePath: @"C:\Import\Report.pdf",
            TargetPath: @"C:\Export\Report.pdf",
            TargetFileName: "Report.pdf",
            TransferMode: AttachmentTransferMode.Move,
            ExternalAisLinkFieldSet: new ExternalAisLinkFieldSet(
                DocumentName: "PDF-Befund",
                FileFormat: "PDF",
                Description: "Messprotokoll",
                FullPath: @"C:\Export\Report.pdf"),
            ExportFields: fields,
            ErrorMessage: null);
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile()
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = true,
            FolderOptions = new InterfaceFolderOptions(
                AisImportFolder: string.Empty,
                DeviceImportFolder: string.Empty,
                ExportFolder: string.Empty,
                ArchiveFolder: string.Empty,
                ErrorFolder: string.Empty,
                ClearAisImportFolderBeforeProcessing: false,
                ClearDeviceImportFolderBeforeProcessing: false,
                ClearExportFolderAfterSuccessfulTransfer: false,
                ArchiveProcessedFiles: false,
                MoveFailedFilesToErrorFolder: false,
                AttachmentExportFolder: @"C:\Export")
        };
    }

    private static PatientData CreatePatient(string? patientNumber = "11253")
    {
        return new PatientData(
            PatientNumber: patientNumber,
            LastName: "Muster",
            FirstName: "Mara",
            BirthDate: "1980-05-31",
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
    }

    private sealed class RecordingPreparationService : IAttachmentExternalLinkPreparationService
    {
        private readonly AttachmentExternalLinkPreparationResult _result;

        public RecordingPreparationService(AttachmentExternalLinkPreparationResult result)
        {
            _result = result;
        }

        public AttachmentExternalLinkPreparationRequest? LastRequest { get; private set; }

        public AttachmentExternalLinkPreparationResult Prepare(AttachmentExternalLinkPreparationRequest? request)
        {
            LastRequest = request;
            return _result;
        }
    }
}
