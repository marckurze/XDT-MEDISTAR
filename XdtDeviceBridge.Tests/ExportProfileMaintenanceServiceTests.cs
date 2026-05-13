using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class ExportProfileMaintenanceServiceTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 5, 13, 12, 0, 0, TimeSpan.Zero);

    private readonly ProfileCatalogService _catalogService = new();
    private readonly ExportProfileDeletionService _deletionService;
    private readonly ExportRuleRemovalService _ruleRemovalService;

    public ExportProfileMaintenanceServiceTests()
    {
        _deletionService = new ExportProfileDeletionService(_catalogService);
        _ruleRemovalService = new ExportRuleRemovalService(_catalogService);
    }

    [Fact]
    public void DeleteExportProfile_ShouldBlockBuiltInProfile()
    {
        var catalog = CreateCatalog(
            exportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            interfaceProfiles: Array.Empty<InterfaceProfileDefinition>());

        var result = _deletionService.Evaluate(catalog, "export-medistar-nidek-ark1s-default");

        Assert.False(result.Success);
        Assert.Contains("BuiltIn-Exportprofile", result.Message);
    }

    [Fact]
    public void DeleteExportProfile_ShouldDeleteUnreferencedUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserExportProfile("export-user");
        _catalogService.SaveNewExportProfile(paths, profile);
        var catalog = _catalogService.Load(paths);
        var profilePath = Path.Combine(paths.ProfilesFolder, "exports", "export-user.json");
        var unrelatedOutputFile = Path.Combine(paths.BaseFolder, "export-output.xdt");
        Directory.CreateDirectory(paths.BaseFolder);
        File.WriteAllText(unrelatedOutputFile, "bestehende Exportdatei");

        var result = _deletionService.Delete(catalog, paths, "export-user");

        Assert.True(result.Success, result.Message);
        Assert.False(File.Exists(profilePath));
        Assert.True(File.Exists(unrelatedOutputFile));
        Assert.Empty(_catalogService.Load(paths).ExportProfiles);
    }

    [Fact]
    public void DeleteExportProfile_ShouldBlockReferencedUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var exportProfile = CreateUserExportProfile("export-user");
        var interfaceProfile = CreateUserInterfaceProfile("interface-user", exportProfile.Metadata.Id);
        _catalogService.Save(paths, CreateCatalog(
            exportProfiles: new[] { exportProfile },
            interfaceProfiles: new[] { interfaceProfile }));
        var catalog = _catalogService.Load(paths);

        var result = _deletionService.Delete(catalog, paths, "export-user");

        Assert.False(result.Success);
        Assert.Contains("wird noch von Schnittstellenprofilen verwendet", result.Message);
        Assert.Contains("User Interface", result.Message);
        Assert.True(File.Exists(Path.Combine(paths.ProfilesFolder, "exports", "export-user.json")));
        var loadedInterface = Assert.Single(_catalogService.Load(paths).InterfaceProfiles);
        Assert.Equal("export-user", loadedInterface.ExportProfileId);
    }

    [Fact]
    public void RemoveExportRule_ShouldRemoveRuleFromUserDefinedProfile()
    {
        var paths = CreateAppDataPaths();
        var profile = CreateUserExportProfile("export-user");
        _catalogService.SaveNewExportProfile(paths, profile);
        var catalog = _catalogService.Load(paths);
        var ruleToRemove = profile.Rules[0];

        var result = _ruleRemovalService.Remove(catalog, paths, "export-user", ruleToRemove.Id, Timestamp);

        Assert.True(result.Success, result.Message);
        var loadedProfile = Assert.Single(_catalogService.Load(paths).ExportProfiles);
        Assert.DoesNotContain(loadedProfile.Rules, rule => rule.Id == ruleToRemove.Id);
        Assert.Equal(profile.Rules.Count - 1, loadedProfile.Rules.Count);
        Assert.Equal(Timestamp, loadedProfile.Metadata.UpdatedAt);
    }

    [Fact]
    public void RemoveExportRule_ShouldBlockBuiltInProfile()
    {
        var builtInProfile = DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault();
        var catalog = CreateCatalog(
            exportProfiles: new[] { builtInProfile },
            interfaceProfiles: Array.Empty<InterfaceProfileDefinition>());

        var result = _ruleRemovalService.Evaluate(
            catalog,
            builtInProfile.Metadata.Id,
            builtInProfile.Rules[0].Id,
            Timestamp);

        Assert.False(result.Success);
        Assert.Contains("BuiltIn-Exportprofilen", result.Message);
    }

    [Fact]
    public void RemoveExportRule_ShouldSaveOnlyAffectedUserDefinedExportProfile()
    {
        var paths = CreateAppDataPaths();
        var firstProfile = CreateUserExportProfile("export-user-1");
        var secondProfile = CreateUserExportProfile("export-user-2") with
        {
            Metadata = CreateUserExportMetadata("export-user-2", "Second Export")
        };
        _catalogService.SaveNewExportProfile(paths, firstProfile);
        _catalogService.SaveNewExportProfile(paths, secondProfile);
        var secondProfilePath = Path.Combine(paths.ProfilesFolder, "exports", "export-user-2.json");
        var secondProfileBefore = File.ReadAllText(secondProfilePath);
        var catalog = _catalogService.Load(paths);

        var result = _ruleRemovalService.Remove(catalog, paths, "export-user-1", firstProfile.Rules[0].Id, Timestamp);

        Assert.True(result.Success, result.Message);
        Assert.Equal(secondProfileBefore, File.ReadAllText(secondProfilePath));
        var loadedProfiles = _catalogService.Load(paths).ExportProfiles;
        Assert.Contains(loadedProfiles, profile => profile.Metadata.Id == "export-user-2" && profile.Rules.Count == secondProfile.Rules.Count);
    }

    [Fact]
    public void RemoveExportRule_ShouldNotChangeReferencingInterfaceProfiles()
    {
        var paths = CreateAppDataPaths();
        var exportProfile = CreateUserExportProfile("export-user");
        var interfaceProfile = CreateUserInterfaceProfile("interface-user", exportProfile.Metadata.Id);
        _catalogService.Save(paths, CreateCatalog(
            exportProfiles: new[] { exportProfile },
            interfaceProfiles: new[] { interfaceProfile }));
        var interfacePath = Path.Combine(paths.ProfilesFolder, "interfaces", "interface-user.json");
        var interfaceBefore = File.ReadAllText(interfacePath);
        var catalog = _catalogService.Load(paths);

        var result = _ruleRemovalService.Remove(catalog, paths, "export-user", exportProfile.Rules[0].Id, Timestamp);

        Assert.True(result.Success, result.Message);
        Assert.Equal(interfaceBefore, File.ReadAllText(interfacePath));
        var loadedInterface = Assert.Single(_catalogService.Load(paths).InterfaceProfiles);
        Assert.Equal("export-user", loadedInterface.ExportProfileId);
    }

    private static AppDataPaths CreateAppDataPaths()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"));
        return new AppDataPathProvider().GetPaths(baseFolder);
    }

    private static ProfileCatalog CreateCatalog(
        IReadOnlyList<ExportProfileDefinition> exportProfiles,
        IReadOnlyList<InterfaceProfileDefinition> interfaceProfiles)
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() },
            ExportProfiles: exportProfiles,
            InterfaceProfiles: interfaceProfiles);
    }

    private static ExportProfileDefinition CreateUserExportProfile(string id)
    {
        return DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateUserExportMetadata(id, "User Export")
        };
    }

    private static InterfaceProfileDefinition CreateUserInterfaceProfile(string id, string exportProfileId)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = new ProfileMetadata(
                Id: id,
                Name: "User Interface",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: null,
                Vendor: null,
                Product: null,
                Version: "1.0",
                CreatedAt: Timestamp,
                UpdatedAt: Timestamp,
                CreatedBy: "TestUser",
                IsBuiltIn: false,
                IsUserDefined: true),
            ExportProfileId = exportProfileId
        };
    }

    private static ProfileMetadata CreateUserExportMetadata(string id, string name)
    {
        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: ProfileKind.ExportProfile,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0",
            CreatedAt: Timestamp,
            UpdatedAt: Timestamp,
            CreatedBy: "TestUser",
            IsBuiltIn: false,
            IsUserDefined: true);
    }
}
