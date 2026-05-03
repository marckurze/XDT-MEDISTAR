using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseRequestFileRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public void Save(string filePath, LicenseRequest request)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(request);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(request, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    public LicenseRequest Load(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"License request file not found: {filePath}", filePath);
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<LicenseRequest>(json, Options)
                ?? throw new InvalidOperationException("License request file did not contain valid request data.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid license request JSON: {ex.Message}", ex);
        }
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
