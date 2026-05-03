using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseFileRepositoryTests
{
    private readonly LicenseFileRepository _repository = new();

    [Fact]
    public void SaveAndLoad_ShouldRoundTripLicenseInfo()
    {
        var filePath = CreateTempFilePath("license.json");
        var license = CreateLicenseInfo();

        _repository.Save(filePath, license);
        var loaded = _repository.Load(filePath);

        Assert.Equal(license.LicenseId, loaded.LicenseId);
        Assert.Equal(license.CustomerName, loaded.CustomerName);
        Assert.Equal(license.ProductCode, loaded.ProductCode);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveLicenseType()
    {
        var filePath = CreateTempFilePath("license.json");
        var license = CreateLicenseInfo() with { LicenseType = LicenseType.Annual };

        _repository.Save(filePath, license);
        var loaded = _repository.Load(filePath);

        Assert.Equal(LicenseType.Annual, loaded.LicenseType);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveLicensedDeviceCount()
    {
        var filePath = CreateTempFilePath("license.json");
        var license = CreateLicenseInfo() with { LicensedDeviceCount = 7 };

        _repository.Save(filePath, license);
        var loaded = _repository.Load(filePath);

        Assert.Equal(7, loaded.LicensedDeviceCount);
    }

    [Fact]
    public void SaveAndLoad_ShouldPreserveInstallationId()
    {
        var filePath = CreateTempFilePath("license.json");
        var license = CreateLicenseInfo() with { InstallationId = "installation-xyz" };

        _repository.Save(filePath, license);
        var loaded = _repository.Load(filePath);

        Assert.Equal("installation-xyz", loaded.InstallationId);
    }

    [Fact]
    public void Save_ShouldCreateTargetFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "nested");
        var filePath = Path.Combine(folder, "license.json");

        _repository.Save(filePath, CreateLicenseInfo());

        Assert.True(Directory.Exists(folder));
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Save_ShouldThrowArgumentExceptionForEmptyFilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() => _repository.Save("", CreateLicenseInfo()));

        Assert.Contains("File path must not be empty.", exception.Message);
    }

    [Fact]
    public void Load_ShouldThrowFileNotFoundExceptionForMissingFile()
    {
        var filePath = CreateTempFilePath("missing-license.json");

        var exception = Assert.Throws<FileNotFoundException>(() => _repository.Load(filePath));

        Assert.Contains("License file not found:", exception.Message);
        Assert.Equal(filePath, exception.FileName);
    }

    [Fact]
    public void Load_ShouldThrowInvalidOperationExceptionForInvalidJson()
    {
        var filePath = CreateTempFilePath("invalid-license.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ invalid json");

        var exception = Assert.Throws<InvalidOperationException>(() => _repository.Load(filePath));

        Assert.Contains("Invalid license JSON:", exception.Message);
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }

    private static LicenseInfo CreateLicenseInfo()
    {
        return new LicenseInfo(
            LicenseId: "license-1",
            CustomerName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            LicensedDeviceCount: 3,
            ValidFrom: new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            ValidUntil: new DateTime(2027, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            LicenseType: LicenseType.Monthly,
            ProductCode: "XDT-DEVICE-BRIDGE",
            MinimumAppVersion: "1.0.0",
            IssuedAt: new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            Signature: "signed-license");
    }
}
