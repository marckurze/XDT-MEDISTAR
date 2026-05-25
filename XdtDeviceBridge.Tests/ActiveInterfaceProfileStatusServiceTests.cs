using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class ActiveInterfaceProfileStatusServiceTests
{
    private static readonly DateTime GraceEndsAtUtc = new(2026, 6, 30, 12, 0, 0, DateTimeKind.Utc);

    private readonly ActiveInterfaceProfileStatusService _service = new();

    [Fact]
    public void BuildRows_ShouldNotShowInactiveProfile()
    {
        var rows = BuildRows(CreateProfile(isActive: false, isLicenseRequired: true));

        Assert.Empty(rows);
    }

    [Fact]
    public void BuildRows_ShouldMarkActiveProfileWithMissingFoldersAsIncomplete()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false);

        var row = Assert.Single(BuildRows(profile));

        Assert.Contains("AIS-Importordner fehlt", row.FolderStatus);
        Assert.Contains("Geräte-Importordner fehlt", row.FolderStatus);
        Assert.Contains("Exportordner fehlt", row.FolderStatus);
        Assert.Equal("Unvollständig", row.ProcessingStatus);
    }

    [Fact]
    public void BuildRows_ShouldMarkConfiguredFolders()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"\\SERVER\Freigabe\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal("Ordner konfiguriert", row.FolderStatus);
        Assert.Equal("Bereit für spätere Automatik", row.ProcessingStatus);
    }

    [Fact]
    public void BuildRows_ShouldShowNotLicenseRequiredStatus()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false);

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal("Nicht lizenzpflichtig", row.LicenseStatus);
    }

    [Fact]
    public void BuildRows_ShouldShowUncoveredLicenseRequiredStatus()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: true);
        var licenseState = CreateLicenseState(profile, isCovered: false, isInGracePeriod: false);

        var row = Assert.Single(BuildRows(profile, licenseState));

        Assert.Equal("Nicht gedeckt", row.LicenseStatus);
    }

    [Fact]
    public void BuildRows_ShouldShowGracePeriodStatus()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: true);
        var licenseState = CreateLicenseState(profile, isCovered: false, isInGracePeriod: true, GraceEndsAtUtc);

        var row = Assert.Single(BuildRows(profile, licenseState));

        Assert.StartsWith("In Karenzzeit bis", row.LicenseStatus, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildRows_ShouldMarkCompleteCoveredProfileAsReadyForFutureAutomation()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: true) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };
        var licenseState = CreateLicenseState(profile, isCovered: true, isInGracePeriod: false);

        var row = Assert.Single(BuildRows(profile, licenseState));

        Assert.Equal("Lizenz gedeckt", row.LicenseStatus);
        Assert.Equal("Bereit für spätere Automatik", row.ProcessingStatus);
    }

    [Fact]
    public void BuildRows_ShouldShowNoAttachmentConfiguredWhenAttachmentOptionsAreEmpty()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal("kein Anhang konfiguriert", row.AttachmentImportFolder);
        Assert.Equal("kein Anhang konfiguriert", row.AttachmentExportFolder);
        Assert.Equal("kein Anhang konfiguriert", row.AttachmentConfigurationStatus);
        Assert.Equal("kein Anhang konfiguriert", row.MonitoringCard.AttachmentConfigurationStatus);
        Assert.DoesNotContain(row.MonitoringCard.ExpectedInputs, input => input.Name == "XDT-Anhang");
    }

    [Fact]
    public void BuildRows_ShouldShowAttachmentFoldersWhenConfigured()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export") with
            {
                AttachmentImportFolder = @"C:\XDT\AnhangImp",
                AttachmentExportFolder = @"C:\XDT\AnhangExp",
                IsAttachmentProcessingEnabled = true,
                AttachmentRequirementMode = AttachmentRequirementMode.Required
            }
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal(@"C:\XDT\AnhangImp", row.AttachmentImportFolder);
        Assert.Equal(@"C:\XDT\AnhangExp", row.AttachmentExportFolder);
        Assert.Equal("XDT-Anhang aktiv (Pflicht)", row.AttachmentConfigurationStatus);
        Assert.Equal(row.AttachmentImportFolder, row.MonitoringCard.AttachmentImportFolder);
        Assert.Equal(row.AttachmentExportFolder, row.MonitoringCard.AttachmentExportFolder);
        Assert.Contains(row.MonitoringCard.ExpectedInputs, input => input.Name == "XDT-Anhang");
    }

    [Fact]
    public void BuildRows_ShouldMarkIncompleteAttachmentFolders()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export") with
            {
                IsAttachmentProcessingEnabled = true
            }
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal("XDT-Anhang Importordner fehlt", row.AttachmentImportFolder);
        Assert.Equal("XDT-Anhang Exportordner fehlt", row.AttachmentExportFolder);
        Assert.Equal("XDT-Anhang aktiv, Ordner unvollständig", row.AttachmentConfigurationStatus);
    }

    [Fact]
    public void BuildRows_AttachmentOnlyShouldShowNeutralDocumentInputBeforePackageStarts()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Dokumente",
                exportFolder: @"C:\XDT\Export") with
            {
                IsAttachmentOnlyMode = true,
                IsAttachmentProcessingEnabled = true,
                AttachmentRequirementMode = AttachmentRequirementMode.Required,
                AttachmentExportFolder = @"C:\XDT\AnhangExp"
            }
        };

        var row = Assert.Single(BuildRows(profile));

        var documentInput = Assert.Single(row.MonitoringCard.ExpectedInputs, input => input.Name == "Dokumentdateien");
        Assert.Equal("gestoppt", documentInput.Status);
        Assert.Equal("Neutral", documentInput.StatusClass);
        Assert.DoesNotContain(row.MonitoringCard.ExpectedInputs, input => input.Key == "attachment");
    }

    [Fact]
    public void BuildRows_ShouldCreateMonitoringCardWithProfileNamesAndDefaultRuntimeStatus()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal("Test-Schnittstelle", row.MonitoringCard.InterfaceProfileName);
        Assert.Equal("MEDISTAR", row.MonitoringCard.AisName);
        Assert.Equal("NIDEK ARK1S", row.MonitoringCard.DeviceName);
        Assert.Equal("MEDISTAR + NIDEK ARK1S Export", row.MonitoringCard.ExportProfileName);
        Assert.Equal("Gestoppt", row.MonitoringCard.CurrentStatus);
        Assert.Equal("Neutral", row.MonitoringCard.StatusClass);
        Assert.Equal(5, row.MonitoringCard.ScanIntervalSeconds);
        Assert.Equal("5 s", row.MonitoringCard.ScanIntervalText);
        Assert.False(row.MonitoringCard.IsScanAnimationActive);
        Assert.Equal("-", row.MonitoringCard.LastScanText);
        Assert.Equal("Nein", row.MonitoringCard.AutomaticProcessingText);
        Assert.True(row.MonitoringCard.UsesPilotDeviceVisual);
        Assert.Equal(string.Empty, row.MonitoringCard.DeviceImagePath);
        Assert.False(row.MonitoringCard.HasDeviceImage);
    }

    [Fact]
    public void BuildRows_ShouldUseStandardMonitoringVisualForCv5000()
    {
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() with
        {
            IsActive = true,
            IsLicenseRequired = false,
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(_service.BuildRows(
            new[] { interfaceProfile },
            new[] { DefaultAisProfiles.CreateMedistarDefault() },
            new[] { DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() },
            new[] { DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default() },
            Array.Empty<LicensedDeviceState>()));

        Assert.True(row.MonitoringCard.UsesPilotDeviceVisual);
        Assert.Equal("Phoropter", row.MonitoringCard.DeviceType);
        Assert.True(row.MonitoringCard.HasDeviceImage);
        Assert.Equal(InterfaceProfileUiPolicy.TopconCv5000DeviceImagePath, row.MonitoringCard.DeviceImagePath);
        Assert.False(row.MonitoringCard.ShouldPulseStatusOrb);
        Assert.All(row.MonitoringCard.ExpectedInputs, input => Assert.Equal("gestoppt", input.Status));

        var activeCard = row.MonitoringCard with
        {
            IsScanAnimationActive = true
        };
        Assert.True(activeCard.ShouldPulseStatusOrb);
        var activeRuntimeCard = row.MonitoringCard.WithPilotMonitoringActivity(isMonitoringActive: true);
        Assert.True(activeRuntimeCard.ShouldPulseStatusOrb);
        Assert.All(activeRuntimeCard.ExpectedInputs, input => Assert.Equal("wartet", input.Status));
        var stoppedRuntimeCard = activeRuntimeCard.WithPilotMonitoringActivity(isMonitoringActive: false);
        Assert.False(stoppedRuntimeCard.ShouldPulseStatusOrb);
        Assert.All(stoppedRuntimeCard.ExpectedInputs, input => Assert.Equal("gestoppt", input.Status));
    }

    [Fact]
    public void BuildRows_ShouldUsePlaceholderWhenConfiguredDeviceImageIsMissing()
    {
        var missingImagePath = @"C:\Praxis\Bilder\nicht-vorhanden.png";
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };
        var deviceProfile = DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() with
        {
            DeviceImagePath = missingImagePath
        };

        var row = Assert.Single(_service.BuildRows(
            new[] { profile },
            new[] { DefaultAisProfiles.CreateMedistarDefault() },
            new[] { deviceProfile },
            new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            Array.Empty<LicensedDeviceState>()));

        Assert.True(row.MonitoringCard.UsesPilotDeviceVisual);
        Assert.Equal(missingImagePath, row.MonitoringCard.DeviceImagePath);
        Assert.False(row.MonitoringCard.HasDeviceImage);
    }

    [Fact]
    public void BuildRows_ShouldPreferExistingDeviceImageOverride()
    {
        var overrideImagePath = Path.Combine(Path.GetTempPath(), "XdtDeviceBridgeTests", Guid.NewGuid().ToString("N"), "cv5000.png");
        Directory.CreateDirectory(Path.GetDirectoryName(overrideImagePath)!);
        File.WriteAllText(overrideImagePath, "image-content");
        var interfaceProfile = DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default() with
        {
            IsActive = true,
            IsLicenseRequired = false,
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(_service.BuildRows(
            new[] { interfaceProfile },
            new[] { DefaultAisProfiles.CreateMedistarDefault() },
            new[] { DefaultDeviceProfileDefinitions.CreateTopconCv5000Default() },
            new[] { DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default() },
            Array.Empty<LicensedDeviceState>(),
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["device-topcon-cv5000-default"] = overrideImagePath
            }));

        Assert.Equal(overrideImagePath, row.MonitoringCard.DeviceImagePath);
        Assert.True(row.MonitoringCard.HasDeviceImage);
    }

    [Theory]
    [InlineData("Solos")]
    [InlineData("CT800A")]
    [InlineData("KR1")]
    [InlineData("ARK1S")]
    public void BuildRows_ShouldUseStandardMonitoringVisualForOtherDevices(string profileKind)
    {
        var (interfaceProfile, deviceProfile, exportProfile) = CreateProfileSet(profileKind);
        interfaceProfile = interfaceProfile with
        {
            IsActive = true,
            IsLicenseRequired = false,
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(_service.BuildRows(
            new[] { interfaceProfile },
            new[] { DefaultAisProfiles.CreateMedistarDefault() },
            new[] { deviceProfile },
            new[] { exportProfile },
            Array.Empty<LicensedDeviceState>()));

        Assert.True(row.MonitoringCard.UsesPilotDeviceVisual);
        Assert.False(row.MonitoringCard.ShouldPulseStatusOrb);
        Assert.All(row.MonitoringCard.ExpectedInputs, input => Assert.Equal("gestoppt", input.Status));
    }

    [Fact]
    public void BuildRows_ShouldUseConfiguredScanIntervalForMonitoringCard()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export") with
            {
                AutoImportScanIntervalSeconds = 12
            }
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Equal(12, row.MonitoringCard.ScanIntervalSeconds);
        Assert.Equal("12 s", row.MonitoringCard.ScanIntervalText);
        Assert.False(row.MonitoringCard.IsScanAnimationActive);
    }

    [Fact]
    public void BuildRows_ShouldCreateExpectedInputBadgesForAisAndDevice()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export")
        };

        var row = Assert.Single(BuildRows(profile));

        var aisInput = Assert.Single(row.MonitoringCard.ExpectedInputs, input => input.Name == "AIS-Patientendatei");
        Assert.Equal("gestoppt", aisInput.Status);
        Assert.Equal("Neutral", aisInput.StatusClass);
        Assert.Equal(@"C:\XDT\AIS", aisInput.FolderPath);

        var deviceInput = Assert.Single(row.MonitoringCard.ExpectedInputs, input => input.Name == "Geräte-Datei");
        Assert.Equal("gestoppt", deviceInput.Status);
        Assert.Equal("Neutral", deviceInput.StatusClass);
        Assert.Equal(@"C:\XDT\Device", deviceInput.FolderPath);
    }

    [Fact]
    public void BuildRows_ShouldCreateFolderDetailsForMonitoringCard()
    {
        var profile = CreateProfile(isActive: true, isLicenseRequired: false) with
        {
            FolderOptions = CreateFolderOptions(
                aisImportFolder: @"C:\XDT\AIS",
                deviceImportFolder: @"C:\XDT\Device",
                exportFolder: @"C:\XDT\Export") with
            {
                ArchiveFolder = @"C:\XDT\Archiv",
                ErrorFolder = @"C:\XDT\Fehler",
                AttachmentImportFolder = @"C:\XDT\AnhangImp",
                AttachmentExportFolder = @"C:\XDT\AnhangExp",
                AttachmentRequirementMode = AttachmentRequirementMode.Required
            }
        };

        var row = Assert.Single(BuildRows(profile));

        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "AIS-Importordner" && detail.Value == @"C:\XDT\AIS");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "Geräte-Importordner" && detail.Value == @"C:\XDT\Device");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "Exportordner ans AIS" && detail.Value == @"C:\XDT\Export");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "Archivordner" && detail.Value == @"C:\XDT\Archiv");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "Fehlerordner" && detail.Value == @"C:\XDT\Fehler");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "XDT-Anhang Importordner" && detail.Value == @"C:\XDT\AnhangImp");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "XDT-Anhang Exportordner" && detail.Value == @"C:\XDT\AnhangExp");
        Assert.Contains(row.MonitoringCard.FolderDetails, detail => detail.Name == "Anhang Erwartung" && detail.Value == "Pflicht");
    }

    private IReadOnlyList<ActiveInterfaceProfileStatusRow> BuildRows(
        InterfaceProfileDefinition profile,
        LicensedDeviceState? licenseState = null)
    {
        return _service.BuildRows(
            new[] { profile },
            new[] { DefaultAisProfiles.CreateMedistarDefault() },
            new[] { DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault() },
            new[] { DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault() },
            licenseState is null ? Array.Empty<LicensedDeviceState>() : new[] { licenseState });
    }

    private static InterfaceProfileDefinition CreateProfile(bool isActive, bool isLicenseRequired)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            IsActive = isActive,
            IsLicenseRequired = isLicenseRequired,
            Metadata = DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault().Metadata with
            {
                Id = "interface-test",
                Name = "Test-Schnittstelle",
                IsBuiltIn = false,
                IsUserDefined = true
            }
        };
    }

    private static (InterfaceProfileDefinition InterfaceProfile, DeviceProfileDefinition DeviceProfile, ExportProfileDefinition ExportProfile) CreateProfileSet(string profileKind)
    {
        return profileKind switch
        {
            "Solos" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconSolosDefault(),
                DefaultDeviceProfileDefinitions.CreateTopconSolosDefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconSolosDefault()),
            "CT800A" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt800ADefault(),
                DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault(),
                DefaultExportProfileDefinitions.CreateMedistarTopconCt800ADefault()),
            "KR1" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr1Default(),
                DefaultDeviceProfileDefinitions.CreateTopconKr1Default(),
                DefaultExportProfileDefinitions.CreateMedistarTopconKr1Default()),
            "ARK1S" => (
                DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
                DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(),
                DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault()),
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };
    }

    private static InterfaceFolderOptions CreateFolderOptions(
        string aisImportFolder,
        string deviceImportFolder,
        string exportFolder)
    {
        return new InterfaceFolderOptions(
            AisImportFolder: aisImportFolder,
            DeviceImportFolder: deviceImportFolder,
            ExportFolder: exportFolder,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ClearAisImportFolderBeforeProcessing: false,
            ClearDeviceImportFolderBeforeProcessing: false,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: false,
            MoveFailedFilesToErrorFolder: true);
    }

    private static LicensedDeviceState CreateLicenseState(
        InterfaceProfileDefinition profile,
        bool isCovered,
        bool isInGracePeriod,
        DateTime? graceEndsAtUtc = null)
    {
        return new LicensedDeviceState(
            InterfaceProfileId: profile.Metadata.Id,
            DisplayName: profile.Metadata.Name,
            IsActive: profile.IsActive,
            IsLicenseRequired: profile.IsLicenseRequired,
            IsCoveredByLicense: isCovered,
            IsInGracePeriod: isInGracePeriod,
            GracePeriodStartedAt: isInGracePeriod ? GraceEndsAtUtc.AddDays(-30) : null,
            GracePeriodEndsAt: graceEndsAtUtc,
            StatusMessage: "Teststatus");
    }
}
