using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseManagerCustomerRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public IReadOnlyList<LicenseManagerCustomerRecord> LoadOrEmpty(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            return Array.Empty<LicenseManagerCustomerRecord>();
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            var records = JsonSerializer.Deserialize<List<LicenseManagerCustomerRecord>>(json, Options);
            return records is null ? Array.Empty<LicenseManagerCustomerRecord>() : records;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid license manager customer JSON: {ex.Message}", ex);
        }
    }

    public void Save(string filePath, IReadOnlyList<LicenseManagerCustomerRecord> records)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(records);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(records, Options);
        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    public IReadOnlyList<LicenseManagerCustomerRecord> Upsert(string filePath, LicenseManagerCustomerRecord customer)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(customer);

        var records = LoadOrEmpty(filePath).ToList();
        var index = FindMatchingIndex(records, customer);
        if (index >= 0)
        {
            records[index] = records[index].MergeImported(customer);
        }
        else
        {
            records.Add(customer);
        }

        Save(filePath, records);
        return records;
    }

    public IReadOnlyList<LicenseManagerCustomerRecord> UpsertLicense(string filePath, LicenseManagerCustomerRecord customer, IssuedLicenseRecord license)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(license);

        var records = LoadOrEmpty(filePath).ToList();
        var enriched = customer.WithLicense(license);
        var index = FindMatchingIndex(records, enriched);
        if (index >= 0)
        {
            records[index] = records[index].MergeImported(customer).WithLicense(license);
        }
        else
        {
            records.Add(enriched);
        }

        Save(filePath, records);
        return records;
    }

    private static int FindMatchingIndex(IReadOnlyList<LicenseManagerCustomerRecord> records, LicenseManagerCustomerRecord customer)
    {
        if (!string.IsNullOrWhiteSpace(customer.CustomerNumber))
        {
            for (var i = 0; i < records.Count; i++)
            {
                if (string.Equals(records[i].CustomerNumber, customer.CustomerNumber, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(customer.InstallationId))
        {
            for (var i = 0; i < records.Count; i++)
            {
                if (string.Equals(records[i].InstallationId, customer.InstallationId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
