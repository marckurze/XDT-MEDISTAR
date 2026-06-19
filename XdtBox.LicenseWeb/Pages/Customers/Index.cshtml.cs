using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseWeb.Pages.Customers;

public sealed class IndexModel : PageModel
{
    private readonly LicenseWebDataStore _store;
    private readonly LicenseWebLicenseService _licenseService;

    public IndexModel(LicenseWebDataStore store, LicenseWebLicenseService licenseService)
    {
        _store = store;
        _licenseService = licenseService;
    }

    public IReadOnlyList<CustomerRow> Customers { get; private set; } = Array.Empty<CustomerRow>();

    [BindProperty]
    public List<string> SelectedCustomerIds { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string id)
    {
        try
        {
            var customer = await _store.DeleteCustomerAsync(id);
            StatusMessage = $"Kunde \"{customer.CustomerName}\" wurde gelöscht.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMergeSelectedAsync()
    {
        try
        {
            var result = await _store.MergeCustomersAsync(SelectedCustomerIds);
            StatusMessage = $"{result.MergedCustomerCount} Kunden wurden zu \"{result.TargetCustomer.CustomerName}\" zusammengeführt.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetCustomerPdfAsync(string id)
    {
        var download = await _licenseService.CreateCustomerPdfAsync(id);
        return File(download.Content, download.ContentType, download.FileName);
    }

    public async Task<IActionResult> OnGetTotalPdfAsync()
    {
        var download = await _licenseService.CreateTotalPdfAsync();
        return File(download.Content, download.ContentType, download.FileName);
    }

    private async Task LoadAsync()
    {
        var snapshot = await _store.LoadSnapshotAsync();
        Customers = snapshot.Customers
            .OrderBy(customer => customer.CustomerName, StringComparer.CurrentCultureIgnoreCase)
            .Select(customer => CustomerRow.From(customer, snapshot.Settings.PricePerDeviceNet))
            .ToArray();
    }

    public sealed record CustomerRow(
        string Id,
        string CustomerNumber,
        string CustomerName,
        string City,
        string InvoiceEmail,
        string PaymentMethod,
        int ActiveInstallationCount,
        int ActiveDeviceCount,
        string MonthlyTotal)
    {
        public static CustomerRow From(LicenseManagerCustomerRecord customer, decimal pricePerDeviceNet)
        {
            var total = LicenseManagerCostCalculator.CalculateNetTotal(customer.BillableDeviceCount, pricePerDeviceNet);
            return new CustomerRow(
                customer.Id,
                customer.CustomerNumber ?? string.Empty,
                customer.CustomerName,
                customer.City,
                customer.InvoiceEmail ?? customer.Email ?? string.Empty,
                customer.PaymentMethod == LicenseManagerPaymentMethod.SepaDirectDebit ? "SEPA-Lastschrift" : "Banküberweisung",
                customer.ActiveInstallationCount,
                customer.BillableDeviceCount,
                total.ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " EUR");
        }
    }
}
