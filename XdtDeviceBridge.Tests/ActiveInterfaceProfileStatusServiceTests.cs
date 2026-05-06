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
