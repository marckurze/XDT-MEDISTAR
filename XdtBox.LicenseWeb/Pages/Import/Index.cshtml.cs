using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;

namespace XdtBox.LicenseWeb.Pages.Import;

public sealed class IndexModel : PageModel
{
    private readonly LicenseWebDataStore _store;
    private readonly LicenseWebLicenseService _licenseService;

    public IndexModel(LicenseWebDataStore store, LicenseWebLicenseService licenseService)
    {
        _store = store;
        _licenseService = licenseService;
    }

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    public string? RequestFile { get; set; }

    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool PrivateKeyReady { get; private set; }
    public string PrivateKeyStatus { get; private set; } = "nicht konfiguriert";

    public async Task OnGetAsync()
    {
        await LoadRuntimeAsync();
    }

    public async Task<IActionResult> OnPostUploadAsync()
    {
        await LoadRuntimeAsync();
        if (UploadFile is null)
        {
            ErrorMessage = "Bitte eine Lizenzanfrage-Datei auswählen.";
            return Page();
        }

        try
        {
            var result = await _store.ImportRequestAsync(UploadFile);
            RequestFile = result.RequestFile;
            StatusMessage = string.Join(" ", result.Messages);
            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostCreateLicenseAsync()
    {
        await LoadRuntimeAsync();
        if (string.IsNullOrWhiteSpace(RequestFile))
        {
            ErrorMessage = "Es ist keine importierte Lizenzanfrage ausgewählt.";
            return Page();
        }

        try
        {
            var result = await _licenseService.CreateLicenseFromRequestAsync(RequestFile);
            var bytes = await System.IO.File.ReadAllBytesAsync(result.OutputFile);
            return File(bytes, "application/octet-stream", result.FileName);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    private async Task LoadRuntimeAsync()
    {
        var snapshot = await _store.LoadSnapshotAsync();
        PrivateKeyReady = snapshot.Runtime.LicenseSignatureAvailable;
        PrivateKeyStatus = snapshot.Runtime.PrivateKeyStatus;
    }
}
