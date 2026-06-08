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
        Assert.EndsWith(Path.Combine("data", "license-manager-customers.json"), paths.CustomersFile);
        Assert.EndsWith(Path.Combine("backups"), paths.BackupFolder);
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
            KeyId: LicensePublicKeyProvider.ProductionKeyId,
            DefaultIssuer: "Technik-Apparat",
            DefaultGraceDays: 7,
            PricePerDeviceNet: 5m);

        repository.Save(filePath, settings);
        var loaded = repository.LoadOrDefault(filePath, @"C:\XDTBox\Lizenzaktivierung");
        var json = File.ReadAllText(filePath);

        Assert.Equal(settings.PrivateKeyPath, loaded.PrivateKeyPath);
        Assert.Equal(5m, loaded.PricePerDeviceNet);
        Assert.Contains("xdtbox_private.pem", json);
        Assert.DoesNotContain("BEGIN PRIVATE KEY", json);
    }

    [Fact]
    public void CostCalculator_ShouldCalculateNetTotal()
    {
        var total = LicenseManagerCostCalculator.CalculateNetTotal(14, 5m);

        Assert.Equal(70m, total);
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
            ContactPerson: "Frau Muster",
            Iban: "DE00123456780000000000",
            Bic: "TESTDEFFXXX",
            AccountHolder: "Praxis Muster",
            SepaDirectDebitConsent: true,
            AlwaysInvoice: false,
            InvoiceEmail: "rechnung@example.test");

        repository.Save(filePath, customer);
        var loaded = repository.LoadOrEmpty(filePath);

        Assert.Equal("Praxis Muster", loaded.CustomerName);
        Assert.Equal("Musterstadt", loaded.City);
        Assert.Equal("Frau Muster", loaded.ContactPerson);
        Assert.Equal("DE00123456780000000000", loaded.Iban);
        Assert.Equal("TESTDEFFXXX", loaded.Bic);
        Assert.Equal("Praxis Muster", loaded.AccountHolder);
        Assert.True(loaded.SepaDirectDebitConsent);
        Assert.False(loaded.AlwaysInvoice);
        Assert.Equal("rechnung@example.test", loaded.InvoiceEmail);
    }

    [Fact]
    public void LicenseManagerCustomerRepository_ShouldCreateCustomerWithoutCustomerNumber()
    {
        var filePath = CreateTempFilePath("customers.json");
        var repository = new LicenseManagerCustomerRepository();
        var customer = CreateCustomer("installation-1") with { CustomerNumber = null };

        var customers = repository.Upsert(filePath, customer);

        var stored = Assert.Single(customers);
        Assert.Null(stored.CustomerNumber);
        Assert.Equal("installation-1", stored.InstallationId);
    }

    [Fact]
    public void LicenseManagerCustomerRepository_ShouldUpdateSameInstallationInsteadOfDuplicating()
    {
        var filePath = CreateTempFilePath("customers.json");
        var repository = new LicenseManagerCustomerRepository();

        repository.Upsert(filePath, CreateCustomer("installation-1") with { CustomerName = "Praxis Alt" });
        var customers = repository.Upsert(filePath, CreateCustomer("installation-1") with { CustomerName = "Praxis Neu", CustomerNumber = "K-100" });

        var stored = Assert.Single(customers);
        Assert.Equal("Praxis Neu", stored.CustomerName);
        Assert.Equal("K-100", stored.CustomerNumber);
    }

    [Fact]
    public void LicenseManagerCustomerRepository_ShouldRejectCorruptedJsonWithControlledMessage()
    {
        var filePath = CreateTempFilePath("customers.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, "{ invalid json");
        var repository = new LicenseManagerCustomerRepository();

        var exception = Assert.Throws<InvalidOperationException>(() => repository.LoadOrEmpty(filePath));

        Assert.Contains("Invalid license manager customer JSON:", exception.Message);
    }

    [Fact]
    public void LicenseManagerBackupService_ShouldRoundTripCustomersSettingsAndHistoryWithoutPrivateKeyPath()
    {
        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtBoxLicenseManagerTests", Guid.NewGuid().ToString("N"));
        var paths = new LicenseManagerPathProvider().GetPaths(baseFolder);
        var backupFile = Path.Combine(baseFolder, "backup.xdtbox-licensemanager-backup");
        var settings = LicenseManagerSettings.CreateDefault(baseFolder) with
        {
            PrivateKeyPath = Path.Combine(baseFolder, "keys", "private.pem"),
            PricePerDeviceNet = 5m
        };
        var customers = new[] { CreateCustomer("installation-1") };
        var history = new[] { CreateRecord("license-1", "Praxis Muster") };
        var service = new LicenseManagerBackupService();

        service.CreateBackup(backupFile, customers, settings, history);
        var data = service.ReadBackup(backupFile);

        Assert.Single(data.Customers);
        Assert.Equal(5m, data.Settings.PricePerDeviceNet);
        Assert.Null(data.Settings.PrivateKeyPath);
        Assert.Single(data.History);
    }

    [Fact]
    public void LicenseManagerCustomerPdfExporter_ShouldCreatePdfWithoutPrivateKeyData()
    {
        var filePath = CreateTempFilePath("customers.pdf");
        var exporter = new LicenseManagerCustomerPdfExporter();

        exporter.ExportCustomers(filePath, new[] { CreateCustomer("installation-1") }, 5m, new DateTime(2026, 6, 8, 12, 0, 0));

        var pdf = File.ReadAllText(filePath);
        Assert.StartsWith("%PDF", pdf);
        Assert.Contains("XDTBox Kunden- und Lizenzuebersicht", pdf);
        Assert.Contains("70,00 EUR", pdf);
        Assert.DoesNotContain("BEGIN PRIVATE KEY", pdf);
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

    private static LicenseManagerCustomerRecord CreateCustomer(string installationId)
    {
        return new LicenseManagerCustomerRecord(
            Id: Guid.NewGuid().ToString("N"),
            CustomerNumber: "K-100",
            CustomerName: "Praxis Muster",
            Street: "Musterstrasse 1",
            PostalCode: "12345",
            City: "Musterstadt",
            Phone: "01234",
            Email: "info@example.test",
            ContactPerson: "Frau Muster",
            InvoiceEmail: "rechnung@example.test",
            Iban: "DE00123456780000000000",
            Bic: "TESTDEFFXXX",
            AccountHolder: "Praxis Muster",
            SepaDirectDebitConsent: true,
            AlwaysInvoice: false,
            InstallationId: installationId,
            MachineName: "TEST-PC",
            ActiveLicensedDeviceCount: 14,
            Devices: new[]
            {
                new IssuedLicenseDeviceRecord(
                    DisplayName: "MEDISTAR + NIDEK LM7",
                    DeviceDisplayName: "NIDEK LM7",
                    InterfaceProfileId: "interface-lm7",
                    DeviceProfileId: "device-lm7",
                    ConnectionKind: DeviceConnectionKind.NetworkLan)
            },
            UpdatedAtUtc: DateTime.UtcNow);
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtBoxLicenseManagerTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }
}
