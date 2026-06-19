using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseManagerBackupService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public void CreateBackup(
        string filePath,
        IReadOnlyList<LicenseManagerCustomerRecord> customers,
        LicenseManagerSettings settings,
        IReadOnlyList<IssuedLicenseRecord> history)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(customers);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(history);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var safeSettings = settings with { PrivateKeyPath = null };
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
        WriteEntry(archive, "manifest.json", new LicenseManagerBackupManifest(
            FormatVersion: 1,
            Product: "XDTBox LicenseManager",
            CreatedAtUtc: DateTime.UtcNow));
        WriteEntry(archive, "customers.json", customers);
        WriteEntry(archive, "settings.json", safeSettings);
        WriteEntry(archive, "history.json", history);
    }

    public LicenseManagerBackupData ReadBackup(string filePath)
    {
        EnsureFilePath(filePath);

        try
        {
            using var archive = ZipFile.OpenRead(filePath);
            var manifest = ReadEntry<LicenseManagerBackupManifest>(archive, "manifest.json")
                ?? throw new InvalidOperationException("Backup manifest is missing.");
            if (manifest.FormatVersion != 1)
            {
                throw new InvalidOperationException($"Unsupported backup format version: {manifest.FormatVersion}");
            }

            return new LicenseManagerBackupData(
                Customers: ReadEntry<List<LicenseManagerCustomerRecord>>(archive, "customers.json") ?? new(),
                Settings: ReadEntry<LicenseManagerSettings>(archive, "settings.json")
                    ?? throw new InvalidOperationException("Backup settings are missing."),
                History: ReadEntry<List<IssuedLicenseRecord>>(archive, "history.json") ?? new());
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidOperationException($"Invalid LicenseManager backup: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid LicenseManager backup JSON: {ex.Message}", ex);
        }
    }

    public void RestoreBackup(string filePath, LicenseManagerPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var data = ReadBackup(filePath);
        var settings = data.Settings with { PrivateKeyPath = null };

        Directory.CreateDirectory(paths.DataFolder);
        File.WriteAllText(paths.CustomersFile, JsonSerializer.Serialize(data.Customers, Options), Utf8NoBom);
        File.WriteAllText(paths.SettingsFile, JsonSerializer.Serialize(settings, Options), Utf8NoBom);
        File.WriteAllText(paths.HistoryFile, JsonSerializer.Serialize(data.History, Options), Utf8NoBom);
    }

    private static void WriteEntry<T>(ZipArchive archive, string entryName, T value)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Utf8NoBom);
        writer.Write(JsonSerializer.Serialize(value, Options));
    }

    private static T? ReadEntry<T>(ZipArchive archive, string entryName)
    {
        var entry = archive.GetEntry(entryName);
        if (entry is null)
        {
            return default;
        }

        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Utf8NoBom);
        return JsonSerializer.Deserialize<T>(reader.ReadToEnd(), Options);
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }

    private sealed record LicenseManagerBackupManifest(int FormatVersion, string Product, DateTime CreatedAtUtc);
}

public sealed record LicenseManagerBackupData(
    IReadOnlyList<LicenseManagerCustomerRecord> Customers,
    LicenseManagerSettings Settings,
    IReadOnlyList<IssuedLicenseRecord> History);
