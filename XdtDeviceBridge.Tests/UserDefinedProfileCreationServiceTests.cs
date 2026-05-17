using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class UserDefinedProfileCreationServiceTests
{
    private readonly UserDefinedProfileCreationService _service = new();
    private readonly DateTimeOffset _timestamp = new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateAisProfile_ShouldCreateUserDefinedMedistarBasedProfile()
    {
        var catalog = CreateDefaultCatalog();

        var result = _service.CreateAisProfile(
            catalog,
            new UserDefinedAisProfileCreationRequest("Praxis AIS", "MEDISTAR", "Windows-1252"),
            _timestamp,
            "Tester");

        Assert.True(result.Success);
        var profile = result.Profile!;
        Assert.Equal("ais-praxis-ais", profile.Metadata.Id);
        Assert.Equal("Praxis AIS", profile.Metadata.Name);
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Equal(ProfileKind.AisProfile, profile.Metadata.ProfileKind);
        Assert.Contains("6228", profile.SupportedOutputFieldCodes);
        Assert.Contains("8402", profile.SupportedOutputFieldCodes);
        Assert.True(profile.SupportsResultTextField6228);
        Assert.True(profile.RequiresExaminationType8402);
    }

    [Fact]
    public void CreateAisProfile_ShouldRejectEmptyName()
    {
        var result = _service.CreateAisProfile(
            CreateDefaultCatalog(),
            new UserDefinedAisProfileCreationRequest("  ", "MEDISTAR", "Windows-1252"),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Bitte geben Sie einen Profilnamen ein.", result.Issues);
    }

    [Fact]
    public void CreateAisProfile_ShouldRejectNameOrIdConflictWithBuiltInProfile()
    {
        var catalog = CreateDefaultCatalog();

        var nameConflict = _service.CreateAisProfile(
            catalog,
            new UserDefinedAisProfileCreationRequest("MEDISTAR", "MEDISTAR", "Windows-1252"),
            _timestamp,
            "Tester");
        var idConflict = _service.CreateAisProfile(
            catalog,
            new UserDefinedAisProfileCreationRequest("Praxis AIS", "MEDISTAR", "Windows-1252"),
            _timestamp,
            "Tester",
            idFactory: () => "ais-medistar-default");

        Assert.False(nameConflict.Success);
        Assert.False(idConflict.Success);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.", nameConflict.Issues);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.", idConflict.Issues);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldCreateUserDefinedDeviceProfile()
    {
        var catalog = CreateDefaultCatalog();

        var result = _service.CreateDeviceProfile(
            catalog,
            new UserDefinedDeviceProfileCreationRequest("Praxis Gerät", "NIDEK", "Testmodell", "Autorefractor", "Xml"),
            _timestamp,
            "Tester");

        Assert.True(result.Success);
        var profile = result.Profile!;
        Assert.Equal("device-praxis-gerat", profile.Metadata.Id);
        Assert.Equal("Praxis Gerät", profile.Metadata.Name);
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Equal("NIDEK", profile.Manufacturer);
        Assert.Equal("Testmodell", profile.Model);
        Assert.Equal("Autorefractor", profile.DeviceType);
        Assert.Equal("Xml", profile.ParserMode);
        Assert.Empty(profile.Measurements);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldRejectMissingRequiredFields()
    {
        var result = _service.CreateDeviceProfile(
            CreateDefaultCatalog(),
            new UserDefinedDeviceProfileCreationRequest("Praxis Gerät", "", " ", "Generisch", ""),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Bitte geben Sie einen Hersteller ein.", result.Issues);
        Assert.Contains("Bitte geben Sie ein Modell ein.", result.Issues);
        Assert.Contains("Bitte wählen Sie eine Parser-/Formatbasis aus.", result.Issues);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldRejectBuiltInConflict()
    {
        var result = _service.CreateDeviceProfile(
            CreateDefaultCatalog(),
            new UserDefinedDeviceProfileCreationRequest("NIDEK ARK1S", "NIDEK", "ARK1S", "Autorefractor", "Xml"),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.", result.Issues);
    }

    [Fact]
    public void CreateExportProfile_ShouldCreateUserDefinedProfileAndKeepRules()
    {
        var catalog = CreateDefaultCatalog();
        var sourceExportProfile = catalog.ExportProfiles.Single();
        var sourceInterfaceProfiles = catalog.InterfaceProfiles.ToArray();

        var result = _service.CreateExportProfile(
            catalog,
            new UserDefinedExportProfileCreationRequest(
                "Praxis Export",
                sourceExportProfile.TargetAisProfileId,
                sourceExportProfile.SourceDeviceProfileId,
                sourceExportProfile.OutputEncoding,
                sourceExportProfile.Rules),
            _timestamp,
            "Tester");

        Assert.True(result.Success);
        var profile = result.Profile!;
        Assert.Equal("export-praxis-export", profile.Metadata.Id);
        Assert.Equal("Praxis Export", profile.Metadata.Name);
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Equal(sourceExportProfile.Rules.Count, profile.Rules.Count);
        Assert.Contains(profile.Rules, rule => rule.Id == sourceExportProfile.Rules[0].Id);
        Assert.True(sourceExportProfile.Metadata.IsBuiltIn);
        Assert.Equal(sourceInterfaceProfiles, catalog.InterfaceProfiles);
    }

    [Fact]
    public void CreateExportProfile_ShouldRejectEmptyName()
    {
        var catalog = CreateDefaultCatalog();
        var sourceExportProfile = catalog.ExportProfiles.Single();

        var result = _service.CreateExportProfile(
            catalog,
            new UserDefinedExportProfileCreationRequest(
                " ",
                sourceExportProfile.TargetAisProfileId,
                sourceExportProfile.SourceDeviceProfileId,
                sourceExportProfile.OutputEncoding,
                sourceExportProfile.Rules),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Bitte geben Sie einen Profilnamen ein.", result.Issues);
    }

    [Fact]
    public void CreateExportProfile_ShouldRejectNameConflict()
    {
        var catalog = CreateDefaultCatalog();
        var sourceExportProfile = catalog.ExportProfiles.Single();

        var result = _service.CreateExportProfile(
            catalog,
            new UserDefinedExportProfileCreationRequest(
                sourceExportProfile.Metadata.Name,
                sourceExportProfile.TargetAisProfileId,
                sourceExportProfile.SourceDeviceProfileId,
                sourceExportProfile.OutputEncoding,
                sourceExportProfile.Rules),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.", result.Issues);
    }

    [Fact]
    public void CreateExportProfile_ShouldRejectMissingReferences()
    {
        var result = _service.CreateExportProfile(
            CreateDefaultCatalog(),
            new UserDefinedExportProfileCreationRequest(
                "Praxis Export",
                "ais-missing",
                "device-missing",
                "Windows-1252",
                Array.Empty<ExportRuleDefinition>()),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Das ausgewählte AIS-Profil wurde nicht gefunden.", result.Issues);
        Assert.Contains("Das ausgewählte Geräteprofil wurde nicht gefunden.", result.Issues);
    }

    [Fact]
    public void CreateUniqueProfileId_ShouldCreateSuffixInsteadOfOverwritingExistingId()
    {
        var id = UserDefinedProfileCreationService.CreateUniqueProfileId(
            "device",
            "NIDEK ARK1S",
            new[] { "device-nidek-ark1s", "device-nidek-ark1s-2" });

        Assert.Equal("device-nidek-ark1s-3", id);
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
