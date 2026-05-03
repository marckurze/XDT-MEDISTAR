using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ProfileFileRepository
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly ProfileJsonSerializer _serializer;

    public ProfileFileRepository()
        : this(new ProfileJsonSerializer())
    {
    }

    public ProfileFileRepository(ProfileJsonSerializer serializer)
    {
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public void SaveAisProfile(string filePath, AisProfile profile)
    {
        Save(filePath, _serializer.SerializeAisProfile(profile));
    }

    public AisProfile LoadAisProfile(string filePath)
    {
        return _serializer.DeserializeAisProfile(Load(filePath));
    }

    public void SaveDeviceProfileDefinition(string filePath, DeviceProfileDefinition profile)
    {
        Save(filePath, _serializer.SerializeDeviceProfileDefinition(profile));
    }

    public DeviceProfileDefinition LoadDeviceProfileDefinition(string filePath)
    {
        return _serializer.DeserializeDeviceProfileDefinition(Load(filePath));
    }

    public void SaveExportProfileDefinition(string filePath, ExportProfileDefinition profile)
    {
        Save(filePath, _serializer.SerializeExportProfileDefinition(profile));
    }

    public ExportProfileDefinition LoadExportProfileDefinition(string filePath)
    {
        return _serializer.DeserializeExportProfileDefinition(Load(filePath));
    }

    public void SaveInterfaceProfileDefinition(string filePath, InterfaceProfileDefinition profile)
    {
        Save(filePath, _serializer.SerializeInterfaceProfileDefinition(profile));
    }

    public InterfaceProfileDefinition LoadInterfaceProfileDefinition(string filePath)
    {
        return _serializer.DeserializeInterfaceProfileDefinition(Load(filePath));
    }

    public void SaveTemplatePackage(string filePath, TemplatePackage package)
    {
        Save(filePath, _serializer.SerializeTemplatePackage(package));
    }

    public TemplatePackage LoadTemplatePackage(string filePath)
    {
        return _serializer.DeserializeTemplatePackage(Load(filePath));
    }

    private static void Save(string filePath, string json)
    {
        EnsureFilePath(filePath);

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(filePath, json, Utf8NoBom);
    }

    private static string Load(string filePath)
    {
        EnsureFilePath(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Profile file not found: {filePath}", filePath);
        }

        return File.ReadAllText(filePath, Utf8NoBom);
    }

    private static void EnsureFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }
    }
}
