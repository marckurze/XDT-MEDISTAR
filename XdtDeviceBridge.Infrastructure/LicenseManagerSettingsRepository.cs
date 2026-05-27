using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseManagerSettingsRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public LicenseManagerSettings LoadOrDefault(string filePath, string baseFolder)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            return LicenseManagerSettings.CreateDefault(baseFolder);
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<LicenseManagerSettings>(json, Options)
                ?? LicenseManagerSettings.CreateDefault(baseFolder);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid license manager settings JSON: {ex.Message}", ex);
        }
    }

    public void Save(string filePath, LicenseManagerSettings settings)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(settings);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
