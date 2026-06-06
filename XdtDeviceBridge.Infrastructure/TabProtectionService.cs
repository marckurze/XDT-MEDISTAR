using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TabProtectionService
{
    public const string SettingsFileName = "tab-protection.json";

    private const int DefaultIterations = 120_000;
    private const int SaltByteCount = 16;
    private const int HashByteCount = 32;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public TabProtectionSettings LoadOrDefault(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Tab protection settings path must not be empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            return TabProtectionSettings.Disabled;
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<TabProtectionSettings>(json, JsonOptions)
                ?? TabProtectionSettings.Disabled;
        }
        catch (JsonException)
        {
            return TabProtectionSettings.Disabled;
        }
    }

    public void Save(string filePath, TabProtectionSettings settings)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Tab protection settings path must not be empty.", nameof(filePath));
        }

        ArgumentNullException.ThrowIfNull(settings);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        File.WriteAllText(filePath, JsonSerializer.Serialize(settings, JsonOptions));
    }

    public TabProtectionSettings CreateEnabledSettings(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password must not be empty.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltByteCount);
        var hash = HashPassword(password, salt, DefaultIterations);
        return new TabProtectionSettings(
            true,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash),
            DefaultIterations);
    }

    public bool VerifyPassword(TabProtectionSettings settings, string password)
    {
        if (!settings.HasPassword || string.IsNullOrEmpty(password))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(settings.SaltBase64!);
            var expectedHash = Convert.FromBase64String(settings.PasswordHashBase64!);
            var actualHash = HashPassword(password, salt, settings.Iterations);
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public bool VerifyGeneralPassword(TabProtectionSettings settings, string password)
    {
        return settings.HasPassword
            && string.Equals(password, BuildGeneralPassword(), StringComparison.Ordinal);
    }

    public bool VerifyAnyPassword(TabProtectionSettings settings, string password)
    {
        return VerifyPassword(settings, password) || VerifyGeneralPassword(settings, password);
    }

    public static string GetDefaultSettingsFilePath(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return Path.Combine(paths.BaseFolder, "ui", SettingsFileName);
    }

    private static byte[] HashPassword(string password, byte[] salt, int iterations)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashByteCount);
    }

    private static string BuildGeneralPassword()
    {
        var segments = new[] { "##", "XDT", "Box", "Admin", "##" };
        return string.Concat(segments);
    }
}
