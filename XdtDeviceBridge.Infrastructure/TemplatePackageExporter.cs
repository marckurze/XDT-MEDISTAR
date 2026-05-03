using System.IO.Compression;
using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageExporter
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly ProfileJsonSerializer _serializer;

    public TemplatePackageExporter()
        : this(new ProfileJsonSerializer())
    {
    }

    public TemplatePackageExporter(ProfileJsonSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public void Export(string zipFilePath, TemplatePackageExportRequest request)
    {
        if (string.IsNullOrWhiteSpace(zipFilePath))
        {
            throw new ArgumentException("ZIP file path must not be empty.", nameof(zipFilePath));
        }

        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Package);

        var directory = Path.GetDirectoryName(Path.GetFullPath(zipFilePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(zipFilePath))
        {
            File.Delete(zipFilePath);
        }

        using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create, Encoding.UTF8);

        WriteEntry(archive, "package.json", _serializer.SerializeTemplatePackage(request.Package));

        foreach (var profile in request.AisProfiles ?? Array.Empty<AisProfile>())
        {
            WriteEntry(archive, $"ais/{profile.Metadata.Id}.json", _serializer.SerializeAisProfile(profile));
        }

        foreach (var profile in request.DeviceProfiles ?? Array.Empty<DeviceProfileDefinition>())
        {
            WriteEntry(archive, $"devices/{profile.Metadata.Id}.json", _serializer.SerializeDeviceProfileDefinition(profile));
        }

        foreach (var profile in request.ExportProfiles ?? Array.Empty<ExportProfileDefinition>())
        {
            WriteEntry(archive, $"exports/{profile.Metadata.Id}.json", _serializer.SerializeExportProfileDefinition(profile));
        }

        foreach (var profile in request.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>())
        {
            WriteEntry(archive, $"interfaces/{profile.Metadata.Id}.json", _serializer.SerializeInterfaceProfileDefinition(profile));
        }
    }

    private static void WriteEntry(ZipArchive archive, string entryName, string content)
    {
        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

        using var stream = entry.Open();
        using var writer = new StreamWriter(stream, Utf8NoBom);
        writer.Write(content);
    }
}
