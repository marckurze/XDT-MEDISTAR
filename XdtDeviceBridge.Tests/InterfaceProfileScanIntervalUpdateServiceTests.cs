using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileScanIntervalUpdateServiceTests
{
    private readonly InterfaceProfileScanIntervalUpdateService _service = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 10, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ChangeBy_ShouldDecreaseIntervalByOneSecond()
    {
        var profile = CreateUserDefinedProfile(intervalSeconds: 5);

        var result = _service.ChangeBy(profile, -1, _timestamp, "TestUser");

        Assert.True(result.Success);
        Assert.True(result.Changed);
        Assert.Equal(5, result.PreviousIntervalSeconds);
        Assert.Equal(4, result.EffectiveIntervalSeconds);
        Assert.Equal(4, result.Profile!.FolderOptions.AutoImportScanIntervalSeconds);
    }

    [Fact]
    public void ChangeBy_ShouldNotGoBelowMinimum()
    {
        var profile = CreateUserDefinedProfile(intervalSeconds: 1);

        var result = _service.ChangeBy(profile, -1, _timestamp, "TestUser");

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.True(result.ReachedMinimum);
        Assert.Equal(1, result.EffectiveIntervalSeconds);
        Assert.Contains("Minimum 1 Sekunde", result.Message);
    }

    [Fact]
    public void ChangeBy_ShouldIncreaseIntervalByOneSecond()
    {
        var profile = CreateUserDefinedProfile(intervalSeconds: 5);

        var result = _service.ChangeBy(profile, 1, _timestamp, "TestUser");

        Assert.True(result.Success);
        Assert.True(result.Changed);
        Assert.Equal(6, result.EffectiveIntervalSeconds);
        Assert.Equal(6, result.Profile!.FolderOptions.AutoImportScanIntervalSeconds);
    }

    [Fact]
    public void ChangeBy_ShouldNotGoAboveMaximum()
    {
        var profile = CreateUserDefinedProfile(intervalSeconds: InterfaceProfileScanIntervalUpdateService.MaximumScanIntervalSeconds);

        var result = _service.ChangeBy(profile, 1, _timestamp, "TestUser");

        Assert.True(result.Success);
        Assert.False(result.Changed);
        Assert.True(result.ReachedMaximum);
        Assert.Equal(InterfaceProfileScanIntervalUpdateService.MaximumScanIntervalSeconds, result.EffectiveIntervalSeconds);
        Assert.Contains("Maximum 300 Sekunden", result.Message);
    }

    [Fact]
    public void ChangeBy_ShouldUpdateUserDefinedProfileMetadata()
    {
        var profile = CreateUserDefinedProfile(intervalSeconds: 7);

        var result = _service.ChangeBy(profile, 1, _timestamp, "TestUser");

        Assert.True(result.Success);
        Assert.False(result.CreatedUserDefinedCopy);
        Assert.Equal(profile.Metadata.Id, result.Profile!.Metadata.Id);
        Assert.True(result.Profile.Metadata.IsUserDefined);
        Assert.False(result.Profile.Metadata.IsBuiltIn);
        Assert.Equal(_timestamp, result.Profile.Metadata.UpdatedAt);
        Assert.Equal(8, result.Profile.FolderOptions.AutoImportScanIntervalSeconds);
    }

    [Fact]
    public void ChangeBy_ShouldCreateUserDefinedCopyForBuiltInProfile()
    {
        var builtInProfile = CreateBuiltInProfile(intervalSeconds: 5);

        var result = _service.ChangeBy(
            builtInProfile,
            1,
            _timestamp,
            "TestUser",
            idFactory: () => "interface-built-in-copy");

        Assert.True(result.Success);
        Assert.True(result.CreatedUserDefinedCopy);
        Assert.Equal("interface-built-in-copy", result.Profile!.Metadata.Id);
        Assert.NotEqual(builtInProfile.Metadata.Id, result.Profile.Metadata.Id);
        Assert.True(result.Profile.Metadata.IsUserDefined);
        Assert.False(result.Profile.Metadata.IsBuiltIn);
        Assert.Equal(6, result.Profile.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.Equal(5, builtInProfile.FolderOptions.AutoImportScanIntervalSeconds);
    }

    [Fact]
    public void ChangeBy_ShouldPersistUpdatedUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var catalogService = new ProfileCatalogService();
        var profile = CreateUserDefinedProfile("interface-user-save", intervalSeconds: 5);
        catalogService.SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);

        var result = _service.ChangeBy(profile, 1, _timestamp, "TestUser");
        catalogService.SaveInterfaceProfileDefinition(paths, result.Profile!, overwriteExisting: true);

        var loaded = Assert.Single(catalogService.Load(paths).InterfaceProfiles);
        Assert.Equal("interface-user-save", loaded.Metadata.Id);
        Assert.Equal(6, loaded.FolderOptions.AutoImportScanIntervalSeconds);
    }

    [Fact]
    public void ChangeBy_ShouldPersistBuiltInChangeAsCopyWithoutOverwritingBuiltIn()
    {
        var paths = CreateAppDataPaths();
        var catalogService = new ProfileCatalogService();
        var builtInProfile = CreateBuiltInProfile(intervalSeconds: 5);
        catalogService.SaveInterfaceProfileDefinition(paths, builtInProfile, overwriteExisting: false);

        var result = _service.ChangeBy(
            builtInProfile,
            1,
            _timestamp,
            "TestUser",
            idFactory: () => "interface-built-in-save-copy");
        catalogService.SaveInterfaceProfileDefinition(paths, result.Profile!, overwriteExisting: false);

        var loaded = catalogService.Load(paths).InterfaceProfiles;
        var loadedBuiltIn = Assert.Single(loaded, profile => profile.Metadata.Id == builtInProfile.Metadata.Id);
        var loadedCopy = Assert.Single(loaded, profile => profile.Metadata.Id == "interface-built-in-save-copy");
        Assert.True(loadedBuiltIn.Metadata.IsBuiltIn);
        Assert.False(loadedBuiltIn.Metadata.IsUserDefined);
        Assert.Equal(5, loadedBuiltIn.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.False(loadedCopy.Metadata.IsBuiltIn);
        Assert.True(loadedCopy.Metadata.IsUserDefined);
        Assert.Equal(6, loadedCopy.FolderOptions.AutoImportScanIntervalSeconds);
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile(int intervalSeconds)
    {
        return CreateUserDefinedProfile("interface-user", intervalSeconds);
    }

    private static InterfaceProfileDefinition CreateUserDefinedProfile(string id, int intervalSeconds)
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        return profile with
        {
            Metadata = CreateMetadata(id, "UserDefined Schnittstelle", isBuiltIn: false),
            IsActive = true,
            FolderOptions = profile.FolderOptions with
            {
                AisImportFolder = @"\\SERVER\Freigabe\AIS",
                DeviceImportFolder = @"\\SERVER\Freigabe\Device",
                ExportFolder = @"\\SERVER\Freigabe\Export",
                ErrorFolder = @"\\SERVER\Freigabe\Fehler",
                AutoImportScanIntervalSeconds = intervalSeconds
            }
        };
    }

    private static InterfaceProfileDefinition CreateBuiltInProfile(int intervalSeconds)
    {
        var profile = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault();
        return profile with
        {
            IsActive = true,
            FolderOptions = profile.FolderOptions with
            {
                AisImportFolder = @"\\SERVER\Freigabe\AIS",
                DeviceImportFolder = @"\\SERVER\Freigabe\Device",
                ExportFolder = @"\\SERVER\Freigabe\Export",
                ErrorFolder = @"\\SERVER\Freigabe\Fehler",
                AutoImportScanIntervalSeconds = intervalSeconds
            }
        };
    }

    private static ProfileMetadata CreateMetadata(string id, string name, bool isBuiltIn)
    {
        var timestamp = new DateTimeOffset(2026, 5, 1, 12, 0, 0, TimeSpan.Zero);
        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: ProfileKind.InterfaceProfile,
            Description: null,
            Vendor: "Test",
            Product: "Test",
            Version: "1.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "Test",
            IsBuiltIn: isBuiltIn,
            IsUserDefined: !isBuiltIn);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPaths(
            BaseFolder: baseFolder,
            ProfilesFolder: Path.Combine(baseFolder, "profiles"),
            TemplatesFolder: Path.Combine(baseFolder, "templates"),
            LicensesFolder: Path.Combine(baseFolder, "licenses"),
            LogsFolder: Path.Combine(baseFolder, "logs"),
            InstallationInfoFile: Path.Combine(baseFolder, "installation.json"),
            LicenseFile: Path.Combine(baseFolder, "license.json"),
            DeviceGracePeriodsFile: Path.Combine(baseFolder, "device-grace-periods.json"),
            LicenseRequestsFolder: Path.Combine(baseFolder, "license-requests"),
            TemplatePackagesFolder: Path.Combine(baseFolder, "template-packages"));
    }
}
