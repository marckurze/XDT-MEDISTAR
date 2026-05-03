using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InstallationInfoProviderTests
{
    private readonly InstallationInfoProvider _provider = new();

    [Fact]
    public void GetOrCreate_ShouldCreateInstallationJson()
    {
        var storageFolder = CreateTempFolder();

        _provider.GetOrCreate(storageFolder);

        Assert.True(File.Exists(Path.Combine(storageFolder, "installation.json")));
    }

    [Fact]
    public void GetOrCreate_ShouldLoadSameInstallationIdOnRepeatedCall()
    {
        var storageFolder = CreateTempFolder();

        var first = _provider.GetOrCreate(storageFolder);
        var second = _provider.GetOrCreate(storageFolder);

        Assert.Equal(first.InstallationId, second.InstallationId);
    }

    [Fact]
    public void GetOrCreate_ShouldThrowArgumentExceptionForEmptyStorageFolder()
    {
        var exception = Assert.Throws<ArgumentException>(() => _provider.GetOrCreate(""));

        Assert.Contains("Storage folder must not be empty.", exception.Message);
    }

    [Fact]
    public void GetOrCreate_ShouldThrowInvalidOperationExceptionForInvalidJson()
    {
        var storageFolder = CreateTempFolder();
        Directory.CreateDirectory(storageFolder);
        File.WriteAllText(Path.Combine(storageFolder, "installation.json"), "{ invalid json");

        var exception = Assert.Throws<InvalidOperationException>(() => _provider.GetOrCreate(storageFolder));

        Assert.Contains("Invalid installation info JSON:", exception.Message);
    }

    [Fact]
    public void GetOrCreate_ShouldSetMachineName()
    {
        var installationInfo = _provider.GetOrCreate(CreateTempFolder());

        Assert.False(string.IsNullOrWhiteSpace(installationInfo.MachineName));
    }

    [Fact]
    public void GetOrCreate_ShouldSetUserName()
    {
        var installationInfo = _provider.GetOrCreate(CreateTempFolder());

        Assert.False(string.IsNullOrWhiteSpace(installationInfo.UserName));
    }

    [Fact]
    public void GetOrCreate_ShouldSetCreatedAt()
    {
        var installationInfo = _provider.GetOrCreate(CreateTempFolder());

        Assert.NotEqual(default, installationInfo.CreatedAt);
    }

    private static string CreateTempFolder()
    {
        return Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
    }
}
