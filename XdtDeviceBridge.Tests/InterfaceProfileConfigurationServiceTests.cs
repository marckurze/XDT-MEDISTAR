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
            attachmentTransferMode: AttachmentTransferMode.Move);

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
        AttachmentTransferMode attachmentTransferMode = AttachmentTransferMode.Move)
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
            AttachmentTransferMode: attachmentTransferMode);
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
