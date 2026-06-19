using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using XdtBox.LicenseWeb.Services;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseWebTests
{
    [Fact]
    public void LicenseWebAuth_ShouldRequireConfiguredHashedAdminAndValidatePassword()
    {
        var hasher = new LicenseWebPasswordHasher();
        var hash = hasher.HashPassword("SehrGeheim123!");
        var options = CreateOptions(new LicenseWebOptions
        {
            Admin = new LicenseWebOptions.AdminOptions
            {
                Username = "admin",
                PasswordHash = hash.HashBase64,
                PasswordSalt = hash.SaltBase64
            }
        });
        var auth = new LicenseWebAuthService(hasher, options);

        Assert.True(auth.IsConfigured);
        Assert.True(auth.ValidateCredentials("admin", "SehrGeheim123!"));
        Assert.False(auth.ValidateCredentials("admin", "falsch"));
    }

    [Fact]
    public async Task LicenseWebDataStore_ShouldRejectDataRootInsideWebRoot()
    {
        var contentRoot = CreateTempFolder();
        var webRoot = Path.Combine(contentRoot, "wwwroot");
        Directory.CreateDirectory(webRoot);
        var store = new LicenseWebDataStore(
            new TestWebHostEnvironment(contentRoot, webRoot),
            CreateOptions(new LicenseWebOptions { DataRoot = Path.Combine(webRoot, "data") }));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => store.LoadSnapshotAsync());

        Assert.Contains("wwwroot", exception.Message);
    }

    [Fact]
    public async Task LicenseWeb_ShouldCreateSingleAndTotalPdfDownloadsWithoutPrivateKeyData()
    {
        var dataRoot = CreateTempFolder();
        var store = CreateStore(dataRoot);
        var paths = store.Paths;
        var customer = CreateCustomer("installation-web-1").WithLicense(CreateRecord("license-web-1") with
        {
            InstallationId = "installation-web-1",
            Devices = new[] { CreateDevice("TOPCON KR-800S", "Raum 2") }
        });
        new LicenseManagerCustomerRepository().Save(paths.CustomersFile, new[] { customer });
        new IssuedLicenseHistoryRepository().Save(paths.HistoryFile, new[]
        {
            CreateRecord("license-web-1") with
            {
                InstallationId = "installation-web-1",
                Devices = new[] { CreateDevice("TOPCON KR-800S", "Raum 2") }
            }
        });
        new LicenseManagerSettingsRepository().Save(paths.SettingsFile, LicenseManagerSettings.CreateDefault(dataRoot) with
        {
            PricePerDeviceNet = 12.5m,
            PrivateKeyPath = @"C:\nicht\sichern\private.pem"
        });
        var service = new LicenseWebLicenseService(store);

        var customerPdf = await service.CreateCustomerPdfAsync(customer.Id);
        var totalPdf = await service.CreateTotalPdfAsync();
        var customerText = System.Text.Encoding.Latin1.GetString(customerPdf.Content);
        var totalText = System.Text.Encoding.Latin1.GetString(totalPdf.Content);

        Assert.Equal("application/pdf", customerPdf.ContentType);
        Assert.Contains("XDTBox_Kunde_K-900_Lizenzuebersicht_", customerPdf.FileName);
        Assert.Contains("Praxis Müller Web", customerText);
        Assert.Contains("TOPCON KR-800S", customerText);
        Assert.Contains("Raum 2", customerText);
        Assert.Contains("Monatliche Summe netto: 12,50 EUR", customerText);
        Assert.Contains("XDTBox Kunden- und Lizenzübersicht", totalText);
        Assert.DoesNotContain("PRIVATE KEY", customerText);
        Assert.DoesNotContain("Signature", customerText);
        Assert.DoesNotContain("|", customerText);
    }

    [Fact]
    public async Task LicenseWeb_ShouldCreateBackupWithoutPrivateKeyAndFailLicenseCreationWithoutKey()
    {
        var dataRoot = CreateTempFolder();
        var store = CreateStore(dataRoot);
        var paths = store.Paths;
        new LicenseManagerCustomerRepository().Save(paths.CustomersFile, new[] { CreateCustomer("installation-web-1") });
        new LicenseManagerSettingsRepository().Save(paths.SettingsFile, LicenseManagerSettings.CreateDefault(dataRoot) with
        {
            PrivateKeyPath = @"C:\geheim\private.pem"
        });
        var requestFile = Path.Combine(paths.RequestsFolder, "request.json");
        new LicenseRequestFileRepository().Save(requestFile, CreateRequest());
        var service = new LicenseWebLicenseService(store);

        var backup = await store.CreateBackupAsync();
        var backupText = System.Text.Encoding.UTF8.GetString(backup);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateLicenseFromRequestAsync(requestFile));

        Assert.DoesNotContain("PrivateKeyPath", backupText);
        Assert.DoesNotContain("private.pem", backupText);
        Assert.Contains("kein serverseitiger Private Key", exception.Message);
    }

    [Fact]
    public async Task LicenseWeb_ShouldRejectUnsafePrivateKeyPath()
    {
        var contentRoot = CreateTempFolder();
        var webRoot = Path.Combine(contentRoot, "wwwroot");
        Directory.CreateDirectory(webRoot);
        var dataRoot = CreateTempFolder();
        var keyPath = Path.Combine(webRoot, "downloads", "private.pem");
        Directory.CreateDirectory(Path.GetDirectoryName(keyPath)!);
        await File.WriteAllTextAsync(keyPath, "PRIVATE KEY");
        var store = new LicenseWebDataStore(
            new TestWebHostEnvironment(contentRoot, webRoot),
            CreateOptions(new LicenseWebOptions { DataRoot = dataRoot, PrivateKeyPath = keyPath }));

        var snapshot = await store.LoadSnapshotAsync();
        var exception = Assert.Throws<InvalidOperationException>(() => store.GetPrivateKeyPathForSigning());
        var diagnostics = await store.CreateDiagnosticsAsync(isHttps: false, adminConfigured: true);

        Assert.False(snapshot.Runtime.PrivateKeyPathSafe);
        Assert.False(snapshot.Runtime.LicenseSignatureAvailable);
        Assert.Contains("unsicher", exception.Message);
        Assert.Contains(diagnostics, item => item.Name == "Private-Key-Pfad" && !item.IsOk);
    }

    [Fact]
    public async Task LicenseWeb_ShouldCreateLicenseWithServerSideTestKeyAndPersistHistory()
    {
        var dataRoot = CreateTempFolder();
        var keyRoot = CreateTempFolder();
        var keyPath = Path.Combine(keyRoot, "test-private.pem");
        using (var rsa = RSA.Create(2048))
        {
            await File.WriteAllTextAsync(keyPath, rsa.ExportPkcs8PrivateKeyPem());
        }

        var store = CreateStore(dataRoot, new LicenseWebOptions
        {
            DataRoot = dataRoot,
            PrivateKeyPath = keyPath,
            KeyId = "xdtbox-test-key",
            Issuer = "XDTBox Lizenzmanager Web Test",
            GraceDays = 9
        });
        var paths = store.Paths;
        Directory.CreateDirectory(paths.RequestsFolder);
        var requestFile = Path.Combine(paths.RequestsFolder, "license-request.json");
        new LicenseRequestFileRepository().Save(requestFile, CreateRequest() with
        {
            Devices = new[]
            {
                new LicenseRequestDevice(
                    Id: "kr800s-1",
                    Name: "TOPCON KR-800S",
                    Manufacturer: "TOPCON",
                    Model: "KR-800S",
                    ProfileId: "interface-topcon-kr800s",
                    IsActive: true,
                    IsLicenseRequired: true,
                    InterfaceProfileId: "interface-topcon-kr800s",
                    DisplayName: "MEDISTAR + TOPCON KR-800S",
                    DeviceProfileId: "device-topcon-kr800s-default",
                    DeviceDisplayName: "TOPCON KR-800S",
                    ConnectionKind: DeviceConnectionKind.NetworkLan,
                    Location: "Raum 7")
            }
        });
        var service = new LicenseWebLicenseService(store);

        var result = await service.CreateLicenseFromRequestAsync(requestFile);
        var envelope = new LicenseEnvelopeReader().ReadFile(result.OutputFile).Envelope;
        var history = new IssuedLicenseHistoryRepository().LoadOrEmpty(paths.HistoryFile);
        var customers = new LicenseManagerCustomerRepository().LoadOrEmpty(paths.CustomersFile);
        var licenseText = await File.ReadAllTextAsync(result.OutputFile);

        Assert.True(File.Exists(result.OutputFile));
        Assert.NotNull(envelope);
        Assert.Equal("xdtbox-test-key", envelope!.KeyId);
        Assert.Single(history);
        Assert.Equal(9, history[0].GraceDays);
        Assert.Equal("Raum 7", history[0].Devices.Single().Location);
        Assert.Single(customers);
        Assert.Equal("Raum 7", customers[0].EffectiveDevices.Single().Location);
        Assert.DoesNotContain(keyPath, licenseText);
        Assert.DoesNotContain("PRIVATE KEY", licenseText);
    }

    [Fact]
    public async Task LicenseWeb_ShouldImportOnlyRequestedCustomerFromSourceDataRoot()
    {
        var sourceRoot = CreateTempFolder();
        var targetRoot = CreateTempFolder();
        var sourcePaths = new LicenseManagerPathProvider().GetPaths(sourceRoot);
        var targetStore = CreateStore(targetRoot);
        var targetPaths = targetStore.Paths;
        var maxi = CreateCustomer("installation-10172") with
        {
            CustomerNumber = "10172",
            CustomerName = "Maxi Augenarzte",
            Devices = new[] { CreateDevice("TOPCON KR-800S", "Nuernberg EG") },
            ActiveLicensedDeviceCount = 1
        };
        var other = CreateCustomer("installation-other") with
        {
            CustomerNumber = "99999",
            CustomerName = "Andere Praxis"
        };
        var maxiHistory = CreateRecord("license-10172") with
        {
            CustomerNumber = "10172",
            CustomerName = "Maxi Augenarzte",
            InstallationId = "installation-10172",
            Devices = new[] { CreateDevice("TOPCON KR-800S", "Nuernberg EG") }
        };
        var otherHistory = CreateRecord("license-other") with
        {
            CustomerNumber = "99999",
            CustomerName = "Andere Praxis",
            InstallationId = "installation-other"
        };
        new LicenseManagerCustomerRepository().Save(sourcePaths.CustomersFile, new[] { maxi, other });
        new IssuedLicenseHistoryRepository().Save(sourcePaths.HistoryFile, new[] { maxiHistory, otherHistory });
        new LicenseManagerCustomerRepository().Save(targetPaths.CustomersFile, new[]
        {
            CreateCustomer("installation-alt") with
            {
                CustomerNumber = "10172",
                CustomerName = "Maxi Bestand"
            }
        });

        var result = await targetStore.ImportSingleCustomerFromDataRootAsync(sourceRoot, "10172");
        var targetCustomers = new LicenseManagerCustomerRepository().LoadOrEmpty(targetPaths.CustomersFile);
        var targetHistory = new IssuedLicenseHistoryRepository().LoadOrEmpty(targetPaths.HistoryFile);

        Assert.Equal("10172", result.Customer.CustomerNumber);
        Assert.Single(targetCustomers);
        Assert.Contains(targetCustomers[0].EffectiveInstallations, installation => installation.InstallationId == "installation-10172");
        Assert.Contains(targetCustomers[0].EffectiveInstallations, installation => installation.InstallationId == "installation-alt");
        Assert.DoesNotContain(targetCustomers, customer => customer.CustomerNumber == "99999");
        Assert.Single(targetHistory);
        Assert.Equal("license-10172", targetHistory[0].LicenseId);
        Assert.Equal("Nuernberg EG", targetHistory[0].Devices.Single().Location);
    }

    [Fact]
    public async Task LicenseWeb_ShouldUpdateCustomerDataLocationsAndPdf()
    {
        var dataRoot = CreateTempFolder();
        var store = CreateStore(dataRoot);
        var paths = store.Paths;
        var customer = CreateCustomer("installation-web-1");
        var device = customer.EffectiveDevices.Single();
        new LicenseManagerCustomerRepository().Save(paths.CustomersFile, new[] { customer });
        new IssuedLicenseHistoryRepository().Save(paths.HistoryFile, new[]
        {
            CreateRecord("license-web-1") with
            {
                InstallationId = "installation-web-1",
                Devices = new[] { device }
            }
        });

        var updated = await store.UpdateCustomerAsync(customer.Id, new LicenseWebCustomerUpdate(
            CustomerNumber: "K-901",
            CustomerName: "Praxis Neu Web",
            Street: "Neue Strasse 2",
            PostalCode: "54321",
            City: "Bonn",
            Phone: "0228",
            Email: "neu@example.test",
            ContactPerson: "Herr Neu",
            InvoiceEmail: "rechnung-neu@example.test",
            Iban: "DE00999999990000000000",
            Bic: "NEUDEFFXXX",
            AccountHolder: "Praxis Neu Web",
            PaymentMethod: LicenseManagerPaymentMethod.SepaDirectDebit,
            DeviceLocations: new[]
            {
                new LicenseWebCustomerDeviceLocationUpdate(
                    LicenseWebDataStore.CreateDeviceLocationKey("installation-web-1", device),
                    "OP 1")
            }));
        var snapshot = await store.LoadSnapshotAsync();
        var service = new LicenseWebLicenseService(store);
        var customerPdf = await service.CreateCustomerPdfAsync(updated.Id);
        var customerText = System.Text.Encoding.Latin1.GetString(customerPdf.Content);

        Assert.Equal("K-901", snapshot.Customers.Single().CustomerNumber);
        Assert.Equal("Praxis Neu Web", snapshot.Customers.Single().CustomerName);
        Assert.Equal("OP 1", snapshot.Customers.Single().EffectiveDevices.Single().Location);
        Assert.Equal("OP 1", snapshot.History.Single().Devices.Single().Location);
        Assert.Contains("Praxis Neu Web", customerText);
        Assert.Contains("OP 1", customerText);
    }

    [Fact]
    public async Task LicenseWeb_ShouldDeleteCustomerAndRelatedHistory()
    {
        var dataRoot = CreateTempFolder();
        var store = CreateStore(dataRoot);
        var paths = store.Paths;
        var customer = CreateCustomer("installation-web-1");
        new LicenseManagerCustomerRepository().Save(paths.CustomersFile, new[] { customer });
        new IssuedLicenseHistoryRepository().Save(paths.HistoryFile, new[] { CreateRecord("license-web-1") });

        var deleted = await store.DeleteCustomerAsync(customer.Id);
        var snapshot = await store.LoadSnapshotAsync();

        Assert.Equal(customer.Id, deleted.Id);
        Assert.Empty(snapshot.Customers);
        Assert.Empty(snapshot.History);
    }

    [Fact]
    public async Task LicenseWeb_ShouldMergeSelectedCustomersIntoFirstTarget()
    {
        var dataRoot = CreateTempFolder();
        var store = CreateStore(dataRoot);
        var paths = store.Paths;
        var target = CreateCustomer("installation-web-1") with
        {
            CustomerNumber = "K-900",
            CustomerName = "Zielpraxis"
        };
        var source = CreateCustomer("installation-web-2") with
        {
            Id = Guid.NewGuid().ToString("N"),
            CustomerNumber = "K-901",
            CustomerName = "Quellpraxis",
            Devices = new[] { CreateDevice("TOPCON KR-800S", "Raum 3") },
            ActiveLicensedDeviceCount = 1
        };
        new LicenseManagerCustomerRepository().Save(paths.CustomersFile, new[] { target, source });

        var result = await store.MergeCustomersAsync(new[] { target.Id, source.Id });
        var snapshot = await store.LoadSnapshotAsync();

        Assert.Equal("Zielpraxis", result.TargetCustomer.CustomerName);
        Assert.Single(snapshot.Customers);
        Assert.Equal("Zielpraxis", snapshot.Customers.Single().CustomerName);
        Assert.Equal(2, snapshot.Customers.Single().EffectiveInstallations.Count);
        Assert.Contains(source.Id, result.RemovedCustomerIds);
    }

    [Fact]
    public async Task LicenseWebAuth_ShouldPersistChangedAdminPasswordInDataRoot()
    {
        var dataRoot = CreateTempFolder();
        var store = CreateStore(dataRoot);
        var hasher = new LicenseWebPasswordHasher();
        var hash = hasher.HashPassword("StartPasswort123!");
        var options = CreateOptions(new LicenseWebOptions
        {
            DataRoot = dataRoot,
            Admin = new LicenseWebOptions.AdminOptions
            {
                Username = "admin",
                PasswordHash = hash.HashBase64,
                PasswordSalt = hash.SaltBase64
            }
        });
        var auth = new LicenseWebAuthService(hasher, options, store);

        await auth.ChangePasswordAsync("StartPasswort123!", "NeuesPasswort123!", "NeuesPasswort123!");

        Assert.False(auth.ValidateCredentials("admin", "StartPasswort123!"));
        Assert.True(auth.ValidateCredentials("admin", "NeuesPasswort123!"));
        Assert.NotNull(store.LoadAdminCredentialOverride());
    }

    [Fact]
    public void LicenseWebSources_ShouldKeepSecretsServerSideAndProtectPages()
    {
        var webRootFiles = Directory.GetFiles(FindWorkspaceFile("XdtBox.LicenseWeb", "wwwroot"), "*", SearchOption.AllDirectories);
        Assert.DoesNotContain(webRootFiles, file => file.EndsWith(".pem", StringComparison.OrdinalIgnoreCase));

        var program = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseWeb", "Program.cs"));
        var layout = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseWeb", "Pages", "Shared", "_Layout.cshtml"));
        var login = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseWeb", "Pages", "Account", "Login.cshtml.cs"));
        var store = File.ReadAllText(FindWorkspaceFile("XdtBox.LicenseWeb", "Services", "LicenseWebDataStore.cs"));
        var backup = File.ReadAllText(FindWorkspaceFile("XdtDeviceBridge.Infrastructure", "LicenseManagerBackupService.cs"));

        Assert.Contains("AddCookie", program);
        Assert.Contains("AuthorizeFolder", program);
        Assert.Contains("AllowAnonymousToPage(\"/Account/Login\")", program);
        Assert.Contains("SignInAsync", login);
        Assert.Contains("Dashboard", layout);
        Assert.Contains("Kunden", layout);
        Assert.Contains("Lizenzanfrage importieren", layout);
        Assert.Contains("Backup", layout);
        Assert.Contains("Einstellungen", layout);
        Assert.Contains("Der LicenseWeb DataRoot darf nicht im {label} liegen", store);
        Assert.Contains("safeSettings = settings with { PrivateKeyPath = null }", backup);
    }

    private static LicenseWebDataStore CreateStore(string dataRoot, LicenseWebOptions? options = null)
    {
        var contentRoot = CreateTempFolder();
        var webRoot = Path.Combine(contentRoot, "wwwroot");
        Directory.CreateDirectory(webRoot);
        return new LicenseWebDataStore(
            new TestWebHostEnvironment(contentRoot, webRoot),
            CreateOptions(options ?? new LicenseWebOptions { DataRoot = dataRoot }));
    }

    private static TestOptionsMonitor<LicenseWebOptions> CreateOptions(LicenseWebOptions options)
    {
        return new TestOptionsMonitor<LicenseWebOptions>(options);
    }

    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), "XdtBoxLicenseWebTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static LicenseManagerCustomerRecord CreateCustomer(string installationId)
    {
        return new LicenseManagerCustomerRecord(
            Id: Guid.NewGuid().ToString("N"),
            CustomerNumber: "K-900",
            CustomerName: "Praxis Müller Web",
            Street: "Teststraße 1",
            PostalCode: "12345",
            City: "Köln",
            Phone: "0221",
            Email: "info@example.test",
            ContactPerson: "Frau Web",
            InvoiceEmail: "rechnung@example.test",
            Iban: "DE00123456780000000000",
            Bic: "TESTDEFFXXX",
            AccountHolder: "Praxis Müller Web",
            SepaDirectDebitConsent: false,
            AlwaysInvoice: true,
            InstallationId: installationId,
            MachineName: "WEB-PC",
            ActiveLicensedDeviceCount: 1,
            Devices: new[] { CreateDevice("TOPCON KR-800S", "Raum 2") },
            UpdatedAtUtc: DateTime.UtcNow);
    }

    private static IssuedLicenseRecord CreateRecord(string licenseId)
    {
        var now = new DateTime(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);
        return new IssuedLicenseRecord(
            LicenseId: licenseId,
            IssuedAtUtc: now,
            LicenseeName: "Praxis Müller Web",
            CustomerNumber: "K-900",
            CustomerName: "Praxis Müller Web",
            Street: "Teststraße 1",
            PostalCode: "12345",
            City: "Köln",
            Phone: "0221",
            Email: "info@example.test",
            ContactPerson: "Frau Web",
            InstallationId: "installation-web-1",
            MaxActiveDeviceConnections: 1,
            ValidFromUtc: now.Date,
            ValidUntilUtc: XdtBoxLicenseConstants.UnlimitedValidUntilUtc,
            GraceDays: 7,
            LicenseType: "Production",
            KeyId: LicensePublicKeyProvider.ProductionKeyId,
            OutputFilePath: @"C:\XDTBox\Lizenzaktivierung\licenses\web.xdtboxlic",
            RequestFilePath: null,
            Notes: null,
            Devices: new[] { CreateDevice("TOPCON KR-800S", "Raum 2") },
            MachineName: "WEB-PC",
            InvoiceEmail: "rechnung@example.test",
            Iban: "DE00123456780000000000",
            Bic: "TESTDEFFXXX",
            AccountHolder: "Praxis Müller Web",
            SepaDirectDebitConsent: false,
            AlwaysInvoice: true);
    }

    private static IssuedLicenseDeviceRecord CreateDevice(string name, string location)
    {
        return new IssuedLicenseDeviceRecord(
            DisplayName: "MEDISTAR + " + name,
            DeviceDisplayName: name,
            InterfaceProfileId: "interface-" + name.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal),
            DeviceProfileId: "device-" + name.ToLowerInvariant().Replace(" ", "-", StringComparison.Ordinal),
            ConnectionKind: DeviceConnectionKind.NetworkLan,
            Location: location);
    }

    private static LicenseRequest CreateRequest()
    {
        return new LicenseRequest(
            RequestId: Guid.NewGuid().ToString("N"),
            InstallationId: "installation-web-1",
            MachineName: "WEB-PC",
            UserName: "Technik",
            IsTerminalServer: false,
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            AppVersion: "1.12",
            ActiveLicensedDeviceCount: 1,
            Devices: Array.Empty<LicenseRequestDevice>(),
            CreatedAt: DateTime.UtcNow,
            Customer: new LicenseRequestCustomer(
                CustomerName: "Praxis Müller Web",
                Street: "Teststraße 1",
                PostalCode: "12345",
                City: "Köln",
                Phone: "0221",
                Email: "info@example.test",
                ContactPerson: "Frau Web",
                Iban: null,
                Bic: null,
                AccountHolder: null,
                SepaDirectDebitConsent: false,
                AlwaysInvoice: true,
                InvoiceEmail: "rechnung@example.test",
                CustomerNumber: "K-900"));
    }

    private static string FindWorkspaceFile(params string[] segments)
    {
        var root = AppContext.BaseDirectory;
        while (!string.IsNullOrWhiteSpace(root))
        {
            var candidate = Path.Combine(new[] { root }.Concat(segments).ToArray());
            if (File.Exists(candidate) || Directory.Exists(candidate))
            {
                return candidate;
            }

            root = Directory.GetParent(root)?.FullName;
        }

        throw new FileNotFoundException("Workspace path not found: " + string.Join("\\", segments));
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public TestOptionsMonitor(T value)
        {
            CurrentValue = value;
        }

        public T CurrentValue { get; }
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public TestWebHostEnvironment(string contentRoot, string webRoot)
        {
            ContentRootPath = contentRoot;
            WebRootPath = webRoot;
            ContentRootFileProvider = new PhysicalFileProvider(contentRoot);
            WebRootFileProvider = new PhysicalFileProvider(webRoot);
        }

        public string ApplicationName { get; set; } = "XdtBox.LicenseWeb.Tests";
        public IFileProvider WebRootFileProvider { get; set; }
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}
