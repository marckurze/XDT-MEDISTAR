using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentAutoProcessingEligibilityServiceTests
{
    private readonly AttachmentAutoProcessingEligibilityService _service = new();

    [Fact]
    public void Evaluate_ShouldAllowWhenAllConditionsAreFulfilled()
    {
        var result = _service.Evaluate(
            CreateProfile(),
            CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.True(result.IsAllowed);
        Assert.Empty(result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockInactiveInterfaceProfile()
    {
        var result = _service.Evaluate(
            CreateProfile(isActive: false),
            CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("Schnittstellenprofil ist nicht aktiv.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockWhenAttachmentProcessingIsDisabled()
    {
        var result = _service.Evaluate(
            CreateProfile(isAttachmentProcessingEnabled: false),
            CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("XDT-Anhang-Verarbeitung ist im Schnittstellenprofil nicht aktiviert.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockWhenGlobalAutomaticProcessingIsDisabled()
    {
        var result = _service.Evaluate(
            CreateProfile(),
            CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: false);

        Assert.False(result.IsAllowed);
        Assert.Contains("Globale automatische Verarbeitung ist nicht aktiviert.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockWhenMonitoringIsNotRunning()
    {
        var result = _service.Evaluate(
            CreateProfile(),
            CreatePatient(),
            isMonitoringRunning: false,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("Überwachung läuft nicht.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockWhenPatientNumberIsMissing()
    {
        var result = _service.Evaluate(
            CreateProfile(),
            CreatePatient(patientNumber: ""),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("AIS-Patientennummer fehlt.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockWhenAttachmentImportFolderIsMissing()
    {
        var result = _service.Evaluate(
            CreateProfile(attachmentImportFolder: ""),
            CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("XDT-Anhang Importordner fehlt.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldBlockWhenAttachmentExportFolderIsMissing()
    {
        var result = _service.Evaluate(
            CreateProfile(attachmentExportFolder: ""),
            CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("XDT-Anhang Exportordner fehlt.", result.Reasons);
    }

    [Fact]
    public void Evaluate_ShouldReturnControlledReasonWhenProfileIsMissing()
    {
        var result = _service.Evaluate(
            interfaceProfile: null,
            patient: CreatePatient(),
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true);

        Assert.False(result.IsAllowed);
        Assert.Contains("Kein Schnittstellenprofil vorhanden.", result.Reasons);
    }

    private static InterfaceProfileDefinition CreateProfile(
        bool isActive = true,
        bool isAttachmentProcessingEnabled = true,
        string attachmentImportFolder = @"C:\XdtDeviceBridge\AttachmentsIn",
        string attachmentExportFolder = @"C:\XdtDeviceBridge\AttachmentsOut")
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        return profile with
        {
            IsActive = isActive,
            FolderOptions = profile.FolderOptions with
            {
                IsAttachmentProcessingEnabled = isAttachmentProcessingEnabled,
                AttachmentImportFolder = attachmentImportFolder,
                AttachmentExportFolder = attachmentExportFolder
            }
        };
    }

    private static PatientData CreatePatient(string? patientNumber = "11253")
    {
        return new PatientData(
            PatientNumber: patientNumber,
            LastName: null,
            FirstName: null,
            BirthDate: null,
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
    }
}
