using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using XdtBox.LicenseWeb.Services;

namespace XdtBox.LicenseWeb.Pages.Account;

[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly LicenseWebAuthService _authService;

    public LoginModel(LicenseWebAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    [Required]
    public string UserName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public bool IsConfigured => _authService.IsConfigured;
    public string? ErrorMessage { get; private set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_authService.IsConfigured)
        {
            ErrorMessage = "Admin-Zugang ist nicht konfiguriert.";
            return Page();
        }

        if (!_authService.ValidateCredentials(UserName, Password))
        {
            ErrorMessage = "Benutzer oder Passwort ist falsch.";
            return Page();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, UserName.Trim()),
            new Claim(ClaimTypes.Role, "Administrator")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return LocalRedirect(string.IsNullOrWhiteSpace(ReturnUrl) ? Url.Page("/Index")! : ReturnUrl);
    }
}
