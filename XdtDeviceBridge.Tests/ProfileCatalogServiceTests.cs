using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileCatalogServiceTests
{
    private readonly ProfileCatalogService _service = new();

    [Fact]
    public void Load_ShouldReturnEmptyListsWhenFoldersAreMissing()
    {
        var paths = CreateAppDataPaths();

        var catalog = _service.Load(paths);

        Assert.Empty(catalog.AisProfiles);
        Assert.Empty(catalog.DeviceProfiles);
        Assert.Empty(catalog.ExportProfiles);
        Assert.Empty(catalog.InterfaceProfiles);
    }

    [Fact]
    public void Save_ShouldStoreAllProfileTypes()
    {
        var paths = CreateAppDataPaths();

        _service.Save(paths, CreateDefaultCatalog());

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "ais", "ais-medistar-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "devices", "device-nidek-ark1s-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "exports", "export-medistar-nidek-ark1s-default.json")));
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-medistar-nidek-ark1s-default.json")));
    }

    [Fact]
    public void Load_ShouldReadSavedProfiles()
    {
        var paths = CreateAppDataPaths();
        _service.Save(paths, CreateDefaultCatalog());

        var catalog = _service.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Single(catalog.DeviceProfiles);
        Assert.Single(catalog.ExportProfiles);
        Assert.Single(catalog.InterfaceProfiles);
        Assert.Equal("MEDISTAR", catalog.AisProfiles[0].Name);
        Assert.Equal("NIDEK", catalog.DeviceProfiles[0].Manufacturer);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldCreateDefaultProfiles()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Equal(6, catalog.DeviceProfiles.Count);
        Assert.Equal(6, catalog.ExportProfiles.Count);
        Assert.Single(catalog.InterfaceProfiles);
        Assert.Equal("ais-medistar-default", catalog.AisProfiles[0].Metadata.Id);
        AssertExpectedDeviceDefaults(catalog);
        AssertExpectedExportDefaults(catalog);
        Assert.Equal("interface-medistar-nidek-ark1s-default", catalog.InterfaceProfiles[0].Metadata.Id);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldCreateAllExpectedDeviceProfileDefinitions()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        AssertExpectedDeviceDefaults(catalog);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldCreateAllExpectedExportProfileDefinitions()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        AssertExpectedExportDefaults(catalog);
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldKeepMedistarAisProfileAvailable()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Contains(catalog.AisProfiles, profile =>
            profile.Metadata.Id == "ais-medistar-default"
            && profile.Name == "MEDISTAR");
    }

    [Fact]
    public void EnsureDefaultProfiles_ShouldNotOverwriteExistingProfiles()
    {
        var paths = CreateAppDataPaths();
        var customAisProfile = DefaultAisProfiles.CreateMedistarDefault() with
        {
            Name = "Custom MEDISTAR"
        };
        _service.Save(paths, new ProfileCatalog(
            AisProfiles: new[] { customAisProfile },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault() with { DeviceType = "Custom Tonometer/Pachymeter" } },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault() with { OutputEncoding = "Custom-Encoding" } },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Contains(catalog.AisProfiles, profile => profile.Metadata.Id == "ais-medistar-default" && profile.Name == "Custom MEDISTAR");
        Assert.Contains(catalog.DeviceProfiles, profile => profile.Metadata.Id == "device-topcon-trk2p-default" && profile.DeviceType == "Custom Tonometer/Pachymeter");
        Assert.Contains(catalog.ExportProfiles, profile => profile.Metadata.Id == "export-medistar-topcon-trk2p-default" && profile.OutputEncoding == "Custom-Encoding");
        Assert.Equal(6, catalog.DeviceProfiles.Count);
        Assert.Equal(6, catalog.ExportProfiles.Count);
    }

    [Fact]
    public void Load_ShouldReadAllExpectedProfilesAfterEnsureDefaultProfiles()
    {
        var paths = CreateAppDataPaths();

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Equal(6, catalog.DeviceProfiles.Count);
        Assert.Equal(6, catalog.ExportProfiles.Count);
        Assert.Single(catalog.InterfaceProfiles);
        AssertExpectedDeviceDefaults(catalog);
        AssertExpectedExportDefaults(catalog);
    }

    [Fact]
    public void Load_ShouldThrowInvalidOperationExceptionForInvalidJson()
    {
        var paths = CreateAppDataPaths();
        var aisFolder = Path.Combine(paths.ProfilesFolder, "ais");
        Directory.CreateDirectory(aisFolder);
        File.WriteAllText(Path.Combine(aisFolder, "invalid.json"), "{ invalid json");

        var exception = Assert.Throws<InvalidOperationException>(() => _service.Load(paths));

        Assert.Contains("Invalid profile JSON file", exception.Message);
    }

    [Fact]
    public void Save_ShouldWriteProfilesToExpectedSubFolders()
    {
        var paths = CreateAppDataPaths();

        _service.Save(paths, CreateDefaultCatalog());

        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "ais")));
        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "devices")));
        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "exports")));
        Assert.True(Directory.Exists(Path.Combine(paths.ProfilesFolder, "interfaces")));
    }

    [Fact]
    public void SaveNewExportProfile_ShouldWriteExportProfileWithoutCatalogSave()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserExportMetadata("export-user-copy")
        };

        _service.SaveNewExportProfile(paths, profile);

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "exports", "export-user-copy.json")));
        var catalog = _service.Load(paths);
        var loadedProfile = Assert.Single(catalog.ExportProfiles);
        Assert.Equal("export-user-copy", loadedProfile.Metadata.Id);
        Assert.True(loadedProfile.Metadata.IsUserDefined);
    }

    [Fact]
    public void SaveNewExportProfile_ShouldNotOverwriteExistingExportProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserExportMetadata("export-user-copy")
        };
        _service.SaveNewExportProfile(paths, profile);

        var exception = Assert.Throws<InvalidOperationException>(() => _service.SaveNewExportProfile(paths, profile));

        Assert.Contains("will not be overwritten", exception.Message);
    }

    [Fact]
    public void SaveInterfaceProfileDefinition_ShouldWriteUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-user")
        };

        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "interfaces", "interface-user.json")));
        var catalog = _service.Load(paths);
        var loadedProfile = Assert.Single(catalog.InterfaceProfiles);
        Assert.Equal("interface-user", loadedProfile.Metadata.Id);
        Assert.True(loadedProfile.Metadata.IsUserDefined);
    }

    [Fact]
    public void SaveInterfaceProfileDefinition_ShouldAllowOverwriteWhenRequested()
    {
        var paths = CreateAppDataPaths();
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserInterfaceMetadata("interface-user")
        };
        _service.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        var updatedProfile = profile with { IsActive = true };
        _service.SaveInterfaceProfileDefinition(paths, updatedProfile, overwriteExisting: true);

        var loadedProfile = Assert.Single(_service.Load(paths).InterfaceProfiles);
        Assert.True(loadedProfile.IsActive);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static ProfileCatalog CreateDefaultCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            InterfaceProfiles: new[] { DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() });
    }

    private static void AssertExpectedDeviceDefaults(ProfileCatalog catalog)
    {
        var ids = catalog.DeviceProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("device-nidek-ark1s-default", ids);
        Assert.Contains("device-nidek-lm7-default", ids);
        Assert.Contains("device-nidek-nt530p-default", ids);
        Assert.Contains("device-topcon-cl300-default", ids);
        Assert.Contains("device-topcon-kr800-default", ids);
        Assert.Contains("device-topcon-trk2p-default", ids);
    }

    private static void AssertExpectedExportDefaults(ProfileCatalog catalog)
    {
        var ids = catalog.ExportProfiles.Select(profile => profile.Metadata.Id).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("export-medistar-nidek-ark1s-default", ids);
        Assert.Contains("export-medistar-nidek-lm7-default", ids);
        Assert.Contains("export-medistar-nidek-nt530p-default", ids);
        Assert.Contains("export-medistar-topcon-cl300-default", ids);
        Assert.Contains("export-medistar-topcon-kr800-default", ids);
        Assert.Contains("export-medistar-topcon-trk2p-default", ids);
    }

    private static ProfileMetadata CreateUserExportMetadata(string id)
    {
        var timestamp = new DateTimeOffset(2026, 5, 5, 12, 0, 0, TimeSpan.Zero);
        return new ProfileMetadata(
            Id: id,
            Name: "User Export",
            ProfileKind: ProfileKind.ExportProfile,
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

    private static ProfileMetadata CreateUserInterfaceMetadata(string id)
    {
        var timestamp = new DateTimeOffset(2026, 5, 5, 12, 0, 0, TimeSpan.Zero);
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
