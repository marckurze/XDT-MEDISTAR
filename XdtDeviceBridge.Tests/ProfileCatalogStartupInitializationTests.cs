using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileCatalogStartupInitializationTests
{
    private const int ExpectedBuiltInProfileCount = 40;

    [Fact]
    public void StartupInitialization_ShouldEnsureDefaultsAndLoadCatalog()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        var paths = new AppDataPathProvider().GetPaths(baseFolder);
        var catalogService = new ProfileCatalogService();

        catalogService.EnsureDefaultProfiles(paths);
        var catalog = catalogService.Load(paths);

        Assert.Single(catalog.AisProfiles);
        Assert.Equal(ExpectedBuiltInProfileCount, catalog.DeviceProfiles.Count);
        Assert.Equal(ExpectedBuiltInProfileCount, catalog.ExportProfiles.Count);
        Assert.Equal(ExpectedBuiltInProfileCount, catalog.InterfaceProfiles.Count);
        Assert.Contains("XdtDeviceBridgeTests", paths.BaseFolder);
    }
}
