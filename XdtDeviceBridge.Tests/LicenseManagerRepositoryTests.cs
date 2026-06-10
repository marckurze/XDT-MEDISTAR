using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;
using System.Text;

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
    public void HistoryRepository_Remove_ShouldDeleteOnlyMatchingEntry()
    {
        var filePath = CreateTempFilePath("history.json");
        var repository = new IssuedLicenseHistoryRepository();
        var first = CreateRecord("license-1", "Praxis A") with { InstallationId = "installation-1" };
        var second = CreateRecord("license-2", "Praxis B") with { InstallationId = "installation-2" };

        repository.Add(filePath, first);
        repository.Add(filePath, second);
        var records = repository.Remove(filePath, first);

        var remaining = Assert.Single(records);
        Assert.Equal("license-2", remaining.LicenseId);
        Assert.True(File.Exists(filePath));
        Assert.DoesNotContain("license-1", File.ReadAllText(filePath));
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
                    ConnectionKind: DeviceConnectionKind.SerialRs232,
                    Location: "Raum 2")
            }
        };

        repository.Add(filePath, record);
        var loaded = repository.LoadOrEmpty(filePath);

        var device = Assert.Single(Assert.Single(loaded).Devices);
        Assert.Equal("MEDISTAR + NIDEK LM7", device.DisplayName);
        Assert.Equal(DeviceConnectionKind.SerialRs232, device.ConnectionKind);
        Assert.Equal("Raum 2", device.Location);
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
    public void DeviceLocationRepository_ShouldRoundTripDeviceLocations()
    {
        var filePath = CreateTempFilePath("device-locations.json");
        var repository = new LicenseDeviceLocationRepository();
        var store = new LicenseDeviceLocationStore(new[]
        {
            new LicenseDeviceLocation("interface-lm7", "Raum 2")
        });

        repository.Save(filePath, store);
        var loaded = repository.LoadOrEmpty(filePath);

        var location = Assert.Single(loaded.DeviceLocations);
        Assert.Equal("interface-lm7", location.InterfaceProfileId);
        Assert.Equal("Raum 2", location.Location);
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
    public void LicenseManagerCustomerRepository_UpsertByInstallation_ShouldAllowSeparateCustomerWithSameNumber()
    {
        var filePath = CreateTempFilePath("customers.json");
        var repository = new LicenseManagerCustomerRepository();

        repository.Upsert(filePath, CreateCustomer("installation-1") with { CustomerName = "Praxis Bestand", CustomerNumber = "K-100" });
        var customers = repository.UpsertByInstallation(
            filePath,
            CreateCustomer("installation-2") with { CustomerName = "Praxis Neu", CustomerNumber = "K-100" });

        Assert.Equal(2, customers.Count);
        Assert.Contains(customers, customer => customer.CustomerName == "Praxis Bestand");
        Assert.Contains(customers, customer => customer.CustomerName == "Praxis Neu");
    }

    [Fact]
    public void LicenseManagerCustomerRecord_FromRequest_ShouldKeepOnlyActiveLicenseRequiredDevices()
    {
        var request = new LicenseRequest(
            RequestId: "request-1",
            InstallationId: "installation-1",
            MachineName: "TEST-PC",
            UserName: "tester",
            IsTerminalServer: false,
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            AppVersion: "1.0",
            ActiveLicensedDeviceCount: 3,
            Devices: new[]
            {
                new LicenseRequestDevice("active-licensed", "MEDISTAR + NIDEK LM7", "NIDEK", "LM7", "profile-lm7", IsActive: true, IsLicenseRequired: true, Location: "Raum 1"),
                new LicenseRequestDevice("inactive", "MEDISTAR + NIDEK RT-6100", "NIDEK", "RT-6100", "profile-rt6100", IsActive: false, IsLicenseRequired: true),
                new LicenseRequestDevice("free", "Nicht lizenzpflichtig", "XDTBox", "Doku", "profile-free", IsActive: true, IsLicenseRequired: false)
            },
            CreatedAt: DateTime.UtcNow,
            Customer: new LicenseRequestCustomer(
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
                SepaDirectDebitConsent: false,
                AlwaysInvoice: true,
                InvoiceEmail: "rechnung@example.test",
                CustomerNumber: "K-100"));

        var customer = LicenseManagerCustomerRecord.FromRequest(request);

        Assert.Equal(LicenseManagerPaymentMethod.BankTransfer, customer.PaymentMethod);
        Assert.Equal(1, customer.BillableDeviceCount);
        Assert.Single(customer.EffectiveDevices);
        Assert.Equal("MEDISTAR + NIDEK LM7", Assert.Single(customer.EffectiveDevices).DisplayName);
        Assert.Equal("Raum 1", Assert.Single(customer.EffectiveDevices).Location);
    }

    [Fact]
    public void LicenseManagerCustomerRecord_ShouldSumMultipleInstallationsAndExcludeCancelledOnes()
    {
        var customer = CreateCustomer("installation-1")
            .WithLicense(CreateRecord("license-1", "Praxis Muster") with
            {
                InstallationId = "installation-1",
                Devices = CreateIssuedDevices("LM7", "AR360")
            })
            .WithLicense(CreateRecord("license-2", "Praxis Muster") with
            {
                InstallationId = "installation-2",
                Devices = CreateIssuedDevices("RT-6100")
            });

        Assert.Equal(2, customer.ActiveInstallationCount);
        Assert.Equal(3, customer.BillableDeviceCount);
        Assert.Equal(15m, LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, 5m));

        var cancelled = customer.CancelInstallation("installation-1", DateTime.UtcNow, "Teststorno");

        Assert.Equal(1, cancelled.ActiveInstallationCount);
        Assert.Equal(1, cancelled.BillableDeviceCount);
        Assert.Equal(5m, LicenseManagerCostCalculator.CalculateNetTotal(cancelled.BillableDeviceCount, 5m));
        Assert.Contains(cancelled.EffectiveInstallations, installation => installation.Status == LicenseManagerInstallationStatus.Cancelled);
    }

    [Fact]
    public void LicenseManagerCustomerRecord_ShouldPreserveExplicitlyEmptyInstallations()
    {
        var customer = CreateCustomer("installation-1") with
        {
            Installations = Array.Empty<LicenseManagerInstallationRecord>()
        };

        var normalized = customer.WithNormalizedInstallations();

        Assert.Empty(normalized.EffectiveInstallations);
        Assert.Equal(0, normalized.ActiveInstallationCount);
        Assert.Equal(0, normalized.BillableDeviceCount);
        Assert.Equal(string.Empty, normalized.InstallationId);
        Assert.Empty(normalized.Devices);
    }

    [Fact]
    public void LicenseManagerCustomerRecord_ShouldCalculatePortfolioMonthlyTotal()
    {
        var first = CreateCustomer("installation-1").WithLicense(CreateRecord("license-1", "Praxis A") with
        {
            InstallationId = "installation-1",
            Devices = CreateIssuedDevices("LM7", "AR360")
        });
        var second = CreateCustomer("installation-2").WithLicense(CreateRecord("license-2", "Praxis B") with
        {
            InstallationId = "installation-2",
            Devices = CreateIssuedDevices("RT-6100", "CV-5000", "NT-1E")
        });
        var customers = new[] { first, second };

        var totalDevices = customers.Sum(customer => customer.BillableDeviceCount);
        var total = LicenseManagerCostCalculator.CalculateNetTotal(totalDevices, 5m);

        Assert.Equal(5, totalDevices);
        Assert.Equal(25m, total);
    }

    [Fact]
    public void LicenseManagerCustomerMergeService_ShouldMergeTwoCustomersIntoSelectedTarget()
    {
        var target = CreateCustomer("installation-a") with
        {
            Id = "customer-a",
            CustomerNumber = "K-100",
            CustomerName = "Praxis A"
        };
        target = target.WithLicense(CreateRecord("license-a", "Praxis A") with
        {
            InstallationId = "installation-a",
            Devices = CreateIssuedDevices("LM7").Select(device => device with { Location = "Raum 1" }).ToArray()
        });
        var source = CreateCustomer("installation-b") with
        {
            Id = "customer-b",
            CustomerNumber = "K-200",
            CustomerName = "Praxis B",
            ContactPerson = "Frau B"
        };
        source = source.WithLicense(CreateRecord("license-b", "Praxis B") with
        {
            InstallationId = "installation-b",
            Devices = CreateIssuedDevices("AR360", "RT6100").Select(device => device with { Location = "Raum 2" }).ToArray()
        }) with
        {
            ContactPerson = "Frau B"
        };
        var selection = new LicenseManagerCustomerMergeSelection(
            TargetCustomerId: target.Id,
            CustomerNumber: "K-200",
            CustomerName: "Praxis Zusammen",
            ContactPerson: source.ContactPerson,
            Street: target.Street,
            PostalCode: target.PostalCode,
            City: target.City,
            Phone: target.Phone,
            Email: target.Email,
            InvoiceEmail: source.InvoiceEmail,
            PaymentMethod: LicenseManagerPaymentMethod.BankTransfer,
            Iban: source.Iban,
            Bic: source.Bic,
            AccountHolder: source.AccountHolder);

        var result = new LicenseManagerCustomerMergeService().Merge(
            new[] { target, source },
            new[] { target.Id, source.Id },
            selection);

        var merged = Assert.Single(result.Customers);
        Assert.Equal(target.Id, merged.Id);
        Assert.Equal("K-200", merged.CustomerNumber);
        Assert.Equal("Praxis Zusammen", merged.CustomerName);
        Assert.Equal("Frau B", merged.ContactPerson);
        Assert.Equal(LicenseManagerPaymentMethod.BankTransfer, merged.PaymentMethod);
        Assert.Equal(2, merged.ActiveInstallationCount);
        Assert.Equal(3, merged.BillableDeviceCount);
        Assert.Contains(merged.EffectiveInstallations, installation => installation.InstallationId == "installation-a");
        Assert.Contains(merged.EffectiveInstallations, installation => installation.InstallationId == "installation-b");
        Assert.Contains(merged.EffectiveDevices, device => device.Location == "Raum 1");
        Assert.Contains(merged.EffectiveDevices, device => device.Location == "Raum 2");
        Assert.Equal(new[] { source.Id }, result.RemovedCustomerIds);
    }

    [Fact]
    public void LicenseManagerCustomerMergeService_ShouldMergeThreeCustomersAndKeepCancelledInstallationsCancelled()
    {
        var target = CreateCustomer("installation-a") with { Id = "customer-a" };
        target = target.WithLicense(CreateRecord("license-a", "Praxis A") with
        {
            InstallationId = "installation-a",
            Devices = CreateIssuedDevices("LM7")
        });
        var source = CreateCustomer("installation-b") with { Id = "customer-b" };
        source = source.WithLicense(CreateRecord("license-b", "Praxis B") with
        {
            InstallationId = "installation-b",
            Devices = CreateIssuedDevices("AR360", "NT530P")
        });
        var cancelled = CreateCustomer("installation-c") with { Id = "customer-c" };
        cancelled = cancelled.WithLicense(CreateRecord("license-c", "Praxis C") with
        {
            InstallationId = "installation-c",
            Devices = CreateIssuedDevices("RT6100")
        }).CancelInstallation("installation-c", new DateTime(2026, 6, 9, 8, 0, 0, DateTimeKind.Utc), "Storno");

        var result = new LicenseManagerCustomerMergeService().Merge(
            new[] { target, source, cancelled },
            new[] { target.Id, source.Id, cancelled.Id },
            CreateMergeSelectionFrom(target));

        var merged = Assert.Single(result.Customers);
        Assert.Equal(3, merged.EffectiveInstallations.Count);
        Assert.Equal(2, merged.ActiveInstallationCount);
        Assert.Equal(3, merged.BillableDeviceCount);
        Assert.Contains(merged.EffectiveInstallations, installation =>
            installation.InstallationId == "installation-c"
            && installation.Status == LicenseManagerInstallationStatus.Cancelled);
    }

    [Fact]
    public void LicenseManagerCustomerMergeService_ShouldResolveDuplicateInstallationIdsWithoutDuplicatingDevices()
    {
        var target = CreateCustomer("installation-a") with { Id = "customer-a" };
        target = target.WithLicense(CreateRecord("license-a", "Praxis A") with
        {
            InstallationId = "installation-a",
            Devices = CreateIssuedDevices("LM7").Select(device => device with { Location = "Raum 1" }).ToArray()
        });
        var source = CreateCustomer("installation-a") with { Id = "customer-b" };
        source = source.WithLicense(CreateRecord("license-b", "Praxis B") with
        {
            InstallationId = "installation-a",
            Devices = CreateIssuedDevices("LM7", "AR360").Select(device => device with { Location = "Raum 2" }).ToArray()
        }).CancelInstallation(
            "installation-a",
            new DateTime(2026, 6, 9, 9, 0, 0, DateTimeKind.Utc),
            "Storno im Bestand");

        var result = new LicenseManagerCustomerMergeService().Merge(
            new[] { target, source },
            new[] { target.Id, source.Id },
            CreateMergeSelectionFrom(target));

        var merged = Assert.Single(result.Customers);
        var installation = Assert.Single(merged.EffectiveInstallations);
        Assert.Equal("installation-a", installation.InstallationId);
        Assert.Equal(LicenseManagerInstallationStatus.Cancelled, installation.Status);
        Assert.Equal(0, merged.BillableDeviceCount);
        Assert.Equal(2, installation.Devices.Count);
        Assert.Contains(installation.Devices, device => device.DeviceDisplayName == "NIDEK LM7" && device.Location == "Raum 2");
        Assert.Contains(installation.Devices, device => device.DeviceDisplayName == "NIDEK AR360");
    }

    [Fact]
    public void LicenseManagerCustomerRepository_MergeCustomers_ShouldPersistAndRoundTripThroughBackup()
    {
        var filePath = CreateTempFilePath("customers.json");
        var repository = new LicenseManagerCustomerRepository();
        var target = CreateCustomer("installation-a") with { Id = "customer-a" };
        var source = CreateCustomer("installation-b") with { Id = "customer-b", CustomerName = "Praxis Quelle" };
        repository.Save(filePath, new[] { target, source });

        var result = repository.MergeCustomers(filePath, new[] { target.Id, source.Id }, CreateMergeSelectionFrom(target));
        var loaded = repository.LoadOrEmpty(filePath);

        var merged = Assert.Single(loaded);
        Assert.Equal(result.TargetCustomer.Id, merged.Id);
        Assert.Equal(2, merged.EffectiveInstallations.Count);
        Assert.DoesNotContain(loaded, customer => customer.Id == source.Id);

        var baseFolder = Path.Combine(Path.GetTempPath(), "XdtBoxLicenseManagerTests", Guid.NewGuid().ToString("N"));
        var backupFile = Path.Combine(baseFolder, "backup.xdtbox-licensemanager-backup");
        var settings = LicenseManagerSettings.CreateDefault(baseFolder);
        var history = new[]
        {
            CreateRecord("license-a", "Praxis A") with { InstallationId = "installation-a" },
            CreateRecord("license-b", "Praxis Quelle") with { InstallationId = "installation-b" }
        };
        var backupService = new LicenseManagerBackupService();

        backupService.CreateBackup(backupFile, loaded, settings, history);
        var backup = backupService.ReadBackup(backupFile);

        var restoredCustomer = Assert.Single(backup.Customers);
        Assert.Equal(target.Id, restoredCustomer.Id);
        Assert.Equal(2, restoredCustomer.EffectiveInstallations.Count);
        Assert.DoesNotContain(backup.Customers, customer => customer.Id == source.Id);
        Assert.Equal(2, backup.History.Count);
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
        var customer = CreateCustomer("installation-1") with
        {
            LicenseValidUntilUtc = XdtBoxLicenseConstants.UnlimitedValidUntilUtc,
            SepaDirectDebitConsent = false,
            AlwaysInvoice = true
        };

        exporter.ExportCustomers(filePath, new[] { customer }, 5m, new DateTime(2026, 6, 8, 12, 0, 0));

        var pdfBytes = File.ReadAllBytes(filePath);
        var pdf = Encoding.Latin1.GetString(pdfBytes);
        Assert.StartsWith("%PDF", pdf);
        Assert.Contains("/MediaBox [0 0 842 595]", pdf);
        Assert.Contains("XDTBox Kunden- und Lizenzübersicht", pdf);
        Assert.Contains("Geräte", pdf);
        Assert.Contains("Banküberweisung", pdf);
        Assert.Contains("unbefristet", pdf);
        Assert.Contains("Summenzeile", pdf);
        Assert.Contains("70,00 EUR", pdf);
        Assert.DoesNotContain("Kdnr | Praxis", pdf);
        Assert.DoesNotContain("Geraete", pdf);
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
            ValidUntilUtc: XdtBoxLicenseConstants.UnlimitedValidUntilUtc,
            GraceDays: 7,
            LicenseType: "Production",
            KeyId: "xdtbox-prod-2026-01",
            OutputFilePath: @"C:\XDTBox\Lizenzaktivierung\licenses\test.xdtboxlic",
            RequestFilePath: @"C:\XDTBox\Lizenzaktivierung\requests\request.json",
            Notes: null,
            Devices: Array.Empty<IssuedLicenseDeviceRecord>());
    }

    private static IssuedLicenseDeviceRecord[] CreateIssuedDevices(params string[] names)
    {
        return names
            .Select(name => new IssuedLicenseDeviceRecord(
                DisplayName: "MEDISTAR + NIDEK " + name,
                DeviceDisplayName: "NIDEK " + name,
                InterfaceProfileId: "interface-" + name.ToLowerInvariant(),
                DeviceProfileId: "device-" + name.ToLowerInvariant(),
                ConnectionKind: DeviceConnectionKind.NetworkLan))
            .ToArray();
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

    private static LicenseManagerCustomerMergeSelection CreateMergeSelectionFrom(LicenseManagerCustomerRecord target)
    {
        return new LicenseManagerCustomerMergeSelection(
            TargetCustomerId: target.Id,
            CustomerNumber: target.CustomerNumber,
            CustomerName: target.CustomerName,
            ContactPerson: target.ContactPerson,
            Street: target.Street,
            PostalCode: target.PostalCode,
            City: target.City,
            Phone: target.Phone,
            Email: target.Email,
            InvoiceEmail: target.InvoiceEmail,
            PaymentMethod: target.PaymentMethod,
            Iban: target.Iban,
            Bic: target.Bic,
            AccountHolder: target.AccountHolder);
    }

    private static string CreateTempFilePath(string fileName)
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtBoxLicenseManagerTests", Guid.NewGuid().ToString("N"));
        return Path.Combine(folder, fileName);
    }
}
