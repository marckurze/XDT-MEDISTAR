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
        bool moveFailedFilesToErrorFolder = false)
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
            MoveFailedFilesToErrorFolder: moveFailedFilesToErrorFolder);
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
