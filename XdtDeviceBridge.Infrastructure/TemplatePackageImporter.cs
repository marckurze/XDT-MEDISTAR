using System.IO.Compression;
using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImporter
{
    private readonly ProfileJsonSerializer _serializer;

    public TemplatePackageImporter()
        : this(new ProfileJsonSerializer())
    {
    }

    public TemplatePackageImporter(ProfileJsonSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public TemplatePackageImportResult Import(string zipFilePath)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
        {
            throw new ArgumentException("ZIP file path must not be empty.", nameof(zipFilePath));
        }

        if (!File.Exists(zipFilePath))
        {
            throw new FileNotFoundException($"Template package ZIP file not found: {zipFilePath}", zipFilePath);
        }

        try
        {
            using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Read, Encoding.UTF8);

            var packageEntry = archive.GetEntry("package.json")
                ?? throw new InvalidOperationException("Template package ZIP must contain package.json.");

            var package = _serializer.DeserializeTemplatePackage(ReadEntry(packageEntry));

            return new TemplatePackageImportResult(
                Package: package,
                AisProfiles: ReadProfiles(archive, "ais/", _serializer.DeserializeAisProfile),
                DeviceProfiles: ReadProfiles(archive, "devices/", _serializer.DeserializeDeviceProfileDefinition),
                ExportProfiles: ReadProfiles(archive, "exports/", _serializer.DeserializeExportProfileDefinition),
                InterfaceProfiles: ReadProfiles(archive, "interfaces/", _serializer.DeserializeInterfaceProfileDefinition));
        }
        catch (InvalidDataException ex)
        {
            throw new InvalidOperationException($"Invalid template package ZIP: {ex.Message}", ex);
        }
    }

    private static IReadOnlyList<T> ReadProfiles<T>(
        ZipArchive archive,
        string folder,
        Func<string, T> deserialize)
    {
        return archive.Entries
            .Where(entry =>
                entry.FullName.StartsWith(folder, StringComparison.OrdinalIgnoreCase)
                && entry.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(entry.Name))
            .OrderBy(entry => entry.FullName, StringComparer.OrdinalIgnoreCase)
            .Select(entry => deserialize(ReadEntry(entry)))
            .ToList();
    }

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
