using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;

namespace XdtBox.LicenseWeb.Pages.Backup;

public sealed class IndexModel : PageModel
{
    private readonly LicenseWebDataStore _store;

    public IndexModel(LicenseWebDataStore store)
    {
        _store = store;
    }

    [BindProperty]
    public IFormFile? RestoreFile { get; set; }

    public string? StatusMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetDownloadAsync()
    {
        var bytes = await _store.CreateBackupAsync();
        return File(bytes, "application/octet-stream", $"xdtbox-licenseweb-backup-{DateTime.Today:yyyyMMdd}.xdtbox-licensemanager-backup");
    }

    public async Task<IActionResult> OnPostRestoreAsync()
    {
        if (RestoreFile is null)
        {
            ErrorMessage = "Bitte eine Sicherungsdatei auswählen.";
            return Page();
        }

        try
        {
            await _store.RestoreBackupAsync(RestoreFile);
            StatusMessage = "Backup wurde wiederhergestellt.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
}
