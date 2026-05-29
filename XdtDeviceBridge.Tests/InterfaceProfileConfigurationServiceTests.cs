using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileConfigurationServiceTests
{
    private readonly InterfaceProfileConfigurationService _service = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 5, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateForExportProfile_ShouldCreateInactiveInterfaceProfile()
    {
        var exportProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        var profile = _service.CreateForExportProfile(
            exportProfile,
            _timestamp,
            "TestUser",
            idFactory: () => "interface-export-copy");

        Assert.Equal("interface-export-copy", profile.Metadata.Id);
        Assert.Equal("Schnittstelle - MEDISTAR + NIDEK ARK1S Export", profile.Metadata.Name);
        Assert.Equal(ProfileKind.InterfaceProfile, profile.Metadata.ProfileKind);
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.Equal(exportProfile.TargetAisProfileId, profile.AisProfileId);
        Assert.Equal(exportProfile.SourceDeviceProfileId, profile.DeviceProfileId);
        Assert.Equal(exportProfile.Metadata.Id, profile.ExportProfileId);
        Assert.False(profile.FolderOptions.ClearAisImportFolderBeforeProcessing);
        Assert.False(profile.FolderOptions.ClearDeviceImportFolderBeforeProcessing);
        Assert.False(profile.FolderOptions.ClearExportFolderAfterSuccessfulTransfer);
        Assert.False(profile.FolderOptions.ArchiveProcessedFiles);
        Assert.True(profile.FolderOptions.MoveFailedFilesToErrorFolder);
        Assert.Equal(ArchiveProcessedFileMode.Copy, profile.FolderOptions.ArchiveProcessedFileMode);
        Assert.Null(profile.FolderOptions.ArchiveRetentionDays);
        Assert.Equal(string.Empty, profile.FolderOptions.AttachmentImportFolder);
        Assert.Equal(string.Empty, profile.FolderOptions.AttachmentExportFolder);
        Assert.Equal(AttachmentFileNameBuilder.DefaultTemplate, profile.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, profile.FolderOptions.AttachmentTransferMode);
        Assert.Equal("Datei", profile.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", profile.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal(string.Empty, profile.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", profile.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Optional, profile.FolderOptions.AttachmentRequirementMode);
        Assert.Equal(30, profile.FolderOptions.AttachmentWaitTimeoutSeconds);
        Assert.Equal(2, profile.FolderOptions.AttachmentFileStabilityWaitSeconds);
        Assert.Equal(5, profile.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.Equal(10, profile.FolderOptions.DeviceFileWaitTimeoutMinutes);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldSaveBuiltInAsUserDefinedCopy()
    {
        var builtInProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var options = CreateFolderOptions(aisImportFolder: @"\\SERVER\Freigabe\XDT\AIS");

        var result = _service.CreateConfiguredProfile(
            builtInProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser",
            idFactory: () => "interface-config-copy");

        Assert.True(result.Success);
        Assert.NotNull(result.Profile);
        Assert.Equal("interface-config-copy", result.Profile!.Metadata.Id);
        Assert.NotEqual(builtInProfile.Metadata.Id, result.Profile.Metadata.Id);
        Assert.Equal("MEDISTAR + NIDEK ARK1S - Konfiguration", result.Profile.Metadata.Name);
        Assert.False(result.Profile.Metadata.IsBuiltIn);
        Assert.True(result.Profile.Metadata.IsUserDefined);
        Assert.Equal(@"\\SERVER\Freigabe\XDT\AIS", result.Profile.FolderOptions.AisImportFolder);
        Assert.True(builtInProfile.Metadata.IsBuiltIn);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldUpdateUserDefinedProfile()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(exportFolder: @"\\SERVER\Freigabe\XDT\Export");

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: false,
            _timestamp,
            "TestUser");

        Assert.True(result.Success);
        Assert.Equal("interface-user", result.Profile!.Metadata.Id);
        Assert.False(result.Profile.IsLicenseRequired);
        Assert.Equal(_timestamp, result.Profile.Metadata.UpdatedAt);
        Assert.Equal(@"\\SERVER\Freigabe\XDT\Export", result.Profile.FolderOptions.ExportFolder);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldPersistNidekRtSerialSendMode()
    {
        var builtInProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekRt3100SerialDefault();
        var options = CreateFolderOptions(aisImportFolder: @"\\SERVER\Freigabe\XDT\AIS");

        var result = _service.CreateConfiguredProfile(
            builtInProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            builtInProfile.DeviceOutput,
            builtInProfile.SerialSettings,
            NidekRtSerialSendMode.RsSdHandshake,
            _timestamp,
            "TestUser",
            idFactory: () => "interface-rt3100-config");

        Assert.True(result.Success);
        Assert.Equal(NidekRtSerialSendMode.RsSdHandshake, result.Profile!.NidekRtSerialSendMode);
        Assert.False(result.Profile.Metadata.IsBuiltIn);
        Assert.True(result.Profile.Metadata.IsUserDefined);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldPreserveAttachmentFolders()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(
            attachmentImportFolder: @"\\SERVER\Freigabe\XDT\GAImport",
            attachmentExportFolder: @"\\SERVER\Freigabe\XDT\GAExport",
            attachmentFileNameTemplate: "GA_{Ais.PatientNumber}{ExtensionUpper}",
            attachmentTransferMode: AttachmentTransferMode.Move,
            attachmentExternalLinkDocumentName: "PDF-Befund",
            attachmentExternalLinkFileFormat: "{ExtensionUpperWithoutDot}",
            attachmentExternalLinkDescription: "Messprotokoll Autorefraktor",
            attachmentExternalLinkPathTemplate: "{Attachment.TargetFullPath}",
            isAttachmentProcessingEnabled: true,
            attachmentRequirementMode: AttachmentRequirementMode.Required,
            attachmentWaitTimeoutSeconds: 45,
            attachmentFileStabilityWaitSeconds: 3,
            autoImportScanIntervalSeconds: 7,
            deviceFileWaitTimeoutMinutes: 12);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.True(result.Success);
        Assert.Equal(@"\\SERVER\Freigabe\XDT\GAImport", result.Profile!.FolderOptions.AttachmentImportFolder);
        Assert.Equal(@"\\SERVER\Freigabe\XDT\GAExport", result.Profile.FolderOptions.AttachmentExportFolder);
        Assert.Equal("GA_{Ais.PatientNumber}{ExtensionUpper}", result.Profile.FolderOptions.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, result.Profile.FolderOptions.AttachmentTransferMode);
        Assert.Equal("PDF-Befund", result.Profile.FolderOptions.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", result.Profile.FolderOptions.AttachmentExternalLinkFileFormat);
        Assert.Equal("Messprotokoll Autorefraktor", result.Profile.FolderOptions.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", result.Profile.FolderOptions.AttachmentExternalLinkPathTemplate);
        Assert.True(result.Profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Required, result.Profile.FolderOptions.AttachmentRequirementMode);
        Assert.Equal(45, result.Profile.FolderOptions.AttachmentWaitTimeoutSeconds);
        Assert.Equal(3, result.Profile.FolderOptions.AttachmentFileStabilityWaitSeconds);
        Assert.Equal(7, result.Profile.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.Equal(12, result.Profile.FolderOptions.DeviceFileWaitTimeoutMinutes);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldPreserveExplicitAttachmentTransferModeCopy()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(attachmentTransferMode: AttachmentTransferMode.Copy);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.True(result.Success);
        Assert.Equal(AttachmentTransferMode.Copy, result.Profile!.FolderOptions.AttachmentTransferMode);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportDangerousAttachmentFolder()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(
            attachmentImportFolder: Path.GetPathRoot(Path.GetTempPath())!);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "Folder path must not be a drive or share root.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportDangerousCleanupPath()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(
            aisImportFolder: Path.GetPathRoot(Path.GetTempPath())!,
            clearAisImportFolderBeforeProcessing: true);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "Folder path must not be a drive or share root.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldAcceptUncPathAsTextWithoutCleanup()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(
            aisImportFolder: @"\\SERVER\Freigabe\XDT\AIS",
            deviceImportFolder: @"\\SERVER\Freigabe\XDT\Device",
            exportFolder: @"\\SERVER\Freigabe\XDT\Export");

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.True(result.Success);
        Assert.DoesNotContain(result.Issues, issue => issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error);
        Assert.Equal(@"\\SERVER\Freigabe\XDT\AIS", result.Profile!.FolderOptions.AisImportFolder);
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldWarnForMissingFolderWhenNoCleanupIsActive()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var missingFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "missing");
        var options = CreateFolderOptions(aisImportFolder: missingFolder);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.True(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Warning
            && issue.Message == "AIS-Importordner existiert aktuell nicht.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldPreserveSerialSettingsAndSkipDeviceImportFolderWarning()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-serial"),
            SerialSettings = SerialCommunicationSettings.Default with { PortName = "COM1" }
        };
        var options = CreateFolderOptions(
            aisImportFolder: Path.GetTempPath(),
            deviceImportFolder: string.Empty,
            exportFolder: Path.GetTempPath(),
            errorFolder: Path.GetTempPath());
        var serialSettings = SerialCommunicationSettings.Default with
        {
            PortName = "COM9",
            BaudRate = 19200,
            IsBidirectional = true
        };

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            deviceOutput: userProfile.DeviceOutput,
            serialSettings: serialSettings,
            timestamp: _timestamp,
            createdBy: "TestUser");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues.Select(issue => issue.Message)));
        Assert.Equal(serialSettings, result.Profile!.SerialSettings);
        Assert.DoesNotContain(result.Issues, issue => issue.Message == "Geräte-Importordner existiert aktuell nicht.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportInvalidArchiveRetentionDays()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(archiveRetentionDays: -1);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "ArchiveRetentionDays must not be negative.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportInvalidAttachmentWaitTimeout()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(attachmentWaitTimeoutSeconds: -1);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "AttachmentWaitTimeoutSeconds must not be negative.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportInvalidAttachmentFileStabilityWait()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(attachmentFileStabilityWaitSeconds: -1);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "AttachmentFileStabilityWaitSeconds must not be negative.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportInvalidAutoImportScanInterval()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(autoImportScanIntervalSeconds: 0);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "AutoImportScanIntervalSeconds must be at least 1.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportInvalidDeviceFileWaitTimeout()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(deviceFileWaitTimeoutMinutes: -1);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "tester");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "DeviceFileWaitTimeoutMinutes must not be negative.");
    }

    [Fact]
    public void CreateConfiguredProfile_ShouldReportMoveModeWithoutArchiveProcessing()
    {
        var userProfile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserMetadata("interface-user")
        };
        var options = CreateFolderOptions(archiveProcessedFileMode: ArchiveProcessedFileMode.Move);

        var result = _service.CreateConfiguredProfile(
            userProfile,
            options,
            isActive: false,
            isLicenseRequired: true,
            _timestamp,
            "TestUser");

        Assert.False(result.Success);
        Assert.Contains(result.Issues, issue =>
            issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error
            && issue.Message == "ArchiveProcessedFiles must be true when ArchiveProcessedFileMode is Move.");
    }

    private static InterfaceFolderOptions CreateFolderOptions(
        string aisImportFolder = "",
        string deviceImportFolder = "",
        string exportFolder = "",
        string archiveFolder = "",
        string errorFolder = "",
        bool clearAisImportFolderBeforeProcessing = false,
        bool clearDeviceImportFolderBeforeProcessing = false,
        bool clearExportFolderAfterSuccessfulTransfer = false,
        bool archiveProcessedFiles = false,
        bool moveFailedFilesToErrorFolder = true,
        ArchiveProcessedFileMode archiveProcessedFileMode = ArchiveProcessedFileMode.Copy,
        int? archiveRetentionDays = null,
        string attachmentImportFolder = "",
        string attachmentExportFolder = "",
        string attachmentFileNameTemplate = AttachmentFileNameBuilder.DefaultTemplate,
        AttachmentTransferMode attachmentTransferMode = AttachmentTransferMode.Move,
        string attachmentExternalLinkDocumentName = "Datei",
        string attachmentExternalLinkFileFormat = "{ExtensionUpperWithoutDot}",
        string attachmentExternalLinkDescription = "",
        string attachmentExternalLinkPathTemplate = "{Attachment.TargetFullPath}",
        bool isAttachmentProcessingEnabled = false,
        AttachmentRequirementMode attachmentRequirementMode = AttachmentRequirementMode.Optional,
        int attachmentWaitTimeoutSeconds = 30,
        int attachmentFileStabilityWaitSeconds = 2,
        int autoImportScanIntervalSeconds = 5,
        int deviceFileWaitTimeoutMinutes = 10)
    {
        return new InterfaceFolderOptions(
            AisImportFolder: aisImportFolder,
            DeviceImportFolder: deviceImportFolder,
            ExportFolder: exportFolder,
            ArchiveFolder: archiveFolder,
            ErrorFolder: errorFolder,
            ClearAisImportFolderBeforeProcessing: clearAisImportFolderBeforeProcessing,
            ClearDeviceImportFolderBeforeProcessing: clearDeviceImportFolderBeforeProcessing,
            ClearExportFolderAfterSuccessfulTransfer: clearExportFolderAfterSuccessfulTransfer,
            ArchiveProcessedFiles: archiveProcessedFiles,
            MoveFailedFilesToErrorFolder: moveFailedFilesToErrorFolder,
            ArchiveProcessedFileMode: archiveProcessedFileMode,
            ArchiveRetentionDays: archiveRetentionDays,
            AttachmentImportFolder: attachmentImportFolder,
            AttachmentExportFolder: attachmentExportFolder,
            AttachmentFileNameTemplate: attachmentFileNameTemplate,
            AttachmentTransferMode: attachmentTransferMode,
            AttachmentExternalLinkDocumentName: attachmentExternalLinkDocumentName,
            AttachmentExternalLinkFileFormat: attachmentExternalLinkFileFormat,
            AttachmentExternalLinkDescription: attachmentExternalLinkDescription,
            AttachmentExternalLinkPathTemplate: attachmentExternalLinkPathTemplate,
            IsAttachmentProcessingEnabled: isAttachmentProcessingEnabled,
            AttachmentRequirementMode: attachmentRequirementMode,
            AttachmentWaitTimeoutSeconds: attachmentWaitTimeoutSeconds,
            AttachmentFileStabilityWaitSeconds: attachmentFileStabilityWaitSeconds,
            AutoImportScanIntervalSeconds: autoImportScanIntervalSeconds,
            DeviceFileWaitTimeoutMinutes: deviceFileWaitTimeoutMinutes);
    }

    private static ProfileMetadata CreateUserMetadata(string id)
    {
        var timestamp = new DateTimeOffset(2026, 5, 4, 12, 0, 0, TimeSpan.Zero);
        return new ProfileMetadata(
            Id: id,
            Name: "User Interface",
            ProfileKind: ProfileKind.InterfaceProfile,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "TestUser",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
