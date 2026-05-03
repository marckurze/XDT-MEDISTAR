using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseFileRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public void Save(string filePath, LicenseInfo license)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(license);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(license, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    public LicenseInfo Load(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"License file not found: {filePath}", filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<LicenseInfo>(json, Options)
                ?? throw new InvalidOperationException("License file did not contain valid license data.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid license JSON: {ex.Message}", ex);
        }
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
