using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicensedDeviceGracePeriodRepositoryTests
{
    private static readonly DateTime StartedAtUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly LicensedDeviceGracePeriodRepository _repository = new();

    [Fact]
    public void SaveAndLoad_ShouldRoundTripGracePeriodStore()
    {
        var filePath = CreateTempFilePath("device-grace-periods.json");
        var store = new LicensedDeviceGracePeriodStore(new[]
        {
            CreateGracePeriod("interface-1"),
            CreateGracePeriod("interface-2")
        });

        _repository.Save(filePath, store);
        var loaded = _repository.Load(filePath);

        Assert.Equal(2, loaded.GracePeriods.Count);
        Assert.Contains(loaded.GracePeriods, gracePeriod => gracePeriod.InterfaceProfileId == "interface-1");
        Assert.Contains(loaded.GracePeriods, gracePeriod => gracePeriod.InterfaceProfileId == "interface-2");
    }

    [Fact]
    public void Save_ShouldCreateTargetFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "licenses");
        var filePath = Path.Combine(folder, "device-grace-periods.json");

        _repository.Save(filePath, new LicensedDeviceGracePeriodStore(new[] { CreateGracePeriod("interface-1") }));

        Assert.True(Directory.Exists(folder));
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void LoadOrEmpty_ShouldReturnEmptyStoreForMissingFile()
    {
        var filePath = CreateTempFilePath("missing-device-grace-periods.json");

        var store = _repository.LoadOrEmpty(filePath);

        Assert.Empty(store.GracePeriods);
    }

    [Fact]
    public void Load_ShouldThrowFileNotFoundExceptionForMissingFile()
    {
        var filePath = CreateTempFilePath("missing-device-grace-periods.json");

        var exception = Assert.Throws<FileNotFoundException>(() => _repository.Load(filePath));

        Assert.Contains("Licensed device grace period file not found:", exception.Message);
        Assert.Equal(filePath, exception.FileName);
    }

    [Fact]
    public void Load_ShouldThrowInvalidOperationExceptionForInvalidJson()
    {
        var filePath = CreateTempFilePath("invalid-device-grace-periods.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ invalid json");

        var exception = Assert.Throws<InvalidOperationException>(() => _repository.Load(filePath));

        Assert.Contains("Invalid licensed device grace period JSON:", exception.Message);
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }

    private static LicensedDeviceGracePeriod CreateGracePeriod(string interfaceProfileId)
    {
        return new LicensedDeviceGracePeriod(
            InterfaceProfileId: interfaceProfileId,
            StartedAtUtc: StartedAtUtc,
            EndsAtUtc: StartedAtUtc.AddDays(30),
            Reason: "Test");
    }
}
