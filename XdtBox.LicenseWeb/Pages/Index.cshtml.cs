using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;
using XdtDeviceBridge.Core;

namespace XdtBox.LicenseWeb.Pages;

public sealed class IndexModel : PageModel
{
    private readonly LicenseWebDataStore _store;

    public IndexModel(LicenseWebDataStore store)
    {
        _store = store;
    }

    public int CustomerCount { get; private set; }
    public int ActiveInstallationCount { get; private set; }
    public int ActiveDeviceCount { get; private set; }
    public string MonthlyTotalDisplay { get; private set; } = string.Empty;
    public string PrivateKeyStatus { get; private set; } = string.Empty;
    public string DataRoot { get; private set; } = string.Empty;

    public async Task OnGetAsync()
    {
        var snapshot = await _store.LoadSnapshotAsync();
        CustomerCount = snapshot.Customers.Count;
        ActiveInstallationCount = snapshot.Customers.Sum(customer => customer.ActiveInstallationCount);
        ActiveDeviceCount = snapshot.Customers.Sum(customer => customer.BillableDeviceCount);
        var total = LicenseManagerCostCalculator.CalculateNetTotal(ActiveDeviceCount, snapshot.Settings.PricePerDeviceNet);
        MonthlyTotalDisplay = total.ToString("N2", CultureInfo.GetCultureInfo("de-DE")) + " EUR";
        PrivateKeyStatus = snapshot.Runtime.PrivateKeyFileExists
            ? "konfiguriert"
            : snapshot.Runtime.PrivateKeyConfigured ? "Pfad konfiguriert, Datei nicht gefunden" : "nicht konfiguriert";
        DataRoot = snapshot.Runtime.DataRoot;
    }
}
