using Microsoft.Extensions.Options;

namespace XdtBox.LicenseWeb.Services;

public sealed class LicenseWebAuthService
{
    private readonly LicenseWebPasswordHasher _passwordHasher;
    private readonly IOptionsMonitor<LicenseWebOptions> _options;
    private readonly LicenseWebDataStore? _store;

    public LicenseWebAuthService(
        LicenseWebPasswordHasher passwordHasher,
        IOptionsMonitor<LicenseWebOptions> options,
        LicenseWebDataStore? store = null)
    {
        _passwordHasher = passwordHasher;
        _options = options;
        _store = store;
    }

    public bool IsConfigured => GetEffectiveAdmin() is not null;

    public string ConfiguredUserName => GetEffectiveAdmin()?.Username ?? string.Empty;

    public bool ValidateCredentials(string userName, string password)
    {
        var admin = GetEffectiveAdmin();
        if (admin is null)
        {
            return false;
        }

        return string.Equals(userName.Trim(), admin.Username, StringComparison.Ordinal)
            && _passwordHasher.Verify(password, admin.PasswordHash!, admin.PasswordSalt!, admin.PasswordIterations);
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword, string repeatedPassword)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Admin-Zugang ist nicht konfiguriert.");
        }

        if (!ValidateCredentials(ConfiguredUserName, currentPassword))
        {
            throw new InvalidOperationException("Das aktuelle Admin-Passwort ist falsch.");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 10)
        {
            throw new InvalidOperationException("Das neue Admin-Passwort muss mindestens 10 Zeichen lang sein.");
        }

        if (!string.Equals(newPassword, repeatedPassword, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Die Wiederholung des neuen Passworts stimmt nicht.");
        }

        if (_store is null)
        {
            throw new InvalidOperationException("Admin-Passwort kann in dieser Umgebung nicht gespeichert werden.");
        }

        var hash = _passwordHasher.HashPassword(newPassword);
        await _store.SaveAdminCredentialOverrideAsync(new LicenseWebAdminCredential(
            Username: ConfiguredUserName,
            PasswordHash: hash.HashBase64,
            PasswordSalt: hash.SaltBase64,
            PasswordIterations: hash.Iterations,
            UpdatedAtUtc: DateTime.UtcNow)).ConfigureAwait(false);
    }

    private EffectiveAdmin? GetEffectiveAdmin()
    {
        var stored = _store?.LoadAdminCredentialOverride();
        if (stored is not null)
        {
            return new EffectiveAdmin(
                stored.Username,
                stored.PasswordHash,
                stored.PasswordSalt,
                stored.PasswordIterations);
        }

        var admin = _options.CurrentValue.Admin;
        return !string.IsNullOrWhiteSpace(admin.Username)
            && !string.IsNullOrWhiteSpace(admin.PasswordHash)
            && !string.IsNullOrWhiteSpace(admin.PasswordSalt)
                ? new EffectiveAdmin(
                    admin.Username!,
                    admin.PasswordHash!,
                    admin.PasswordSalt!,
                    admin.PasswordIterations)
                : null;
    }

    private sealed record EffectiveAdmin(
        string Username,
        string PasswordHash,
        string PasswordSalt,
        int PasswordIterations);
}
