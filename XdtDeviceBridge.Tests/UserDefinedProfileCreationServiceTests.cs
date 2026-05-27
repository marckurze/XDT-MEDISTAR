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
    public void CreateAisProfile_ShouldCreateGenericOtherAisProfileWithoutMedistarDefaults()
    {
        var result = _service.CreateAisProfile(
            CreateDefaultCatalog(),
            new UserDefinedAisProfileCreationRequest("Anderes AIS", "Generisch / anderes AIS", "UTF-8"),
            _timestamp,
            "Tester");

        Assert.True(result.Success);
        var profile = result.Profile!;
        Assert.Equal("Anderes AIS", profile.Metadata.Name);
        Assert.Equal("Generisch / anderes AIS", profile.Vendor);
        Assert.Equal("UTF-8", profile.DefaultEncoding);
        Assert.Empty(profile.RequiredStaticFields);
        Assert.Empty(profile.SupportedOutputFieldCodes);
        Assert.False(profile.SupportsResultTextField6228);
        Assert.False(profile.RequiresExaminationType8402);
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
        Assert.False(profile.IsBidirectional);
        Assert.Equal(string.Empty, profile.DeviceImagePath);
        Assert.Equal(DeviceConnectionKind.NetworkLan, profile.ConnectionKind);
        Assert.Null(profile.SerialSettings);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldPersistSerialRs232ConnectionKind()
    {
        var catalog = CreateDefaultCatalog();

        var result = _service.CreateDeviceProfile(
            catalog,
            new UserDefinedDeviceProfileCreationRequest(
                "RS232 Gerät",
                "NIDEK",
                "Altgerät",
                "Autorefractor",
                "Text",
                ConnectionKind: DeviceConnectionKind.SerialRs232),
            _timestamp,
            "Tester");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues));
        var profile = result.Profile!;
        Assert.Equal(DeviceConnectionKind.SerialRs232, profile.ConnectionKind);
        Assert.NotNull(profile.SerialSettings);
        Assert.Equal(9600, profile.SerialSettings!.BaudRate);
        Assert.Equal(8, profile.SerialSettings.DataBits);
        Assert.Equal(SerialStopBitsSetting.One, profile.SerialSettings.StopBits);
        Assert.Equal(SerialHandshakeSetting.None, profile.SerialSettings.Handshake);
        Assert.False(profile.SerialSettings.IsBidirectional);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldPersistOptionalDeviceImagePath()
    {
        var catalog = CreateDefaultCatalog();

        var result = _service.CreateDeviceProfile(
            catalog,
            new UserDefinedDeviceProfileCreationRequest(
                "Praxis Phoropter",
                "TOPCON",
                "CV-5000S",
                "Phoropter",
                "Xml",
                DeviceImagePath: @"C:\Praxis\Bilder\cv5000.png"),
            _timestamp,
            "Tester");

        Assert.True(result.Success);
        Assert.Equal(@"C:\Praxis\Bilder\cv5000.png", result.Profile!.DeviceImagePath);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldPersistBidirectionalCapabilityOnly()
    {
        var catalog = CreateDefaultCatalog();

        var result = _service.CreateDeviceProfile(
            catalog,
            new UserDefinedDeviceProfileCreationRequest(
                "CV5000 Raum 1",
                "TOPCON",
                "CV-5000S",
                "Phoropter",
                "Xml",
                IsBidirectional: true),
            _timestamp,
            "Tester");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues));
        var profile = result.Profile!;
        Assert.True(profile.IsBidirectional);
    }

    [Fact]
    public void CreateDeviceProfile_ShouldUseGenericDeviceTypeWhenFieldIsEmpty()
    {
        var result = _service.CreateDeviceProfile(
            CreateDefaultCatalog(),
            new UserDefinedDeviceProfileCreationRequest("Praxis Gerät", "NIDEK", "Testmodell", "", "Xml"),
            _timestamp,
            "Tester");

        Assert.True(result.Success);
        Assert.Equal("Generisch", result.Profile!.DeviceType);
        Assert.Equal("Xml", result.Profile.ParserMode);
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
    public void CreateInterfaceProfile_ShouldCreateInactiveUserDefinedProfile()
    {
        var catalog = CreateDefaultCatalog();
        var sourceInterfaces = catalog.InterfaceProfiles.ToArray();
        var result = _service.CreateInterfaceProfile(
            catalog,
            new UserDefinedInterfaceProfileCreationRequest(
                "Praxis Schnittstelle",
                "ais-medistar-default",
                "device-nidek-ark1s-default",
                "export-medistar-nidek-ark1s-default"),
            _timestamp,
            "Tester");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues));
        var profile = result.Profile!;
        Assert.Equal("interface-praxis-schnittstelle", profile.Metadata.Id);
        Assert.Equal("Praxis Schnittstelle", profile.Metadata.Name);
        Assert.False(profile.Metadata.IsBuiltIn);
        Assert.True(profile.Metadata.IsUserDefined);
        Assert.Equal(ProfileKind.InterfaceProfile, profile.Metadata.ProfileKind);
        Assert.Equal("ais-medistar-default", profile.AisProfileId);
        Assert.Equal("device-nidek-ark1s-default", profile.DeviceProfileId);
        Assert.Equal("export-medistar-nidek-ark1s-default", profile.ExportProfileId);
        Assert.False(profile.IsActive);
        Assert.True(profile.IsLicenseRequired);
        Assert.Equal(string.Empty, profile.FolderOptions.AisImportFolder);
        Assert.Equal(string.Empty, profile.FolderOptions.DeviceImportFolder);
        Assert.Equal(string.Empty, profile.FolderOptions.ExportFolder);
        Assert.Equal(5, profile.FolderOptions.AutoImportScanIntervalSeconds);
        Assert.Equal(10, profile.FolderOptions.DeviceFileWaitTimeoutMinutes);
        Assert.Equal(2, profile.FolderOptions.AttachmentFileStabilityWaitSeconds);
        Assert.Null(profile.DeviceOutput);
        Assert.Null(profile.SerialSettings);
        Assert.Equal(sourceInterfaces, catalog.InterfaceProfiles);
    }

    [Fact]
    public void CreateInterfaceProfile_ShouldPrepareSerialSettingsForRs232Device()
    {
        var serialDevice = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            ConnectionKind = DeviceConnectionKind.SerialRs232,
            SerialSettings = SerialCommunicationSettings.Default with
            {
                PortName = "COM7",
                BaudRate = 19200,
                IsBidirectional = true
            }
        };
        var catalog = new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { serialDevice },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            InterfaceProfiles: Array.Empty<InterfaceProfileDefinition>());

        var result = _service.CreateInterfaceProfile(
            catalog,
            new UserDefinedInterfaceProfileCreationRequest(
                "Praxis RS232",
                "ais-medistar-default",
                "device-nidek-ark1s-default",
                "export-medistar-nidek-ark1s-default"),
            _timestamp,
            "Tester");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues));
        var profile = result.Profile!;
        Assert.False(profile.IsActive);
        Assert.NotNull(profile.SerialSettings);
        Assert.Equal("COM7", profile.SerialSettings!.PortName);
        Assert.Equal(19200, profile.SerialSettings.BaudRate);
        Assert.True(profile.SerialSettings.IsBidirectional);
    }

    [Fact]
    public void CreateInterfaceProfile_ShouldRejectMissingRequiredFields()
    {
        var result = _service.CreateInterfaceProfile(
            CreateDefaultCatalog(),
            new UserDefinedInterfaceProfileCreationRequest(" ", "", "", ""),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Bitte geben Sie einen Profilnamen ein.", result.Issues);
        Assert.Contains("Bitte wählen Sie ein AIS-Profil aus.", result.Issues);
        Assert.Contains("Bitte wählen Sie ein Geräteprofil aus.", result.Issues);
        Assert.Contains("Bitte wählen Sie ein Exportprofil aus.", result.Issues);
    }

    [Fact]
    public void CreateInterfaceProfile_ShouldRejectDuplicateNameAndBuiltInId()
    {
        var catalog = CreateDefaultCatalog();
        var duplicateName = _service.CreateInterfaceProfile(
            catalog,
            new UserDefinedInterfaceProfileCreationRequest(
                "MEDISTAR + NIDEK ARK1S",
                "ais-medistar-default",
                "device-nidek-ark1s-default",
                "export-medistar-nidek-ark1s-default"),
            _timestamp,
            "Tester");
        var duplicateId = _service.CreateInterfaceProfile(
            catalog,
            new UserDefinedInterfaceProfileCreationRequest(
                "Praxis Schnittstelle",
                "ais-medistar-default",
                "device-nidek-ark1s-default",
                "export-medistar-nidek-ark1s-default"),
            _timestamp,
            "Tester",
            idFactory: () => "interface-medistar-nidek-ark1s-default");

        Assert.False(duplicateName.Success);
        Assert.False(duplicateId.Success);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.", duplicateName.Issues);
        Assert.Contains("Es existiert bereits ein Profil mit diesem Namen oder dieser ID.", duplicateId.Issues);
    }

    [Fact]
    public void CreateInterfaceProfile_ShouldRejectMissingReferences()
    {
        var result = _service.CreateInterfaceProfile(
            CreateDefaultCatalog(),
            new UserDefinedInterfaceProfileCreationRequest(
                "Praxis Schnittstelle",
                "ais-missing",
                "device-missing",
                "export-missing"),
            _timestamp,
            "Tester");

        Assert.False(result.Success);
        Assert.Contains("Das ausgewählte AIS-Profil wurde nicht gefunden.", result.Issues);
        Assert.Contains("Das ausgewählte Geräteprofil wurde nicht gefunden.", result.Issues);
        Assert.Contains("Das ausgewählte Exportprofil wurde nicht gefunden.", result.Issues);
    }

    [Fact]
    public void CreateInterfaceProfile_ShouldPrepareCv5000DeviceOutputButKeepItInactive()
    {
        var catalog = CreateCv5000Catalog();
        var result = _service.CreateInterfaceProfile(
            catalog,
            new UserDefinedInterfaceProfileCreationRequest(
                "MEDISTAR + CV5000 Raum 1",
                "ais-medistar-default",
                "device-topcon-cv5000-default",
                "export-medistar-topcon-cv5000-default"),
            _timestamp,
            "Tester");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues));
        var profile = result.Profile!;
        Assert.False(profile.IsActive);
        Assert.NotNull(profile.DeviceOutput);
        Assert.False(profile.DeviceOutput!.IsEnabled);
        Assert.Equal(string.Empty, profile.DeviceOutput.OutputFolder);
        Assert.Equal("CVImport.xml", profile.DeviceOutput.FileNameTemplate);
        Assert.Equal("TOPCON CV-5000 XML", profile.DeviceOutput.Format);
    }

    [Fact]
    public void CreateInterfaceProfile_ShouldKeepDocumentAttachmentOptionsUsable()
    {
        var catalog = CreateDocumentAttachmentCatalog();
        var result = _service.CreateInterfaceProfile(
            catalog,
            new UserDefinedInterfaceProfileCreationRequest(
                "MEDISTAR + Dokumente Praxis",
                "ais-medistar-default",
                "device-document-attachment-default",
                "export-medistar-document-attachment-default"),
            _timestamp,
            "Tester");

        Assert.True(result.Success, string.Join(Environment.NewLine, result.Issues));
        var profile = result.Profile!;
        Assert.False(profile.IsActive);
        Assert.True(profile.FolderOptions.IsAttachmentOnlyMode);
        Assert.True(profile.FolderOptions.ShowAttachmentDocumentationDialog);
        Assert.Equal(AttachmentRequirementMode.Required, profile.FolderOptions.AttachmentRequirementMode);
        Assert.Null(profile.DeviceOutput);
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

    private static ProfileCatalog CreateCv5000Catalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default() },
            InterfaceProfiles: new[] { DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() });
    }

    private static ProfileCatalog CreateDocumentAttachmentCatalog()
    {
        return new ProfileCatalog(
            AisProfiles: new[] { DefaultAisProfiles.CreateMedistarDefault() },
            DeviceProfiles: new[] { DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault() },
            ExportProfiles: new[] { DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault() },
            InterfaceProfiles: new[] { DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault() });
    }
}
