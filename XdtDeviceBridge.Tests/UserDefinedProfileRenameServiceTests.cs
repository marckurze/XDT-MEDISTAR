using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class UserDefinedProfileRenameServiceTests
{
    private readonly ProfileCatalogService _catalogService = new();
    private readonly UserDefinedProfileRenameService _renameService = new();

    [Fact]
    public void Rename_ShouldRenameUserDefinedAisProfileAndKeepIdAndSettings()
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Rename(
            workspace.Catalog,
            workspace.Paths,
            UserDefinedProfileRenameKind.AisProfile,
            workspace.UserAis.Metadata.Id,
            "Praxis AIS Empfang");

        var renamed = LoadCatalog(workspace).AisProfiles.Single(profile => profile.Metadata.Id == workspace.UserAis.Metadata.Id);
        Assert.True(result.Success);
        Assert.Equal("Der Profilname wurde geändert.", result.Message);
        Assert.Equal(workspace.UserAis.Metadata.Id, renamed.Metadata.Id);
        Assert.Equal("Praxis AIS Empfang", renamed.Metadata.Name);
        Assert.Equal("Praxis AIS Empfang", renamed.Name);
        Assert.Equal(workspace.UserAis.Vendor, renamed.Vendor);
        Assert.Equal(workspace.UserAis.DefaultEncoding, renamed.DefaultEncoding);
        Assert.Equal(workspace.UserAis.RequiredStaticFields, renamed.RequiredStaticFields);
        Assert.False(renamed.Metadata.IsBuiltIn);
        Assert.True(renamed.Metadata.IsUserDefined);
    }

    [Fact]
    public void Rename_ShouldRenameUserDefinedDeviceProfileAndKeepParserAndMeasurements()
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Rename(
            workspace.Catalog,
            workspace.Paths,
            UserDefinedProfileRenameKind.DeviceProfile,
            workspace.UserDevice.Metadata.Id,
            "LM7 Praxisraum 1");

        var renamed = LoadCatalog(workspace).DeviceProfiles.Single(profile => profile.Metadata.Id == workspace.UserDevice.Metadata.Id);
        Assert.True(result.Success);
        Assert.Equal(workspace.UserDevice.Metadata.Id, renamed.Metadata.Id);
        Assert.Equal("LM7 Praxisraum 1", renamed.Metadata.Name);
        Assert.Equal(workspace.UserDevice.Manufacturer, renamed.Manufacturer);
        Assert.Equal(workspace.UserDevice.Model, renamed.Model);
        Assert.Equal(workspace.UserDevice.DeviceType, renamed.DeviceType);
        Assert.Equal(workspace.UserDevice.ParserMode, renamed.ParserMode);
        Assert.Equal(workspace.UserDevice.Measurements, renamed.Measurements);
    }

    [Fact]
    public void Rename_ShouldRenameUserDefinedExportProfileAndKeepRulesAndReferences()
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Rename(
            workspace.Catalog,
            workspace.Paths,
            UserDefinedProfileRenameKind.ExportProfile,
            workspace.UserExport.Metadata.Id,
            "LM7 Export Praxisraum 1");

        var loaded = LoadCatalog(workspace);
        var renamed = loaded.ExportProfiles.Single(profile => profile.Metadata.Id == workspace.UserExport.Metadata.Id);
        var interfaceProfile = loaded.InterfaceProfiles.Single(profile => profile.Metadata.Id == workspace.UserInterface.Metadata.Id);
        Assert.True(result.Success);
        Assert.Equal(workspace.UserExport.Metadata.Id, renamed.Metadata.Id);
        Assert.Equal("LM7 Export Praxisraum 1", renamed.Metadata.Name);
        Assert.Equal(workspace.UserExport.TargetAisProfileId, renamed.TargetAisProfileId);
        Assert.Equal(workspace.UserExport.SourceDeviceProfileId, renamed.SourceDeviceProfileId);
        Assert.Equal(workspace.UserExport.OutputEncoding, renamed.OutputEncoding);
        Assert.Equal(workspace.UserExport.Rules, renamed.Rules);
        Assert.Equal(workspace.UserExport.Metadata.Id, interfaceProfile.ExportProfileId);
    }

    [Fact]
    public void Rename_ShouldRenameUserDefinedInterfaceProfileAndKeepFoldersAndReferences()
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Rename(
            workspace.Catalog,
            workspace.Paths,
            UserDefinedProfileRenameKind.InterfaceProfile,
            workspace.UserInterface.Metadata.Id,
            "MEDISTAR + LM7 Praxisraum 1");

        var renamed = LoadCatalog(workspace).InterfaceProfiles.Single(profile => profile.Metadata.Id == workspace.UserInterface.Metadata.Id);
        Assert.True(result.Success);
        Assert.Equal(workspace.UserInterface.Metadata.Id, renamed.Metadata.Id);
        Assert.Equal("MEDISTAR + LM7 Praxisraum 1", renamed.Metadata.Name);
        Assert.Equal(workspace.UserInterface.AisProfileId, renamed.AisProfileId);
        Assert.Equal(workspace.UserInterface.DeviceProfileId, renamed.DeviceProfileId);
        Assert.Equal(workspace.UserInterface.ExportProfileId, renamed.ExportProfileId);
        Assert.Equal(workspace.UserInterface.FolderOptions, renamed.FolderOptions);
        Assert.Equal(workspace.UserInterface.IsActive, renamed.IsActive);
        Assert.Equal(workspace.UserInterface.IsLicenseRequired, renamed.IsLicenseRequired);
    }

    [Theory]
    [InlineData(UserDefinedProfileRenameKind.AisProfile, "ais-medistar-default")]
    [InlineData(UserDefinedProfileRenameKind.DeviceProfile, "device-nidek-lm7-default")]
    [InlineData(UserDefinedProfileRenameKind.ExportProfile, "export-medistar-nidek-lm7-default")]
    [InlineData(UserDefinedProfileRenameKind.InterfaceProfile, "interface-medistar-nidek-lm7-default")]
    public void Evaluate_ShouldBlockBuiltInProfiles(
        UserDefinedProfileRenameKind kind,
        string profileId)
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Evaluate(workspace.Catalog, kind, profileId, "Neuer Name");

        Assert.False(result.Success);
        Assert.Contains("BuiltIn-Profile können nicht umbenannt werden.", result.Issues);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Evaluate_ShouldBlockEmptyOrWhitespaceName(string newName)
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Evaluate(
            workspace.Catalog,
            UserDefinedProfileRenameKind.ExportProfile,
            workspace.UserExport.Metadata.Id,
            newName);

        Assert.False(result.Success);
        Assert.Contains("Bitte geben Sie einen neuen Namen ein.", result.Issues);
    }

    [Fact]
    public void Evaluate_ShouldBlockNameConflictWithinSameProfileKind()
    {
        var workspace = CreateWorkspace(includeSecondUserExportProfile: true);

        var result = _renameService.Evaluate(
            workspace.Catalog,
            UserDefinedProfileRenameKind.ExportProfile,
            workspace.UserExport.Metadata.Id,
            "Zweiter LM7 Export");

        Assert.False(result.Success);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen.", result.Issues);
    }

    [Fact]
    public void Evaluate_ShouldAllowSameVisibleNameInDifferentProfileKind()
    {
        var workspace = CreateWorkspace();

        var result = _renameService.Evaluate(
            workspace.Catalog,
            UserDefinedProfileRenameKind.AisProfile,
            workspace.UserAis.Metadata.Id,
            workspace.UserDevice.Metadata.Name);

        Assert.True(result.Success);
        Assert.False(result.NoChange);
    }

    [Fact]
    public void Rename_ShouldKeepInterfaceReferencesWhenReferencedExportProfileIsRenamed()
    {
        var workspace = CreateWorkspace();

        _renameService.Rename(
            workspace.Catalog,
            workspace.Paths,
            UserDefinedProfileRenameKind.ExportProfile,
            workspace.UserExport.Metadata.Id,
            "Neuer sichtbarer Exportname");

        var loadedInterface = LoadCatalog(workspace).InterfaceProfiles.Single(profile => profile.Metadata.Id == workspace.UserInterface.Metadata.Id);
        Assert.Equal(workspace.UserAis.Metadata.Id, loadedInterface.AisProfileId);
        Assert.Equal(workspace.UserDevice.Metadata.Id, loadedInterface.DeviceProfileId);
        Assert.Equal(workspace.UserExport.Metadata.Id, loadedInterface.ExportProfileId);
    }

    private ProfileCatalog LoadCatalog(TestWorkspace workspace)
    {
        return _catalogService.Load(workspace.Paths);
    }

    private TestWorkspace CreateWorkspace(bool includeSecondUserExportProfile = false)
    {
        var paths = CreateTempPaths();
        var builtInAis = DefaultAisProfiles.CreateMedistarDefault();
        var builtInDevice = DefaultDeviceProfileDefinitions.CreateNidekLm7Default();
        var builtInExport = DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default();
        var builtInInterface = DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default();

        var userAis = builtInAis with
        {
            Metadata = CreateUserDefinedMetadata(builtInAis.Metadata, "ais-praxis-lm7", "Praxis AIS LM7"),
            Name = "Praxis AIS LM7"
        };
        var userDevice = builtInDevice with
        {
            Metadata = CreateUserDefinedMetadata(builtInDevice.Metadata, "device-praxis-lm7", "Praxis NIDEK LM7")
        };
        var userExport = builtInExport with
        {
            Metadata = CreateUserDefinedMetadata(builtInExport.Metadata, "export-praxis-lm7", "Praxis LM7 Export"),
            TargetAisProfileId = userAis.Metadata.Id,
            SourceDeviceProfileId = userDevice.Metadata.Id
        };
        var userInterface = builtInInterface with
        {
            Metadata = CreateUserDefinedMetadata(builtInInterface.Metadata, "interface-praxis-lm7", "Praxis LM7 Schnittstelle"),
            AisProfileId = userAis.Metadata.Id,
            DeviceProfileId = userDevice.Metadata.Id,
            ExportProfileId = userExport.Metadata.Id,
            FolderOptions = CreateFolderOptions(),
            IsActive = true
        };

        var userExports = includeSecondUserExportProfile
            ? new[]
            {
                userExport,
                userExport with
                {
                    Metadata = CreateUserDefinedMetadata(userExport.Metadata, "export-praxis-lm7-2", "Zweiter LM7 Export")
                }
            }
            : new[] { userExport };

        foreach (var profile in new[] { userAis })
        {
            _catalogService.SaveNewAisProfile(paths, profile);
        }

        foreach (var profile in new[] { userDevice })
        {
            _catalogService.SaveNewDeviceProfileDefinition(paths, profile);
        }

        foreach (var profile in userExports)
        {
            _catalogService.SaveNewExportProfile(paths, profile);
        }

        foreach (var profile in new[] { userInterface })
        {
            _catalogService.SaveNewInterfaceProfileDefinition(paths, profile);
        }

        var catalog = new ProfileCatalog(
            AisProfiles: new[] { builtInAis, userAis },
            DeviceProfiles: new[] { builtInDevice, userDevice },
            ExportProfiles: new[] { builtInExport }.Concat(userExports).ToArray(),
            InterfaceProfiles: new[] { builtInInterface, userInterface });

        return new TestWorkspace(paths, catalog, userAis, userDevice, userExport, userInterface);
    }

    private static ProfileMetadata CreateUserDefinedMetadata(
        ProfileMetadata source,
        string id,
        string name)
    {
        return source with
        {
            Id = id,
            Name = name,
            IsBuiltIn = false,
            IsUserDefined = true
        };
    }

    private static InterfaceFolderOptions CreateFolderOptions()
    {
        return new InterfaceFolderOptions(
            AisImportFolder: @"C:\Praxis\AIS-In",
            DeviceImportFolder: @"C:\Praxis\LM7-In",
            ExportFolder: @"C:\Praxis\Export",
            ArchiveFolder: @"C:\Praxis\Archiv",
            ErrorFolder: @"C:\Praxis\Fehler",
            ClearAisImportFolderBeforeProcessing: true,
            ClearDeviceImportFolderBeforeProcessing: true,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: true,
            MoveFailedFilesToErrorFolder: true,
            ArchiveProcessedFileMode: ArchiveProcessedFileMode.Move,
            ArchiveRetentionDays: 14,
            AttachmentImportFolder: @"C:\Praxis\Anhang-In",
            AttachmentExportFolder: @"C:\Praxis\Anhang-Out",
            IsAttachmentProcessingEnabled: true,
            AttachmentRequirementMode: AttachmentRequirementMode.Optional);
    }

    private static AppDataPaths CreateTempPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private sealed record TestWorkspace(
        AppDataPaths Paths,
        ProfileCatalog Catalog,
        AisProfile UserAis,
        DeviceProfileDefinition UserDevice,
        ExportProfileDefinition UserExport,
        InterfaceProfileDefinition UserInterface);
}
