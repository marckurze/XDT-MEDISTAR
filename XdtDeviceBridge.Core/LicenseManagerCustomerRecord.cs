using System.Text.Json.Serialization;

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
    string? LastLicenseFilePath = null,
    IReadOnlyList<LicenseManagerInstallationRecord>? Installations = null)
{
    [JsonIgnore]
    public LicenseManagerPaymentMethod PaymentMethod =>
        SepaDirectDebitConsent && !AlwaysInvoice
            ? LicenseManagerPaymentMethod.SepaDirectDebit
            : LicenseManagerPaymentMethod.BankTransfer;

    [JsonIgnore]
    public IReadOnlyList<LicenseManagerInstallationRecord> EffectiveInstallations => NormalizeInstallations();

    [JsonIgnore]
    public IReadOnlyList<LicenseManagerInstallationRecord> ActiveInstallations =>
        EffectiveInstallations
            .Where(installation => installation.IsActive)
            .ToArray();

    [JsonIgnore]
    public int BillableDeviceCount => ActiveInstallations.Sum(installation => installation.BillableDeviceCount);

    [JsonIgnore]
    public int ActiveInstallationCount => ActiveInstallations.Count;

    [JsonIgnore]
    public IReadOnlyList<IssuedLicenseDeviceRecord> EffectiveDevices =>
        ActiveInstallations
            .SelectMany(installation => installation.Devices)
            .ToArray();

    [JsonIgnore]
    public DateTime? EffectiveLastLicenseIssuedAtUtc =>
        ActiveInstallations
            .Select(installation => installation.LastLicenseIssuedAtUtc)
            .Where(value => value.HasValue)
            .DefaultIfEmpty(LastLicenseIssuedAtUtc)
            .Max();

    [JsonIgnore]
    public DateTime? EffectiveLicenseValidUntilUtc =>
        ActiveInstallations
            .OrderByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
            .Select(installation => installation.LicenseValidUntilUtc)
            .FirstOrDefault(value => value.HasValue)
        ?? LicenseValidUntilUtc;

    [JsonIgnore]
    public string? EffectiveLastLicenseFilePath =>
        ActiveInstallations
            .OrderByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
            .Select(installation => installation.LastLicenseFilePath)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
        ?? LastLicenseFilePath;

    public static LicenseManagerCustomerRecord FromRequest(LicenseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var customer = request.Customer ?? LicenseRequestCustomer.Empty;
        var installation = LicenseManagerInstallationRecord.FromRequest(request);

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
            SepaDirectDebitConsent: customer.SepaDirectDebitConsent && !customer.AlwaysInvoice,
            AlwaysInvoice: customer.AlwaysInvoice || !customer.SepaDirectDebitConsent,
            InstallationId: request.InstallationId,
            MachineName: Normalize(request.MachineName),
            ActiveLicensedDeviceCount: installation.ActiveLicensedDeviceCount,
            Devices: installation.Devices,
            UpdatedAtUtc: DateTime.UtcNow,
            Installations: new[] { installation });
    }

    public LicenseManagerCustomerRecord WithLicense(IssuedLicenseRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        var installation = LicenseManagerInstallationRecord.FromLicense(record);
        return (WithInstallation(installation) with
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
            SepaDirectDebitConsent = record.SepaDirectDebitConsent && !record.AlwaysInvoice,
            AlwaysInvoice = record.AlwaysInvoice || !record.SepaDirectDebitConsent,
            UpdatedAtUtc = DateTime.UtcNow
        }).WithCompatibilitySnapshot(installation);
    }

    public LicenseManagerCustomerRecord MergeImported(LicenseManagerCustomerRecord incoming)
    {
        ArgumentNullException.ThrowIfNull(incoming);

        var merged = this with
        {
            CustomerNumber = Normalize(incoming.CustomerNumber) ?? CustomerNumber,
            CustomerName = string.IsNullOrWhiteSpace(incoming.CustomerName) ? CustomerName : incoming.CustomerName,
            Street = incoming.Street,
            PostalCode = incoming.PostalCode,
            City = incoming.City,
            Phone = incoming.Phone,
            Email = Normalize(incoming.Email),
            ContactPerson = Normalize(incoming.ContactPerson),
            InvoiceEmail = Normalize(incoming.InvoiceEmail) ?? InvoiceEmail,
            Iban = Normalize(incoming.Iban) ?? Iban,
            Bic = Normalize(incoming.Bic) ?? Bic,
            AccountHolder = Normalize(incoming.AccountHolder) ?? AccountHolder,
            SepaDirectDebitConsent = incoming.SepaDirectDebitConsent && !incoming.AlwaysInvoice,
            AlwaysInvoice = incoming.AlwaysInvoice || !incoming.SepaDirectDebitConsent,
            UpdatedAtUtc = DateTime.UtcNow
        };

        foreach (var installation in incoming.EffectiveInstallations)
        {
            merged = merged.WithInstallation(installation);
        }

        return merged.WithCompatibilitySnapshot(merged.SelectPreferredInstallation());
    }

    public LicenseManagerCustomerRecord CancelInstallation(string installationId, DateTime cancelledAtUtc, string? note)
    {
        if (string.IsNullOrWhiteSpace(installationId))
        {
            return this;
        }

        var installations = EffectiveInstallations
            .Select(installation => string.Equals(installation.InstallationId, installationId, StringComparison.OrdinalIgnoreCase)
                ? installation.Cancel(cancelledAtUtc, note)
                : installation)
            .ToArray();

        var preferred = installations.FirstOrDefault(installation => installation.IsActive) ?? installations.FirstOrDefault();
        return (this with
        {
            Installations = installations,
            UpdatedAtUtc = DateTime.UtcNow
        }).WithCompatibilitySnapshot(preferred);
    }

    public LicenseManagerCustomerRecord WithNormalizedInstallations()
    {
        var installations = EffectiveInstallations;
        return (this with { Installations = installations }).WithCompatibilitySnapshot(SelectPreferredInstallation(installations));
    }

    private LicenseManagerCustomerRecord WithInstallation(LicenseManagerInstallationRecord installation)
    {
        var installations = EffectiveInstallations.ToList();
        var index = installations.FindIndex(existing =>
            string.Equals(existing.InstallationId, installation.InstallationId, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            installations[index] = MergeInstallations(installations[index], installation);
        }
        else
        {
            installations.Add(installation);
        }

        return this with { Installations = installations.ToArray() };
    }

    private LicenseManagerCustomerRecord WithCompatibilitySnapshot(LicenseManagerInstallationRecord? preferred)
    {
        preferred ??= SelectPreferredInstallation();

        return this with
        {
            InstallationId = preferred?.InstallationId ?? string.Empty,
            MachineName = preferred?.MachineName,
            ActiveLicensedDeviceCount = BillableDeviceCount,
            Devices = ActiveInstallations.SelectMany(installation => installation.Devices).ToArray(),
            LastLicenseIssuedAtUtc = EffectiveLastLicenseIssuedAtUtc,
            LicenseValidUntilUtc = EffectiveLicenseValidUntilUtc,
            LastLicenseFilePath = EffectiveLastLicenseFilePath
        };
    }

    private LicenseManagerInstallationRecord? SelectPreferredInstallation()
    {
        return SelectPreferredInstallation(EffectiveInstallations);
    }

    private static LicenseManagerInstallationRecord? SelectPreferredInstallation(IReadOnlyList<LicenseManagerInstallationRecord> installations)
    {
        return installations
            .OrderByDescending(installation => installation.IsActive)
            .ThenByDescending(installation => installation.LastLicenseIssuedAtUtc ?? DateTime.MinValue)
            .FirstOrDefault();
    }

    private IReadOnlyList<LicenseManagerInstallationRecord> NormalizeInstallations()
    {
        if (Installations is not null)
        {
            return Installations.ToArray();
        }

        if (string.IsNullOrWhiteSpace(InstallationId))
        {
            return Array.Empty<LicenseManagerInstallationRecord>();
        }

        return new[]
        {
            new LicenseManagerInstallationRecord(
                InstallationId: InstallationId,
                MachineName: MachineName,
                ActiveLicensedDeviceCount: ActiveLicensedDeviceCount,
                Devices: Devices,
                LastLicenseIssuedAtUtc: LastLicenseIssuedAtUtc,
                LicenseValidUntilUtc: LicenseValidUntilUtc,
                LastLicenseFilePath: LastLicenseFilePath)
        };
    }

    private static LicenseManagerInstallationRecord MergeInstallations(
        LicenseManagerInstallationRecord existing,
        LicenseManagerInstallationRecord incoming)
    {
        if (existing.Status == LicenseManagerInstallationStatus.Cancelled
            && incoming.LastLicenseIssuedAtUtc is null)
        {
            return existing;
        }

        return incoming with
        {
            LastLicenseIssuedAtUtc = incoming.LastLicenseIssuedAtUtc ?? existing.LastLicenseIssuedAtUtc,
            LicenseValidUntilUtc = incoming.LicenseValidUntilUtc ?? existing.LicenseValidUntilUtc,
            LastLicenseFilePath = Normalize(incoming.LastLicenseFilePath) ?? existing.LastLicenseFilePath,
            Status = incoming.Status,
            CancelledAtUtc = incoming.CancelledAtUtc,
            CancellationNote = incoming.CancellationNote
        };
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
