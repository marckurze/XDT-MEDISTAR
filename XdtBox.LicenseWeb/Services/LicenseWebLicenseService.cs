using XdtBox.LicenseIssuer;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebLicenseService
{
    private readonly LicenseWebDataStore _store;
    private readonly LicenseIssuerService _issuerService = new();
    private readonly LicenseManagerCustomerPdfExporter _pdfExporter = new();

    public LicenseWebLicenseService(LicenseWebDataStore store)
    {
        _store = store;
    }

    public async Task<DownloadFile> CreateCustomerPdfAsync(string customerId)
    {
        var snapshot = await _store.LoadSnapshotAsync().ConfigureAwait(false);
        var customer = FindCustomer(snapshot.Customers, customerId);
        var fileName = LicenseManagerCustomerPdfExporter.CreateSuggestedCustomerPdfFileName(customer, DateTime.Now);
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
        _pdfExporter.ExportCustomer(tempFile, customer, snapshot.History, snapshot.Settings.PricePerDeviceNet, DateTime.Now);
        return new DownloadFile(fileName, "application/pdf", await File.ReadAllBytesAsync(tempFile).ConfigureAwait(false));
    }

    public async Task<DownloadFile> CreateTotalPdfAsync()
    {
        var snapshot = await _store.LoadSnapshotAsync().ConfigureAwait(false);
        var fileName = $"XDTBox_Kunden_Lizenzuebersicht_{DateTime.Today:yyyyMMdd}.pdf";
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".pdf");
        _pdfExporter.ExportCustomers(tempFile, snapshot.Customers, snapshot.Settings.PricePerDeviceNet, DateTime.Now);
        return new DownloadFile(fileName, "application/pdf", await File.ReadAllBytesAsync(tempFile).ConfigureAwait(false));
    }

    public async Task<LicenseWebCreateLicenseResult> CreateLicenseFromRequestAsync(string requestFile)
    {
        var privateKeyPath = _store.ResolvePrivateKeyPath();
        if (string.IsNullOrWhiteSpace(privateKeyPath) || !File.Exists(privateKeyPath))
        {
            throw new InvalidOperationException("Lizenz-Erstellung ist deaktiviert, weil kein serverseitiger Private Key konfiguriert ist.");
        }

        var snapshot = await _store.LoadSnapshotAsync().ConfigureAwait(false);
        var request = new LicenseRequestFileRepository().Load(requestFile);
        var customer = LicenseManagerCustomerRecord.FromRequest(request);
        var paths = _store.Paths;
        Directory.CreateDirectory(paths.LicensesFolder);
        var outputFile = Path.Combine(paths.LicensesFolder, $"xdtbox-license-{request.InstallationId}-{DateTime.UtcNow:yyyyMMddHHmmss}.xdtboxlic");
        var options = new LicenseIssuerOptions(
            RequestFile: requestFile,
            InstallationId: null,
            LicenseeName: request.Customer?.CustomerName ?? request.InstallationId,
            CustomerNumber: request.Customer?.CustomerNumber,
            MaxActiveDeviceConnections: Math.Max(1, request.ActiveLicensedDeviceCount),
            ValidFromUtc: DateTime.UtcNow.Date,
            ValidUntilUtc: XdtBoxLicenseConstants.UnlimitedValidUntilUtc,
            GraceDays: snapshot.Settings.DefaultGraceDays,
            LicenseType: "Production",
            Issuer: snapshot.Settings.DefaultIssuer,
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            Notes: "Erstellt durch XDTBox Lizenzmanager Web",
            KeyId: snapshot.Settings.KeyId,
            PrivateKeyPath: privateKeyPath,
            OutputFile: outputFile);
        var result = _issuerService.CreateLicense(options);
        var record = CreateHistoryRecord(result, options, request);
        await _store.AddHistoryAsync(record, customer).ConfigureAwait(false);
        return new LicenseWebCreateLicenseResult(result.OutputFile, Path.GetFileName(result.OutputFile), record);
    }

    private static IssuedLicenseRecord CreateHistoryRecord(
        LicenseIssuerResult result,
        LicenseIssuerOptions options,
        LicenseRequest request)
    {
        var customer = request.Customer ?? LicenseRequestCustomer.Empty;
        var devices = CreateDeviceRecords(request.Devices);
        return new IssuedLicenseRecord(
            LicenseId: result.Payload.LicenseId,
            IssuedAtUtc: result.Payload.IssuedAtUtc,
            LicenseeName: result.Payload.LicenseeName,
            CustomerNumber: result.Payload.CustomerNumber,
            CustomerName: customer.CustomerName,
            Street: customer.Street,
            PostalCode: customer.PostalCode,
            City: customer.City,
            Phone: customer.Phone,
            Email: customer.Email,
            ContactPerson: customer.ContactPerson,
            InstallationId: result.Payload.InstallationId,
            MaxActiveDeviceConnections: result.Payload.MaxActiveDeviceConnections,
            ValidFromUtc: result.Payload.ValidFromUtc,
            ValidUntilUtc: result.Payload.ValidUntilUtc,
            GraceDays: result.Payload.GraceDays,
            LicenseType: result.Payload.LicenseType,
            KeyId: options.KeyId,
            OutputFilePath: result.OutputFile,
            RequestFilePath: options.RequestFile,
            Notes: result.Payload.Notes,
            Devices: devices,
            MachineName: request.MachineName,
            InvoiceEmail: customer.InvoiceEmail,
            Iban: customer.Iban,
            Bic: customer.Bic,
            AccountHolder: customer.AccountHolder,
            SepaDirectDebitConsent: customer.SepaDirectDebitConsent,
            AlwaysInvoice: customer.AlwaysInvoice);
    }

    private static IReadOnlyList<IssuedLicenseDeviceRecord> CreateDeviceRecords(IEnumerable<LicenseRequestDevice> devices)
    {
        return devices
            .Where(device => device.IsActive && device.IsLicenseRequired)
            .Select(device => new IssuedLicenseDeviceRecord(
                DisplayName: string.IsNullOrWhiteSpace(device.DisplayName) ? device.Name : device.DisplayName,
                DeviceDisplayName: string.IsNullOrWhiteSpace(device.DeviceDisplayName) ? device.Model : device.DeviceDisplayName,
                InterfaceProfileId: string.IsNullOrWhiteSpace(device.InterfaceProfileId) ? device.ProfileId : device.InterfaceProfileId,
                DeviceProfileId: device.DeviceProfileId,
                ConnectionKind: device.ConnectionKind,
                Location: string.IsNullOrWhiteSpace(device.Location) ? null : device.Location.Trim()))
            .ToArray();
    }

    private static LicenseManagerCustomerRecord FindCustomer(IReadOnlyList<LicenseManagerCustomerRecord> customers, string customerId)
    {
        return customers.FirstOrDefault(customer => string.Equals(customer.Id, customerId, StringComparison.Ordinal))
            ?? throw new InvalidOperationException("Kunde wurde nicht gefunden.");
    }
}

public sealed record DownloadFile(string FileName, string ContentType, byte[] Content);
