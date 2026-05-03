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
        Assert.Single(catalog.DeviceProfiles);
        Assert.Single(catalog.ExportProfiles);
        Assert.Single(catalog.InterfaceProfiles);
        Assert.Equal("ais-medistar-default", catalog.AisProfiles[0].Metadata.Id);
        Assert.Equal("device-nidek-ark1s-default", catalog.DeviceProfiles[0].Metadata.Id);
        Assert.Equal("export-medistar-nidek-ark1s-default", catalog.ExportProfiles[0].Metadata.Id);
        Assert.Equal("interface-medistar-nidek-ark1s-default", catalog.InterfaceProfiles[0].Metadata.Id);
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
            DeviceProfiles: Array.Empty<DeviceProfileDefinition>(),
            ExportProfiles: Array.Empty<ExportProfileDefinition>(),
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>()));

        _service.EnsureDefaultProfiles(paths);
        var catalog = _service.Load(paths);

        Assert.Contains(catalog.AisProfiles, profile => profile.Metadata.Id == "ais-medistar-default" && profile.Name == "Custom MEDISTAR");
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
}
