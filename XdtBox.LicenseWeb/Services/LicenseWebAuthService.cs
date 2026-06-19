using Microsoft.Extensions.Options;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebAuthService
{
    private readonly LicenseWebPasswordHasher _passwordHasher;
    private readonly IOptionsMonitor<LicenseWebOptions> _options;

    public LicenseWebAuthService(LicenseWebPasswordHasher passwordHasher, IOptionsMonitor<LicenseWebOptions> options)
    {
        _passwordHasher = passwordHasher;
        _options = options;
    }

    public bool IsConfigured
    {
        get
        {
            var admin = _options.CurrentValue.Admin;
            return !string.IsNullOrWhiteSpace(admin.Username)
                && !string.IsNullOrWhiteSpace(admin.PasswordHash)
                && !string.IsNullOrWhiteSpace(admin.PasswordSalt);
        }
    }

    public string ConfiguredUserName => _options.CurrentValue.Admin.Username ?? string.Empty;

    public bool ValidateCredentials(string userName, string password)
    {
        var admin = _options.CurrentValue.Admin;
        if (!IsConfigured)
        {
            return false;
        }

        return string.Equals(userName.Trim(), admin.Username, StringComparison.Ordinal)
            && _passwordHasher.Verify(password, admin.PasswordHash!, admin.PasswordSalt!);
    }
}
