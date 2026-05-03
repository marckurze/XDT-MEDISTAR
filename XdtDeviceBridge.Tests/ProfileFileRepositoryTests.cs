using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ProfileFileRepositoryTests
{
    private readonly ProfileFileRepository _repository = new();

    [Fact]
    public void SaveAndLoadAisProfile_ShouldRoundTrip()
    {
        var filePath = CreateTempFilePath("ais-profile.json");
        var profile = DefaultAisProfiles.CreateMedistarDefault();

        _repository.SaveAisProfile(filePath, profile);
        var loaded = _repository.LoadAisProfile(filePath);

        Assert.Equal("MEDISTAR", loaded.Name);
        Assert.Equal("Windows-1252", loaded.DefaultEncoding);
        Assert.Equal("6310", loaded.RequiredStaticFields["8000"]);
    }

    [Fact]
    public void SaveAndLoadDeviceProfileDefinition_ShouldRoundTrip()
    {
        var filePath = CreateTempFilePath("device-profile.json");
        var profile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault();

        _repository.SaveDeviceProfileDefinition(filePath, profile);
        var loaded = _repository.LoadDeviceProfileDefinition(filePath);

        Assert.Equal("NIDEK", loaded.Manufacturer);
        Assert.Equal("ARK1S", loaded.Model);
        Assert.Contains(loaded.Measurements, measurement =>
            measurement.Id == "far-pd"
            && measurement.SourcePath == "PD/PDList[@No='1']/FarPD");
    }

    [Fact]
    public void SaveAndLoadExportProfileDefinition_ShouldRoundTrip()
    {
        var filePath = CreateTempFilePath("export-profile.json");
        var profile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();

        _repository.SaveExportProfileDefinition(filePath, profile);
        var loaded = _repository.LoadExportProfileDefinition(filePath);

        Assert.Equal("Windows-1252", loaded.OutputEncoding);
        Assert.Equal(profile.Rules.Count, loaded.Rules.Count);
        Assert.Equal(2, loaded.Rules.Count(rule => rule.TargetFieldCode == "6228"));
    }

    [Fact]
    public void SaveAndLoadInterfaceProfileDefinition_ShouldRoundTrip()
    {
        var filePath = CreateTempFilePath("interface-profile.json");
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();

        _repository.SaveInterfaceProfileDefinition(filePath, profile);
        var loaded = _repository.LoadInterfaceProfileDefinition(filePath);

        Assert.False(loaded.IsActive);
        Assert.True(loaded.IsLicenseRequired);
        Assert.Equal(profile.FolderOptions, loaded.FolderOptions);
    }

    [Fact]
    public void SaveAndLoadTemplatePackage_ShouldRoundTrip()
    {
        var filePath = CreateTempFilePath("template-package.json");
        var package = CreateTemplatePackage();

        _repository.SaveTemplatePackage(filePath, package);
        var loaded = _repository.LoadTemplatePackage(filePath);

        Assert.Equal(package.Metadata.Id, loaded.Metadata.Id);
        Assert.Equal("1.0", loaded.PackageFormatVersion);
        Assert.Equal(package.IncludedProfiles.Count, loaded.IncludedProfiles.Count);
    }

    [Fact]
    public void Save_ShouldCreateTargetFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "nested");
        var filePath = Path.Combine(folder, "ais-profile.json");

        _repository.SaveAisProfile(filePath, DefaultAisProfiles.CreateMedistarDefault());

        Assert.True(Directory.Exists(folder));
        Assert.True(File.Exists(filePath));
    }

    [Fact]
    public void Save_ShouldThrowArgumentExceptionForEmptyFilePath()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            _repository.SaveAisProfile("", DefaultAisProfiles.CreateMedistarDefault()));

        Assert.Contains("File path must not be empty.", exception.Message);
    }

    [Fact]
    public void Load_ShouldThrowFileNotFoundExceptionForMissingFile()
    {
        var filePath = CreateTempFilePath("missing-profile.json");

        var exception = Assert.Throws<FileNotFoundException>(() => _repository.LoadAisProfile(filePath));

        Assert.Contains("Profile file not found:", exception.Message);
        Assert.Equal(filePath, exception.FileName);
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }

    private static TemplatePackage CreateTemplatePackage()
    {
        var timestamp = new DateTimeOffset(2026, 5, 3, 12, 0, 0, TimeSpan.Zero);
        var createdAt = new DateTime(2026, 5, 3, 12, 0, 0, DateTimeKind.Utc);

        return new TemplatePackage(
            Metadata: CreateMetadata("package-medistar-nidek-ark1s", "MEDISTAR + NIDEK ARK1S Package", ProfileKind.TemplatePackage, timestamp),
            IncludedProfiles: new[]
            {
                CreateMetadata("ais-medistar-default", "MEDISTAR", ProfileKind.AisProfile, timestamp),
                CreateMetadata("device-nidek-ark1s-default", "NIDEK ARK1S", ProfileKind.DeviceProfile, timestamp),
                CreateMetadata("export-medistar-nidek-ark1s-default", "MEDISTAR + NIDEK ARK1S Export", ProfileKind.ExportProfile, timestamp)
            },
            PackageFormatVersion: "1.0",
            CreatedAt: createdAt,
            CreatedBy: "XdtDeviceBridge",
            Description: "Repository roundtrip test package.");
    }

    private static ProfileMetadata CreateMetadata(
        string id,
        string name,
        ProfileKind profileKind,
        DateTimeOffset timestamp)
    {
        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: profileKind,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: true,
            IsUserDefined: false);
    }
}
