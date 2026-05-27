using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseManagerRepositoryTests
{
    [Fact]
    public void LicenseManagerPathProvider_ShouldUseExpectedDefaultFiles()
    {
        var provider = new LicenseManagerPathProvider();
        var paths = provider.GetPaths(Path.Combine(Path.GetTempPath(), "XdtBoxLicenseManagerTests"));

        Assert.EndsWith(Path.Combine("licenses"), paths.LicensesFolder);
        Assert.EndsWith(Path.Combine("requests"), paths.RequestsFolder);
        Assert.EndsWith(Path.Combine("keys"), paths.KeysFolder);
        Assert.EndsWith(Path.Combine("data", "license-history.json"), paths.HistoryFile);
        Assert.EndsWith(Path.Combine("data", "license-manager-settings.json"), paths.SettingsFile);
    }

    [Fact]
    public void SettingsRepository_ShouldPersistPrivateKeyPathButNotKeyContent()
    {
        var filePath = CreateTempFilePath("settings.json");
        var repository = new LicenseManagerSettingsRepository();
        var settings = new LicenseManagerSettings(
            DefaultOutputFolder: @"C:\XDTBox\Lizenzaktivierung\licenses",
            DefaultRequestFolder: @"C:\XDTBox\Lizenzaktivierung\requests",
            DefaultKeyFolder: @"C:\XDTBox\Lizenzaktivierung\keys",
            PrivateKeyPath: @"C:\XDTBox\Lizenzaktivierung\keys\xdtbox_private.pem",
            KeyId: "xdtbox-prod-2026-01",
            DefaultIssuer: "Technik-Apparat",
            DefaultGraceDays: 7);

        repository.Save(filePath, settings);
        var loaded = repository.LoadOrDefault(filePath, @"C:\XDTBox\Lizenzaktivierung");
        var json = File.ReadAllText(filePath);

        Assert.Equal(settings.PrivateKeyPath, loaded.PrivateKeyPath);
        Assert.Contains("xdtbox_private.pem", json);
        Assert.DoesNotContain("BEGIN PRIVATE KEY", json);
    }

    [Fact]
    public void HistoryRepository_Add_ShouldAppendWithoutOverwritingExistingEntries()
    {
        var filePath = CreateTempFilePath("history.json");
        var repository = new IssuedLicenseHistoryRepository();

        repository.Add(filePath, CreateRecord("license-1", "Praxis A"));
        var records = repository.Add(filePath, CreateRecord("license-2", "Praxis B"));

        Assert.Equal(2, records.Count);
        Assert.Contains(records, record => record.LicenseId == "license-1");
        Assert.Contains(records, record => record.LicenseId == "license-2");
    }

    [Fact]
    public void HistoryRepository_ShouldPersistDeviceDocumentation()
    {
        var filePath = CreateTempFilePath("history.json");
        var repository = new IssuedLicenseHistoryRepository();
        var record = CreateRecord("license-1", "Praxis A") with
        {
            Devices = new[]
            {
                new IssuedLicenseDeviceRecord(
                    DisplayName: "MEDISTAR + NIDEK LM7",
                    DeviceDisplayName: "NIDEK LM7",
                    InterfaceProfileId: "interface-lm7",
                    DeviceProfileId: "device-lm7",
                    ConnectionKind: DeviceConnectionKind.SerialRs232)
            }
        };

        repository.Add(filePath, record);
        var loaded = repository.LoadOrEmpty(filePath);

        var device = Assert.Single(Assert.Single(loaded).Devices);
        Assert.Equal("MEDISTAR + NIDEK LM7", device.DisplayName);
        Assert.Equal(DeviceConnectionKind.SerialRs232, device.ConnectionKind);
    }

    [Fact]
    public void CustomerDataRepository_ShouldRoundTripCustomerData()
    {
        var filePath = CreateTempFilePath("customer.json");
        var repository = new LicenseCustomerDataRepository();
        var customer = new LicenseRequestCustomer(
            CustomerName: "Praxis Muster",
            Street: "Musterstraße 1",
            PostalCode: "12345",
            City: "Musterstadt",
            Phone: "01234",
            Email: "info@example.test",
            ContactPerson: "Frau Muster");

        repository.Save(filePath, customer);
        var loaded = repository.LoadOrEmpty(filePath);

        Assert.Equal("Praxis Muster", loaded.CustomerName);
        Assert.Equal("Musterstadt", loaded.City);
        Assert.Equal("Frau Muster", loaded.ContactPerson);
    }

    private static IssuedLicenseRecord CreateRecord(string licenseId, string customerName)
    {
        var now = new DateTime(2026, 5, 27, 12, 0, 0, DateTimeKind.Utc);

        return new IssuedLicenseRecord(
            LicenseId: licenseId,
            IssuedAtUtc: now,
            LicenseeName: customerName,
            CustomerNumber: null,
            CustomerName: customerName,
            Street: "Musterstraße 1",
            PostalCode: "12345",
            City: "Musterstadt",
            Phone: "01234",
            Email: null,
            ContactPerson: null,
            InstallationId: "installation-1",
            MaxActiveDeviceConnections: 3,
            ValidFromUtc: now.Date,
            ValidUntilUtc: now.Date.AddYears(1),
            GraceDays: 7,
            LicenseType: "Production",
            KeyId: "xdtbox-prod-2026-01",
            OutputFilePath: @"C:\XDTBox\Lizenzaktivierung\licenses\test.xdtboxlic",
            RequestFilePath: @"C:\XDTBox\Lizenzaktivierung\requests\request.json",
            Notes: null,
            Devices: Array.Empty<IssuedLicenseDeviceRecord>());
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtBoxLicenseManagerTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }
}
