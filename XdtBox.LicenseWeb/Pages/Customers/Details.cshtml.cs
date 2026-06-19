using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseWeb.Pages.Customers;

public sealed class DetailsModel : PageModel
{
    private readonly LicenseWebDataStore _store;
    private readonly LicenseWebLicenseService _licenseService;

    public DetailsModel(LicenseWebDataStore store, LicenseWebLicenseService licenseService)
    {
        _store = store;
        _licenseService = licenseService;
    }

    public CustomerDetail Customer { get; private set; } = CustomerDetail.Empty;
    public IReadOnlyList<InstallationRow> Installations { get; private set; } = Array.Empty<InstallationRow>();
    public IReadOnlyList<DeviceRow> Devices { get; private set; } = Array.Empty<DeviceRow>();
    public IReadOnlyList<HistoryRow> History { get; private set; } = Array.Empty<HistoryRow>();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var snapshot = await _store.LoadSnapshotAsync();
        var customer = snapshot.Customers.FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.Ordinal));
        if (customer is null)
        {
            return NotFound();
        }

        Customer = CustomerDetail.From(customer, snapshot.Settings.PricePerDeviceNet);
        Installations = customer.EffectiveInstallations.Select(InstallationRow.From).ToArray();
        Devices = customer.EffectiveDevices.Select(DeviceRow.From).ToArray();
        var installationIds = customer.EffectiveInstallations.Select(installation => installation.InstallationId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        History = snapshot.History
            .Where(record => installationIds.Contains(record.InstallationId)
                || (!string.IsNullOrWhiteSpace(customer.CustomerNumber)
                    && string.Equals(record.CustomerNumber, customer.CustomerNumber, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(record => record.IssuedAtUtc)
            .Select(HistoryRow.From)
            .ToArray();
        return Page();
    }

    public async Task<IActionResult> OnGetPdfAsync(string id)
    {
        var download = await _licenseService.CreateCustomerPdfAsync(id);
        return File(download.Content, download.ContentType, download.FileName);
    }

    private static string FormatValidity(DateTime? value)
    {
        return XdtBoxLicenseConstants.IsUnlimitedValidUntil(value)
            ? "unbefristet"
            : value?.ToLocalTime().ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE")) ?? "-";
    }

    private static string JoinLocations(IEnumerable<IssuedLicenseDeviceRecord> devices)
    {
        var values = devices.Select(device => device.Location).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct().ToArray();
        return values.Length == 0 ? "-" : string.Join("; ", values);
    }

    public sealed record CustomerDetail(
        string Id,
        string CustomerName,
        string CustomerNumber,
        string Address,
        string Phone,
        string Email,
        string ContactPerson,
        string InvoiceEmail,
        string PaymentMethod,
        string Iban,
        string MonthlyTotal)
    {
        public static readonly CustomerDetail Empty = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        public static CustomerDetail From(LicenseManagerCustomerRecord customer, decimal price)
        {
            var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, price);
            return new CustomerDetail(
                customer.Id,
                customer.CustomerName,
                customer.CustomerNumber ?? "-",
                $"{customer.Street}, {customer.PostalCode} {customer.City}",
                customer.Phone,
                customer.Email ?? "-",
                customer.ContactPerson ?? "-",
                customer.InvoiceEmail ?? customer.Email ?? "-",
                customer.PaymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit ? "SEPA-Lastschrift" : "Banküberweisung",
                customer.Iban ?? "-",
                total.ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " EUR");
        }
    }

    public sealed record InstallationRow(string Status, string InstallationId, string MachineName, string Locations, int DeviceCount, string ValidUntil)
    {
        public static InstallationRow From(LicenseManagerInstallationRecord installation)
        {
            return new InstallationRow(
                installation.IsActive ? "Aktiv" : "Storniert",
                installation.InstallationId,
                installation.MachineName ?? "-",
                JoinLocations(installation.Devices),
                installation.BillableDeviceCount,
                FormatValidity(installation.LicenseValidUntilUtc));
        }
    }

    public sealed record DeviceRow(string DisplayName, string DeviceDisplayName, string Location, string ConnectionKind)
    {
        public static DeviceRow From(IssuedLicenseDeviceRecord device)
        {
            return new DeviceRow(device.DisplayName, device.DeviceDisplayName, device.Location ?? "-", device.ConnectionKind.ToString());
        }
    }

    public sealed record HistoryRow(string IssuedAt, string InstallationId, int DeviceCount, string Locations, string ValidUntil, string FileName)
    {
        public static HistoryRow From(IssuedLicenseRecord record)
        {
            return new HistoryRow(
                record.IssuedAtUtc.ToLocalTime().ToString("dd.MM.yyyy", CultureInfo.GetCultureInfo("de-DE")),
                record.InstallationId,
                record.MaxActiveDeviceConnections,
                JoinLocations(record.Devices),
                FormatValidity(record.ValidUntilUtc),
                Path.GetFileName(record.OutputFilePath));
        }
    }
}
