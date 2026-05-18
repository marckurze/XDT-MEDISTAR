using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AttachmentPackageDecisionServiceTests
{
    private readonly AttachmentPackageDecisionService _service = new();

    [Fact]
    public void Decide_ShouldContinueWithoutAttachmentWhenAttachmentProcessingIsDisabled()
    {
        var result = Decide(CreateProfile(isAttachmentProcessingEnabled: false), CreatePatient(), CreateScanResult(), hasWaitTimedOut: true);

        Assert.True(result.CanContinueWithoutAttachment);
        Assert.False(result.CanProcessAttachment);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentDisabled, result.Reason);
    }

    [Fact]
    public void Decide_ShouldWaitForOptionalAttachmentBeforeTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Optional),
            CreatePatient(),
            CreateScanResult(),
            hasWaitTimedOut: false);

        Assert.True(result.ShouldWait);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentOptionalWait, result.Reason);
    }

    [Fact]
    public void Decide_ShouldWaitForRequiredAttachmentBeforeTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Required),
            CreatePatient(),
            CreateScanResult(),
            hasWaitTimedOut: false);

        Assert.True(result.ShouldWait);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentRequiredWait, result.Reason);
    }

    [Fact]
    public void Decide_ShouldContinueWithoutAttachmentForOptionalTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Optional),
            CreatePatient(),
            CreateScanResult(),
            hasWaitTimedOut: true);

        Assert.True(result.CanContinueWithoutAttachment);
        Assert.False(result.ShouldBlock);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment, result.Reason);
    }

    [Fact]
    public void Decide_ShouldBlockForRequiredTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Required),
            CreatePatient(),
            CreateScanResult(),
            hasWaitTimedOut: true);

        Assert.True(result.ShouldBlock);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock, result.Reason);
    }

    [Theory]
    [InlineData(AttachmentRequirementMode.Optional)]
    [InlineData(AttachmentRequirementMode.Required)]
    public void Decide_ShouldProcessSingleSupportedAttachment(AttachmentRequirementMode requirementMode)
    {
        var candidate = CreateCandidate("report.pdf", isSupported: true);

        var result = Decide(
            CreateProfile(requirementMode: requirementMode),
            CreatePatient(),
            CreateScanResult(candidate),
            hasWaitTimedOut: false);

        Assert.True(result.CanProcessAttachment);
        Assert.False(result.ShouldWait);
        Assert.False(result.ShouldBlock);
        Assert.Equal(candidate, result.SelectedCandidate);
        Assert.Single(result.SelectedCandidates);
        Assert.Equal(AttachmentPackageDecisionReason.SingleAttachmentReady, result.Reason);
    }

    [Fact]
    public void Decide_ShouldWaitWhenSingleSupportedAttachmentIsNotStableBeforeTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Optional),
            CreatePatient(),
            CreateScanResult(CreateCandidate("report.pdf", isSupported: true, isStable: false)),
            hasWaitTimedOut: false);

        Assert.True(result.ShouldWait);
        Assert.False(result.CanProcessAttachment);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentNotStableWait, result.Reason);
    }

    [Fact]
    public void Decide_ShouldContinueWithoutAttachmentWhenOptionalAttachmentIsNotStableAfterTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Optional),
            CreatePatient(),
            CreateScanResult(CreateCandidate("report.pdf", isSupported: true, isStable: false)),
            hasWaitTimedOut: true);

        Assert.True(result.CanContinueWithoutAttachment);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment, result.Reason);
    }

    [Fact]
    public void Decide_ShouldBlockWhenRequiredAttachmentIsNotStableAfterTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Required),
            CreatePatient(),
            CreateScanResult(CreateCandidate("report.pdf", isSupported: true, isStable: false)),
            hasWaitTimedOut: true);

        Assert.True(result.ShouldBlock);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock, result.Reason);
    }

    [Fact]
    public void Decide_ShouldProcessOptionalMultipleStableSupportedAttachments()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Optional),
            CreatePatient(),
            CreateScanResult(
                CreateCandidate("report-1.pdf", isSupported: true),
                CreateCandidate("report-2.pdf", isSupported: true)),
            hasWaitTimedOut: false);

        Assert.True(result.CanProcessAttachment);
        Assert.False(result.CanContinueWithoutAttachment);
        Assert.Equal(2, result.SelectedCandidates.Count);
        Assert.Equal(new[] { "report-1.pdf", "report-2.pdf" }, result.SelectedCandidates.Select(candidate => candidate.FileName));
        Assert.Equal(AttachmentPackageDecisionReason.MultipleAttachmentsReady, result.Reason);
    }

    [Fact]
    public void Decide_ShouldProcessRequiredMultipleStableSupportedAttachments()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Required),
            CreatePatient(),
            CreateScanResult(
                CreateCandidate("report-1.pdf", isSupported: true),
                CreateCandidate("report-2.pdf", isSupported: true)),
            hasWaitTimedOut: false);

        Assert.True(result.CanProcessAttachment);
        Assert.False(result.ShouldBlock);
        Assert.Equal(2, result.SelectedCandidates.Count);
        Assert.Equal(AttachmentPackageDecisionReason.MultipleAttachmentsReady, result.Reason);
    }

    [Fact]
    public void Decide_ShouldWaitWhenMultipleSupportedAttachmentsAreNotAllStableBeforeTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Required),
            CreatePatient(),
            CreateScanResult(
                CreateCandidate("report-1.pdf", isSupported: true, isStable: true),
                CreateCandidate("report-2.jpg", isSupported: true, isStable: false)),
            hasWaitTimedOut: false);

        Assert.True(result.ShouldWait);
        Assert.False(result.CanProcessAttachment);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentNotStableWait, result.Reason);
    }

    [Fact]
    public void Decide_ShouldBlockRequiredWhenMultipleSupportedAttachmentsAreNotAllStableAfterTimeout()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Required),
            CreatePatient(),
            CreateScanResult(
                CreateCandidate("report-1.pdf", isSupported: true, isStable: true),
                CreateCandidate("report-2.jpg", isSupported: true, isStable: false)),
            hasWaitTimedOut: true);

        Assert.True(result.ShouldBlock);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentRequiredTimeoutBlock, result.Reason);
    }

    [Fact]
    public void Decide_ShouldBlockWhenPatientNumberIsMissing()
    {
        var result = Decide(CreateProfile(), CreatePatient(patientNumber: ""), CreateScanResult(), hasWaitTimedOut: true);

        Assert.True(result.ShouldBlock);
        Assert.Equal(AttachmentPackageDecisionReason.MissingPatientNumber, result.Reason);
    }

    [Theory]
    [InlineData("", @"C:\XdtDeviceBridge\AttachmentsOut")]
    [InlineData(@"C:\XdtDeviceBridge\AttachmentsIn", "")]
    public void Decide_ShouldBlockWhenAttachmentFoldersAreMissing(string importFolder, string exportFolder)
    {
        var result = Decide(
            CreateProfile(attachmentImportFolder: importFolder, attachmentExportFolder: exportFolder),
            CreatePatient(),
            CreateScanResult(),
            hasWaitTimedOut: true);

        Assert.True(result.ShouldBlock);
        Assert.Equal(AttachmentPackageDecisionReason.MissingAttachmentFolders, result.Reason);
    }

    [Fact]
    public void Decide_ShouldIgnoreUnsupportedCandidatesForAutomaticSelection()
    {
        var result = Decide(
            CreateProfile(requirementMode: AttachmentRequirementMode.Optional),
            CreatePatient(),
            CreateScanResult(CreateCandidate("readme.exe", isSupported: false)),
            hasWaitTimedOut: true);

        Assert.True(result.CanContinueWithoutAttachment);
        Assert.False(result.CanProcessAttachment);
        Assert.Null(result.SelectedCandidate);
        Assert.Empty(result.SelectedCandidates);
        Assert.Equal(AttachmentPackageDecisionReason.AttachmentOptionalTimeoutContinueWithoutAttachment, result.Reason);
    }

    private AttachmentPackageDecisionResult Decide(
        InterfaceProfileDefinition profile,
        PatientData patient,
        AttachmentImportFolderScanResult scanResult,
        bool hasWaitTimedOut)
    {
        return _service.Decide(
            profile,
            patient,
            scanResult,
            isMonitoringRunning: true,
            isGlobalAutomaticProcessingEnabled: true,
            hasWaitTimedOut);
    }

    private static InterfaceProfileDefinition CreateProfile(
        bool isActive = true,
        bool isAttachmentProcessingEnabled = true,
        AttachmentRequirementMode requirementMode = AttachmentRequirementMode.Optional,
        string attachmentImportFolder = @"C:\XdtDeviceBridge\AttachmentsIn",
        string attachmentExportFolder = @"C:\XdtDeviceBridge\AttachmentsOut")
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = isActive,
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions with
            {
                AisImportFolder = @"C:\XdtDeviceBridge\Ais",
                DeviceImportFolder = @"C:\XdtDeviceBridge\Device",
                ExportFolder = @"C:\XdtDeviceBridge\Export",
                ErrorFolder = @"C:\XdtDeviceBridge\Error",
                IsAttachmentProcessingEnabled = isAttachmentProcessingEnabled,
                AttachmentRequirementMode = requirementMode,
                AttachmentImportFolder = attachmentImportFolder,
                AttachmentExportFolder = attachmentExportFolder
            }
        };
    }

    private static PatientData CreatePatient(string? patientNumber = "11253")
    {
        return new PatientData(
            PatientNumber: patientNumber,
            LastName: "Muster",
            FirstName: "Mara",
            BirthDate: null,
            PostalCodeCity: null,
            Street: null,
            GenderCode: null,
            SourceSystem: null,
            TargetSystem: null,
            GdtVersion: null,
            ExaminationType: null);
    }

    private static AttachmentImportFolderScanResult CreateScanResult(params AttachmentImportFileCandidate[] candidates)
    {
        return new AttachmentImportFolderScanResult(
            Success: true,
            ScannedFolder: @"C:\XdtDeviceBridge\AttachmentsIn",
            Candidates: candidates,
            ErrorMessage: null);
    }

    private static AttachmentImportFileCandidate CreateCandidate(string fileName, bool isSupported)
    {
        return CreateCandidate(fileName, isSupported, isStable: isSupported);
    }

    private static AttachmentImportFileCandidate CreateCandidate(string fileName, bool isSupported, bool isStable)
    {
        return new AttachmentImportFileCandidate(
            FileName: fileName,
            Extension: Path.GetExtension(fileName).ToLowerInvariant(),
            FullPath: Path.Combine(@"C:\XdtDeviceBridge\AttachmentsIn", fileName),
            SizeBytes: 123,
            LastWriteTimeUtc: new DateTime(2026, 5, 8, 12, 0, 0, DateTimeKind.Utc),
            IsSupported: isSupported,
            StableStatus: isStable ? "Stabil." : "Noch nicht stabil.",
            ErrorMessage: isSupported ? null : "Dateityp wird für XDT-Anhänge nicht unterstützt.",
            IsStable: isStable);
    }
}
