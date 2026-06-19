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

    [BindProperty]
    public CustomerEditInput Edit { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        return await LoadAsync(id, populateEdit: true)
            ? Page()
            : NotFound();
    }

    public async Task<IActionResult> OnPostSaveAsync(string id)
    {
        try
        {
            await _store.UpdateCustomerAsync(id, Edit.ToUpdate());
            StatusMessage = "Kundendaten gespeichert.";
            return RedirectToPage(new { id });
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            return await LoadAsync(id, populateEdit: false)
                ? Page()
                : NotFound();
        }
    }

    public async Task<IActionResult> OnGetPdfAsync(string id)
    {
        var download = await _licenseService.CreateCustomerPdfAsync(id);
        return File(download.Content, download.ContentType, download.FileName);
    }

    private async Task<bool> LoadAsync(string id, bool populateEdit)
    {
        var snapshot = await _store.LoadSnapshotAsync();
        var customer = snapshot.Customers.FirstOrDefault(candidate => string.Equals(candidate.Id, id, StringComparison.Ordinal));
        if (customer is null)
        {
            return false;
        }

        Customer = CustomerDetail.From(customer, snapshot.Settings.PricePerDeviceNet);
        Installations = customer.EffectiveInstallations.Select(InstallationRow.From).ToArray();
        Devices = customer.EffectiveInstallations
            .SelectMany(installation => installation.Devices.Select(device => DeviceRow.From(installation.InstallationId, device)))
            .ToArray();
        var installationIds = customer.EffectiveInstallations.Select(installation => installation.InstallationId).ToHashSet(StringComparer.OrdinalIgnoreCase);
        History = snapshot.History
            .Where(record => installationIds.Contains(record.InstallationId)
                || (!string.IsNullOrWhiteSpace(customer.CustomerNumber)
                    && string.Equals(record.CustomerNumber, customer.CustomerNumber, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(record => record.IssuedAtUtc)
            .Select(HistoryRow.From)
            .ToArray();
        if (populateEdit)
        {
            Edit = CustomerEditInput.From(customer);
        }
        else
        {
            AlignEditDeviceLocations(customer);
        }

        return true;
    }

    private void AlignEditDeviceLocations(LicenseManagerCustomerRecord customer)
    {
        var postedLocations = Edit.DeviceLocations
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last().Location, StringComparer.OrdinalIgnoreCase);
        Edit.DeviceLocations = customer.EffectiveInstallations
            .SelectMany(installation => installation.Devices.Select(device =>
            {
                var key = LicenseWebDataStore.CreateDeviceLocationKey(installation.InstallationId, device);
                return new DeviceLocationInput
                {
                    Key = key,
                    Location = postedLocations.TryGetValue(key, out var location) ? location : device.Location
                };
            }))
            .ToList();
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
        string MonthlyTotal)
    {
        public static readonly CustomerDetail Empty = new(string.Empty, string.Empty, string.Empty);

        public static CustomerDetail From(LicenseManagerCustomerRecord customer, decimal price)
        {
            var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, price);
            return new CustomerDetail(
                customer.Id,
                customer.CustomerName,
                total.ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " EUR");
        }
    }

    public sealed class CustomerEditInput
    {
        public string? CustomerNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ContactPerson { get; set; }
        public string? InvoiceEmail { get; set; }
        public string? Iban { get; set; }
        public string? Bic { get; set; }
        public string? AccountHolder { get; set; }
        public string PaymentMethod { get; set; } = nameof(LicenseManagerPaymentMethod.BankTransfer);
        public List<DeviceLocationInput> DeviceLocations { get; set; } = new();

        public static CustomerEditInput From(LicenseManagerCustomerRecord customer)
        {
            return new CustomerEditInput
            {
                CustomerNumber = customer.CustomerNumber,
                CustomerName = customer.CustomerName,
                Street = customer.Street,
                PostalCode = customer.PostalCode,
                City = customer.City,
                Phone = customer.Phone,
                Email = customer.Email,
                ContactPerson = customer.ContactPerson,
                InvoiceEmail = customer.InvoiceEmail,
                Iban = customer.Iban,
                Bic = customer.Bic,
                AccountHolder = customer.AccountHolder,
                PaymentMethod = customer.PaymentMethod.ToString(),
                DeviceLocations = customer.EffectiveInstallations
                    .SelectMany(installation => installation.Devices.Select(device => new DeviceLocationInput
                    {
                        Key = LicenseWebDataStore.CreateDeviceLocationKey(installation.InstallationId, device),
                        Location = device.Location
                    }))
                    .ToList()
            };
        }

        public LicenseWebCustomerUpdate ToUpdate()
        {
            var paymentMethod = string.Equals(PaymentMethod, nameof(LicenseManagerPaymentMethod.SepaDirectDebit), StringComparison.Ordinal)
                ? LicenseManagerPaymentMethod.SepaDirectDebit
                : LicenseManagerPaymentMethod.BankTransfer;
            return new LicenseWebCustomerUpdate(
                CustomerNumber,
                CustomerName,
                Street,
                PostalCode,
                City,
                Phone,
                Email,
                ContactPerson,
                InvoiceEmail,
                Iban,
                Bic,
                AccountHolder,
                paymentMethod,
                DeviceLocations.Select(item => new LicenseWebCustomerDeviceLocationUpdate(item.Key, item.Location)).ToArray());
        }
    }

    public sealed class DeviceLocationInput
    {
        public string Key { get; set; } = string.Empty;
        public string? Location { get; set; }
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

    public sealed record DeviceRow(string Key, string DisplayName, string DeviceDisplayName, string Location, string ConnectionKind)
    {
        public static DeviceRow From(string installationId, IssuedLicenseDeviceRecord device)
        {
            return new DeviceRow(
                LicenseWebDataStore.CreateDeviceLocationKey(installationId, device),
                device.DisplayName,
                device.DeviceDisplayName,
                device.Location ?? string.Empty,
                device.ConnectionKind.ToString());
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
