namespace XdtDeviceBridge.Core;

public sealed record LicenseManagerCustomerRecord(
    string Id,
    string? CustomerNumber,
    string CustomerName,
    string Street,
    string PostalCode,
    string City,
    string Phone,
    string? Email,
    string? ContactPerson,
    string? InvoiceEmail,
    string? Iban,
    string? Bic,
    string? AccountHolder,
    bool SepaDirectDebitConsent,
    bool AlwaysInvoice,
    string InstallationId,
    string? MachineName,
    int ActiveLicensedDeviceCount,
    IReadOnlyList<IssuedLicenseDeviceRecord> Devices,
    DateTime UpdatedAtUtc,
    DateTime? LastLicenseIssuedAtUtc = null,
    DateTime? LicenseValidUntilUtc = null,
    string? LastLicenseFilePath = null)
{
    public static LicenseManagerCustomerRecord FromRequest(LicenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var customer = request.Customer ?? LicenseRequestCustomer.Empty;
        var devices = request.Devices
            .Where(device => device.IsActive && device.IsLicenseRequired)
            .Select(device => new IssuedLicenseDeviceRecord(
                DisplayName: string.IsNullOrWhiteSpace(device.DisplayName) ? device.Name : device.DisplayName,
                DeviceDisplayName: string.IsNullOrWhiteSpace(device.DeviceDisplayName) ? device.Model : device.DeviceDisplayName,
                InterfaceProfileId: string.IsNullOrWhiteSpace(device.InterfaceProfileId) ? device.ProfileId : device.InterfaceProfileId,
                DeviceProfileId: device.DeviceProfileId,
                ConnectionKind: device.ConnectionKind))
            .ToArray();

        return new LicenseManagerCustomerRecord(
            Id: Guid.NewGuid().ToString("N"),
            CustomerNumber: Normalize(customer.CustomerNumber),
            CustomerName: customer.CustomerName,
            Street: customer.Street,
            PostalCode: customer.PostalCode,
            City: customer.City,
            Phone: customer.Phone,
            Email: Normalize(customer.Email),
            ContactPerson: Normalize(customer.ContactPerson),
            InvoiceEmail: Normalize(customer.InvoiceEmail),
            Iban: Normalize(customer.Iban),
            Bic: Normalize(customer.Bic),
            AccountHolder: Normalize(customer.AccountHolder),
            SepaDirectDebitConsent: customer.SepaDirectDebitConsent,
            AlwaysInvoice: customer.AlwaysInvoice,
            InstallationId: request.InstallationId,
            MachineName: Normalize(request.MachineName),
            ActiveLicensedDeviceCount: request.ActiveLicensedDeviceCount,
            Devices: devices,
            UpdatedAtUtc: DateTime.UtcNow);
    }

    public LicenseManagerCustomerRecord WithLicense(IssuedLicenseRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        return this with
        {
            CustomerNumber = Normalize(record.CustomerNumber) ?? CustomerNumber,
            CustomerName = string.IsNullOrWhiteSpace(record.CustomerName) ? CustomerName : record.CustomerName,
            Street = record.Street,
            PostalCode = record.PostalCode,
            City = record.City,
            Phone = record.Phone,
            Email = Normalize(record.Email),
            ContactPerson = Normalize(record.ContactPerson),
            InvoiceEmail = Normalize(record.InvoiceEmail) ?? InvoiceEmail,
            Iban = Normalize(record.Iban) ?? Iban,
            Bic = Normalize(record.Bic) ?? Bic,
            AccountHolder = Normalize(record.AccountHolder) ?? AccountHolder,
            SepaDirectDebitConsent = record.SepaDirectDebitConsent,
            AlwaysInvoice = record.AlwaysInvoice,
            InstallationId = record.InstallationId,
            MachineName = Normalize(record.MachineName) ?? MachineName,
            ActiveLicensedDeviceCount = record.MaxActiveDeviceConnections,
            Devices = record.Devices,
            UpdatedAtUtc = DateTime.UtcNow,
            LastLicenseIssuedAtUtc = record.IssuedAtUtc,
            LicenseValidUntilUtc = record.ValidUntilUtc,
            LastLicenseFilePath = record.OutputFilePath
        };
    }

    public LicenseManagerCustomerRecord MergeImported(LicenseManagerCustomerRecord incoming)
    {
        ArgumentNullException.ThrowIfNull(incoming);

        return incoming with
        {
            Id = Id,
            CustomerNumber = Normalize(incoming.CustomerNumber) ?? CustomerNumber,
            LastLicenseIssuedAtUtc = LastLicenseIssuedAtUtc,
            LicenseValidUntilUtc = LicenseValidUntilUtc,
            LastLicenseFilePath = LastLicenseFilePath,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
