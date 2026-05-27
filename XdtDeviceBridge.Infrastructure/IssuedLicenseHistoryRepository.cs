using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class IssuedLicenseHistoryRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    public IReadOnlyList<IssuedLicenseRecord> LoadOrEmpty(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            return Array.Empty<IssuedLicenseRecord>();
        }

        try
        {
            var json = File.ReadAllText(filePath, Utf8NoBom);
            var records = JsonSerializer.Deserialize<List<IssuedLicenseRecord>>(json, Options);
            return records is null ? Array.Empty<IssuedLicenseRecord>() : records;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid issued license history JSON: {ex.Message}", ex);
        }
    }

    public void Save(string filePath, IReadOnlyList<IssuedLicenseRecord> records)
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

    public IReadOnlyList<IssuedLicenseRecord> Add(string filePath, IssuedLicenseRecord record)
    {
        EnsureFilePath(filePath);
        ArgumentNullException.ThrowIfNull(record);

        var records = LoadOrEmpty(filePath).ToList();
        records.Add(record);
        Save(filePath, records);
        return records;
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
