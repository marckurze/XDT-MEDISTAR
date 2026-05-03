using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileCatalogStartupInitializationTests
{
    [Fact]
    public void StartupInitialization_ShouldEnsureDefaultsAndLoadCatalog()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        var paths = new AppDataPathProvider().GetPaths(baseFolder);
        var catalogService = new ProfileCatalogService();

        catalogService.EnsureDefaultProfiles(paths);
        var catalog = catalogService.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Equal(6, catalog.DeviceProfiles.Count);
        Assert.Equal(6, catalog.ExportProfiles.Count);
        Assert.Single(catalog.InterfaceProfiles);
        Assert.Contains("XdtDeviceBridgeTests", paths.BaseFolder);
    }
}
