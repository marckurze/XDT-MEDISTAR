using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileDefinitionTests
{
    [Fact]
    public void CreateMedistarNidekArk1sDefault_ShouldCreateProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-nidek-ark1s-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-nidek-ark1s-default", profile.ExportProfileId);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.NotNull(profile.FolderOptions);
    }

    [Fact]
    public void Validate_ShouldAcceptInactiveDefaultProfileWithEmptyFolders()
    {
        var issues = InterfaceProfileDefinitionValidator.Validate(DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault());

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportWrongProfileKind()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(ProfileKind.ExportProfile)
        };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("Metadata.ProfileKind must be InterfaceProfile.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyAisProfileId()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with { AisProfileId = "" };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AisProfileId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyDeviceProfileId()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with { DeviceProfileId = " " };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("DeviceProfileId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyExportProfileId()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with { ExportProfileId = "" };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ExportProfileId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldReportActiveProfileWithoutRequiredFolders()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = true,
            FolderOptions = CreateFolderOptions(errorFolder: @"C:\XdtDeviceBridge\errors")
        };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AisImportFolder must be set when profile is active.", issues);
        Assert.Contains("DeviceImportFolder must be set when profile is active.", issues);
        Assert.Contains("ExportFolder must be set when profile is active.", issues);
    }

    [Fact]
    public void Validate_ShouldReportClearAisImportFolderWithoutAisImportFolder()
    {
        var profile = WithFolderOptions(CreateFolderOptions(clearAisImportFolderBeforeProcessing: true));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AisImportFolder must be set when ClearAisImportFolderBeforeProcessing is true.", issues);
    }

    [Fact]
    public void Validate_ShouldReportClearDeviceImportFolderWithoutDeviceImportFolder()
    {
        var profile = WithFolderOptions(CreateFolderOptions(clearDeviceImportFolderBeforeProcessing: true));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("DeviceImportFolder must be set when ClearDeviceImportFolderBeforeProcessing is true.", issues);
    }

    [Fact]
    public void Validate_ShouldReportClearExportFolderWithoutExportFolder()
    {
        var profile = WithFolderOptions(CreateFolderOptions(clearExportFolderAfterSuccessfulTransfer: true));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ExportFolder must be set when ClearExportFolderAfterSuccessfulTransfer is true.", issues);
    }

    [Fact]
    public void Validate_ShouldReportArchiveProcessedFilesWithoutArchiveFolder()
    {
        var profile = WithFolderOptions(CreateFolderOptions(archiveProcessedFiles: true));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ArchiveFolder must be set when ArchiveProcessedFiles is true.", issues);
    }

    [Fact]
    public void Validate_ShouldReportMoveFailedFilesToErrorFolderWithoutErrorFolder()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            aisImportFolder: @"C:\XdtDeviceBridge\ais",
            deviceImportFolder: @"C:\XdtDeviceBridge\devices",
            exportFolder: @"C:\XdtDeviceBridge\export",
            moveFailedFilesToErrorFolder: true)) with
        {
            IsActive = true
        };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ErrorFolder must be set when MoveFailedFilesToErrorFolder is true.", issues);
    }

    [Fact]
    public void CreateMedistarNidekArk1sDefault_ShouldDisableDeleteOptionsByDefault()
    {
        var options = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().FolderOptions;

        Assert.False(options.ClearAisImportFolderBeforeProcessing);
        Assert.False(options.ClearDeviceImportFolderBeforeProcessing);
        Assert.False(options.ClearExportFolderAfterSuccessfulTransfer);
        Assert.False(options.ArchiveProcessedFiles);
        Assert.True(options.MoveFailedFilesToErrorFolder);
        Assert.Equal(ArchiveProcessedFileMode.Copy, options.ArchiveProcessedFileMode);
        Assert.Null(options.ArchiveRetentionDays);
        Assert.Equal(string.Empty, options.AttachmentImportFolder);
        Assert.Equal(string.Empty, options.AttachmentExportFolder);
        Assert.Equal(AttachmentFileNameBuilder.DefaultTemplate, options.AttachmentFileNameTemplate);
        Assert.Equal(AttachmentTransferMode.Move, options.AttachmentTransferMode);
        Assert.Equal("Datei", options.AttachmentExternalLinkDocumentName);
        Assert.Equal("{ExtensionUpperWithoutDot}", options.AttachmentExternalLinkFileFormat);
        Assert.Equal(string.Empty, options.AttachmentExternalLinkDescription);
        Assert.Equal("{Attachment.TargetFullPath}", options.AttachmentExternalLinkPathTemplate);
        Assert.False(options.IsAttachmentProcessingEnabled);
        Assert.Equal(AttachmentRequirementMode.Optional, options.AttachmentRequirementMode);
        Assert.Equal(30, options.AttachmentWaitTimeoutSeconds);
        Assert.Equal(2, options.AttachmentFileStabilityWaitSeconds);
        Assert.Equal(5, options.AutoImportScanIntervalSeconds);
        Assert.Equal(10, options.DeviceFileWaitTimeoutMinutes);
    }

    [Fact]
    public void CreateMedistarManualDocumentTransferDefault_ShouldUseManualSelectionOptions()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault();

        Assert.Equal("interface-medistar-manual-document-transfer-default", profile.Metadata.Id);
        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-manual-document-selection-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-manual-document-transfer-default", profile.ExportProfileId);
        Assert.True(profile.FolderOptions.IsAttachmentOnlyMode);
        Assert.True(profile.FolderOptions.ShowAttachmentDocumentationDialog);
        Assert.Equal(AttachmentCompletionMode.ManualConfirmation, profile.FolderOptions.AttachmentCompletionMode);
        Assert.Equal(AttachmentOnlySourceMode.ManualUserSelection, profile.FolderOptions.AttachmentOnlySourceMode);
        Assert.Equal(AttachmentTransferMode.Copy, profile.FolderOptions.AttachmentTransferMode);
        Assert.Empty(profile.FolderOptions.DeviceImportFolder);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void CreateMedistarTopconCl300Default_ShouldCreateProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCl300Default();

        Assert.Equal("interface-medistar-topcon-cl300-default", profile.Metadata.Id);
        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-topcon-cl300-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-cl300-default", profile.ExportProfileId);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void CreateMedistarTopconKr800Default_ShouldCreateProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default();

        Assert.Equal("interface-medistar-topcon-kr800-default", profile.Metadata.Id);
        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-topcon-kr800-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-kr800-default", profile.ExportProfileId);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void CreateMedistarTopconTrk2PDefault_ShouldCreateProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconTrk2PDefault();

        Assert.Equal("interface-medistar-topcon-trk2p-default", profile.Metadata.Id);
        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-topcon-trk2p-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-trk2p-default", profile.ExportProfileId);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void CreateMedistarTopconCt1PDefault_ShouldCreateProfile()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt1PDefault();

        Assert.Equal("interface-medistar-topcon-ct1p-default", profile.Metadata.Id);
        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-topcon-ct1p-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-topcon-ct1p-default", profile.ExportProfileId);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.False(profile.FolderOptions.IsAttachmentProcessingEnabled);
        Assert.Empty(InterfaceProfileDefinitionValidator.Validate(profile));
    }

    [Fact]
    public void Validate_ShouldAcceptEmptyAttachmentFolders()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            attachmentImportFolder: string.Empty,
            attachmentExportFolder: string.Empty));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldAcceptEmptyAttachmentExternalLinkFields()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            attachmentExternalLinkDocumentName: string.Empty,
            attachmentExternalLinkFileFormat: string.Empty,
            attachmentExternalLinkDescription: string.Empty,
            attachmentExternalLinkPathTemplate: string.Empty));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportNegativeArchiveRetentionDays()
    {
        var profile = WithFolderOptions(CreateFolderOptions(archiveRetentionDays: -1));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ArchiveRetentionDays must not be negative.", issues);
    }

    [Fact]
    public void Validate_ShouldReportArchiveRetentionWithoutArchiveFolderWhenArchiveIsEnabled()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            archiveProcessedFiles: true,
            archiveRetentionDays: 30));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ArchiveFolder must be set when ArchiveRetentionDays is configured for archived files.", issues);
    }

    [Fact]
    public void Validate_ShouldReportMoveModeWithoutArchiveProcessedFiles()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            archiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            archiveProcessedFiles: false));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("ArchiveProcessedFiles must be true when ArchiveProcessedFileMode is Move.", issues);
    }

    [Fact]
    public void Validate_ShouldReportInvalidAttachmentTransferMode()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            attachmentTransferMode: (AttachmentTransferMode)999));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AttachmentTransferMode must be a valid value.", issues);
    }

    [Fact]
    public void Validate_ShouldReportInvalidAttachmentRequirementMode()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            attachmentRequirementMode: (AttachmentRequirementMode)999));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AttachmentRequirementMode must be a valid value.", issues);
    }

    [Fact]
    public void Validate_ShouldReportNegativeAttachmentWaitTimeout()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            attachmentWaitTimeoutSeconds: -1));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AttachmentWaitTimeoutSeconds must not be negative.", issues);
    }

    [Fact]
    public void Validate_ShouldReportNegativeAttachmentFileStabilityWait()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            attachmentFileStabilityWaitSeconds: -1));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AttachmentFileStabilityWaitSeconds must not be negative.", issues);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ShouldReportTooSmallAutoImportScanInterval(int intervalSeconds)
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            autoImportScanIntervalSeconds: intervalSeconds));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AutoImportScanIntervalSeconds must be at least 1.", issues);
    }

    [Fact]
    public void Validate_ShouldReportNegativeDeviceFileWaitTimeout()
    {
        var profile = WithFolderOptions(CreateFolderOptions(
            deviceFileWaitTimeoutMinutes: -1));

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("DeviceFileWaitTimeoutMinutes must not be negative.", issues);
    }

    [Fact]
    public void Validate_ShouldAcceptManualDocumentTransferWithoutDeviceImportFolder()
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault() with
        {
            IsActive = true,
            FolderOptions = DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault().FolderOptions with
            {
                AisImportFolder = @"C:\XdtDeviceBridge\ais",
                DeviceImportFolder = string.Empty,
                ExportFolder = @"C:\XdtDeviceBridge\export",
                ErrorFolder = @"C:\XdtDeviceBridge\errors",
                AttachmentExportFolder = @"C:\XdtDeviceBridge\attachments"
            }
        };

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.DoesNotContain("DeviceImportFolder must be set when profile is active.", issues);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportInvalidAttachmentOnlySourceMode()
    {
        var profile = WithFolderOptions(CreateFolderOptions() with
        {
            AttachmentOnlySourceMode = (AttachmentOnlySourceMode)999
        });

        var issues = InterfaceProfileDefinitionValidator.Validate(profile);

        Assert.Contains("AttachmentOnlySourceMode must be a valid value.", issues);
    }

    private static InterfaceProfileDefinition WithFolderOptions(InterfaceFolderOptions options)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            FolderOptions = options
        };
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
        bool moveFailedFilesToErrorFolder = false,
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

    private static ProfileMetadata CreateMetadata(ProfileKind profileKind)
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);

        return new ProfileMetadata(
            Id: "metadata-test",
            Name: "Metadata Test",
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
