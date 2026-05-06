using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicensedDeviceGracePeriodRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public void Save(string filePath, LicensedDeviceGracePeriodStore store)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(store);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(store, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    public LicensedDeviceGracePeriodStore Load(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Licensed device grace period file not found: {filePath}", filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<LicensedDeviceGracePeriodStore>(json, Options)
                ?? throw new InvalidOperationException("Licensed device grace period file did not contain valid data.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid licensed device grace period JSON: {ex.Message}", ex);
        }
    }

    public LicensedDeviceGracePeriodStore LoadOrEmpty(string filePath)
    {
        EnsureFilePath(filePath);

        return File.Exists(filePath)
            ? Load(filePath)
            : LicensedDeviceGracePeriodStore.Empty;
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
