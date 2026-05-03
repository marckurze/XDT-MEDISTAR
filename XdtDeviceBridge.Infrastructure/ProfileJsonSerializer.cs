using System.Text.Json;
using System.Text.Json.Serialization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ProfileJsonSerializer
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    public string SerializeAisProfile(AisProfile profile)
    {
        return Serialize(profile);
    }

    public AisProfile DeserializeAisProfile(string json)
    {
        return Deserialize<AisProfile>(json, nameof(AisProfile));
    }

    public string SerializeDeviceProfileDefinition(DeviceProfileDefinition profile)
    {
        return Serialize(profile);
    }

    public DeviceProfileDefinition DeserializeDeviceProfileDefinition(string json)
    {
        return Deserialize<DeviceProfileDefinition>(json, nameof(DeviceProfileDefinition));
    }

    public string SerializeExportProfileDefinition(ExportProfileDefinition profile)
    {
        return Serialize(profile);
    }

    public ExportProfileDefinition DeserializeExportProfileDefinition(string json)
    {
        return Deserialize<ExportProfileDefinition>(json, nameof(ExportProfileDefinition));
    }

    public string SerializeInterfaceProfileDefinition(InterfaceProfileDefinition profile)
    {
        return Serialize(profile);
    }

    public InterfaceProfileDefinition DeserializeInterfaceProfileDefinition(string json)
    {
        return Deserialize<InterfaceProfileDefinition>(json, nameof(InterfaceProfileDefinition));
    }

    public string SerializeTemplatePackage(TemplatePackage package)
    {
        return Serialize(package);
    }

    public TemplatePackage DeserializeTemplatePackage(string json)
    {
        return Deserialize<TemplatePackage>(json, nameof(TemplatePackage));
    }

    private static string Serialize<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, Options);
    }

    private static T Deserialize<T>(string json, string modelName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON content must not be empty.", nameof(json));
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, Options)
                ?? throw new InvalidOperationException($"JSON content did not contain a valid {modelName}.");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON for {modelName}: {ex.Message}", ex);
        }
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }
}
