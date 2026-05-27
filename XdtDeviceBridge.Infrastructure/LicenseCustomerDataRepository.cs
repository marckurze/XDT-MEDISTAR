using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseCustomerDataRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public LicenseRequestCustomer LoadOrEmpty(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            return LicenseRequestCustomer.Empty;
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            return JsonSerializer.Deserialize<LicenseRequestCustomer>(json, Options)
                ?? LicenseRequestCustomer.Empty;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid license customer data JSON: {ex.Message}", ex);
        }
    }

    public void Save(string filePath, LicenseRequestCustomer customer)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(customer);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(customer, Options);
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
