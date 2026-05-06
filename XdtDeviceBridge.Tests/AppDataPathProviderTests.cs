using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class AppDataPathProviderTests
{
    private readonly AppDataPathProvider _provider = new();

    [Fact]
    public void GetPaths_ShouldCreateExpectedSubFolderPaths()
    {
        var baseFolder = CreateBaseFolderPath();

        var paths = _provider.GetPaths(baseFolder);

        Assert.Equal(Path.Combine(baseFolder, "profiles"), paths.ProfilesFolder);
        Assert.Equal(Path.Combine(baseFolder, "templates"), paths.TemplatesFolder);
        Assert.Equal(Path.Combine(baseFolder, "licenses"), paths.LicensesFolder);
        Assert.Equal(Path.Combine(baseFolder, "logs"), paths.LogsFolder);
        Assert.Equal(Path.Combine(baseFolder, "license-requests"), paths.LicenseRequestsFolder);
        Assert.Equal(Path.Combine(baseFolder, "template-packages"), paths.TemplatePackagesFolder);
    }

    [Fact]
    public void GetPaths_ShouldThrowArgumentExceptionForEmptyBaseFolder()
    {
        var exception = Assert.Throws<ArgumentException>(() => _provider.GetPaths(""));

        Assert.Contains("Base folder must not be empty.", exception.Message);
    }

    [Fact]
    public void InstallationInfoFile_ShouldEndWithInstallationJson()
    {
        var paths = _provider.GetPaths(CreateBaseFolderPath());

        Assert.EndsWith("installation.json", paths.InstallationInfoFile);
    }

    [Fact]
    public void LicenseFile_ShouldEndWithLicensesLicenseJson()
    {
        var paths = _provider.GetPaths(CreateBaseFolderPath());
        var expectedSuffix = Path.Combine("licenses", "license.json");

        Assert.EndsWith(expectedSuffix, paths.LicenseFile);
    }

    [Fact]
    public void DeviceGracePeriodsFile_ShouldEndWithLicensesDeviceGracePeriodsJson()
    {
        var paths = _provider.GetPaths(CreateBaseFolderPath());
        var expectedSuffix = Path.Combine("licenses", "device-grace-periods.json");

        Assert.EndsWith(expectedSuffix, paths.DeviceGracePeriodsFile);
    }

    [Fact]
    public void GetDefaultUserPaths_ShouldReturnNonEmptyBaseFolder()
    {
        var paths = _provider.GetDefaultUserPaths();

        Assert.False(string.IsNullOrWhiteSpace(paths.BaseFolder));
    }

    [Fact]
    public void GetDefaultUserPaths_ShouldUseXdtDeviceBridgeBaseFolder()
    {
        var paths = _provider.GetDefaultUserPaths();

        Assert.Contains("XdtDeviceBridge", paths.BaseFolder);
    }

    private static string CreateBaseFolderPath()
    {
        return Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
    }
}
