using System.Text.Json;

namespace XdtDeviceBridge.Infrastructure;

public sealed class XdtBoxAppSettingsRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public XdtBoxAppSettings LoadOrDefault(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Settings file path must not be empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            return XdtBoxAppSettings.CreateDefault();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<XdtBoxAppSettings>(json, Options)
                ?? XdtBoxAppSettings.CreateDefault();
        }
        catch (JsonException)
        {
            return XdtBoxAppSettings.CreateDefault();
        }
    }

    public void Save(string filePath, XdtBoxAppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Settings file path must not be empty.", nameof(filePath));
        }

        ArgumentNullException.ThrowIfNull(settings);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? ".");
        var json = JsonSerializer.Serialize(settings, Options);
        File.WriteAllText(filePath, json);
    }
}
