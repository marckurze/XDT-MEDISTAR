using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;

namespace XdtBox.LicenseWeb.Pages.Settings;

public sealed class IndexModel : PageModel
{
    private readonly LicenseWebDataStore _store;

    public IndexModel(LicenseWebDataStore store)
    {
        _store = store;
    }

    [BindProperty]
    public string PricePerDeviceNet { get; set; } = string.Empty;

    public string DataRoot { get; private set; } = string.Empty;
    public string PrivateKeyStatus { get; private set; } = string.Empty;
    public string Issuer { get; private set; } = string.Empty;
    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var snapshot = await _store.LoadSnapshotAsync();
        if (!decimal.TryParse(PricePerDeviceNet.Replace(".", ",", StringComparison.Ordinal), NumberStyles.Number, CultureInfo.GetCultureInfo("de-DE"), out var price))
        {
            ErrorMessage = "Der Einzelpreis ist keine gültige Zahl.";
            await LoadAsync();
            return Page();
        }

        await _store.SaveSettingsAsync(snapshot.Settings with { PricePerDeviceNet = price });
        StatusMessage = "Einstellungen gespeichert.";
        await LoadAsync();
        return Page();
    }

    private async Task LoadAsync()
    {
        var snapshot = await _store.LoadSnapshotAsync();
        PricePerDeviceNet = snapshot.Settings.PricePerDeviceNet.ToString("N2", CultureInfo.GetCultureInfo("de-DE"));
        DataRoot = snapshot.Runtime.DataRoot;
        PrivateKeyStatus = snapshot.Runtime.PrivateKeyFileExists
            ? "konfiguriert"
            : snapshot.Runtime.PrivateKeyConfigured ? "Pfad konfiguriert, Datei nicht gefunden" : "nicht konfiguriert";
        Issuer = snapshot.Settings.DefaultIssuer;
    }
}
