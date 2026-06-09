using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseDeviceLocationRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public LicenseDeviceLocationStore LoadOrEmpty(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            return LicenseDeviceLocationStore.Empty;
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<LicenseDeviceLocationStore>(json, Options)
                ?? LicenseDeviceLocationStore.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid license device location JSON: {ex.Message}", ex);
        }
    }

    public void Save(string filePath, LicenseDeviceLocationStore store)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(store);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var normalized = new LicenseDeviceLocationStore(
            store.DeviceLocations
                .Where(location => !string.IsNullOrWhiteSpace(location.InterfaceProfileId))
                .Select(location => new LicenseDeviceLocation(
                    location.InterfaceProfileId.Trim(),
                    Normalize(location.Location)))
                .GroupBy(location => location.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.Last())
                .OrderBy(location => location.InterfaceProfileId, StringComparer.OrdinalIgnoreCase)
                .ToArray());

        var json = JsonSerializer.Serialize(normalized, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
