using System.Text.Json;
using System.Text.Json.Serialization;

namespace XdtDeviceBridge.Infrastructure;

public sealed class DeviceTechnicalProfileService
{
    public const string DefaultOverrideFileName = "device-info-overrides.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private readonly string? _originalCatalogPath;
    private readonly string _overrideCatalogPath;
    private readonly Func<Stream?>? _originalCatalogStreamFactory;

    public DeviceTechnicalProfileService(string originalCatalogPath, string overrideCatalogPath)
    {
        if (string.IsNullOrWhiteSpace(originalCatalogPath))
        {
            throw new ArgumentException("Original catalog path must not be empty.", nameof(originalCatalogPath));
        }

        if (string.IsNullOrWhiteSpace(overrideCatalogPath))
        {
            throw new ArgumentException("Override catalog path must not be empty.", nameof(overrideCatalogPath));
        }

        _originalCatalogPath = originalCatalogPath;
        _overrideCatalogPath = overrideCatalogPath;
    }

    public DeviceTechnicalProfileService(Func<Stream?> originalCatalogStreamFactory, string overrideCatalogPath)
    {
        ArgumentNullException.ThrowIfNull(originalCatalogStreamFactory);

        if (string.IsNullOrWhiteSpace(overrideCatalogPath))
        {
            throw new ArgumentException("Override catalog path must not be empty.", nameof(overrideCatalogPath));
        }

        _originalCatalogStreamFactory = originalCatalogStreamFactory;
        _overrideCatalogPath = overrideCatalogPath;
    }

    public static string GetDefaultOverrideFilePath(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return Path.Combine(paths.BaseFolder, DefaultOverrideFileName);
    }

    public IReadOnlyList<DeviceTechnicalProfile> LoadOriginalProfiles()
    {
        if (_originalCatalogStreamFactory is not null)
        {
            using var resourceStream = _originalCatalogStreamFactory();
            return resourceStream is null
                ? Array.Empty<DeviceTechnicalProfile>()
                : LoadOriginalProfiles(resourceStream);
        }

        if (string.IsNullOrWhiteSpace(_originalCatalogPath) || !File.Exists(_originalCatalogPath))
        {
            return Array.Empty<DeviceTechnicalProfile>();
        }

        using var stream = File.OpenRead(_originalCatalogPath);
        return LoadOriginalProfiles(stream);
    }

    public IReadOnlyList<DeviceTechnicalProfile> LoadOriginalProfiles(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var catalog = JsonSerializer.Deserialize<DeviceTechnicalProfileCatalog>(stream, JsonOptions);
        return catalog?.Profiles ?? Array.Empty<DeviceTechnicalProfile>();
    }

    public bool HasOriginalProfile(string deviceProfileId)
    {
        return LoadOriginalProfiles()
            .Any(profile => IsSameDevice(profile.DeviceProfileId, deviceProfileId));
    }

    public DeviceTechnicalProfile? LoadProfile(string deviceProfileId)
    {
        var original = LoadOriginalProfiles()
            .FirstOrDefault(profile => IsSameDevice(profile.DeviceProfileId, deviceProfileId));
        if (original is null)
        {
            return null;
        }

        var deviceOverride = LoadOverrides()
            .FirstOrDefault(profile => IsSameDevice(profile.DeviceProfileId, deviceProfileId));

        return deviceOverride is null
            ? original
            : Merge(original, deviceOverride);
    }

    public IReadOnlyList<DeviceTechnicalProfileOverride> LoadOverrides()
    {
        if (!File.Exists(_overrideCatalogPath))
        {
            return Array.Empty<DeviceTechnicalProfileOverride>();
        }

        try
        {
            using var stream = File.OpenRead(_overrideCatalogPath);
            var catalog = JsonSerializer.Deserialize<DeviceTechnicalProfileOverrideCatalog>(stream, JsonOptions);
            return catalog?.Profiles ?? Array.Empty<DeviceTechnicalProfileOverride>();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or NotSupportedException)
        {
            return Array.Empty<DeviceTechnicalProfileOverride>();
        }
    }

    public void SaveOverride(DeviceTechnicalProfileOverride deviceOverride)
    {
        ArgumentNullException.ThrowIfNull(deviceOverride);
        if (string.IsNullOrWhiteSpace(deviceOverride.DeviceProfileId))
        {
            throw new ArgumentException("Device profile id must not be empty.", nameof(deviceOverride));
        }

        var overrides = LoadOverrides()
            .Where(profile => !IsSameDevice(profile.DeviceProfileId, deviceOverride.DeviceProfileId))
            .Append(deviceOverride)
            .OrderBy(profile => profile.DeviceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        SaveOverrides(overrides);
    }

    public void SaveTechnicianNote(string deviceProfileId, string technicianNote)
    {
        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            throw new ArgumentException("Device profile id must not be empty.", nameof(deviceProfileId));
        }

        var existing = LoadOverrides()
            .FirstOrDefault(profile => IsSameDevice(profile.DeviceProfileId, deviceProfileId));

        SaveOverride(existing is null
            ? new DeviceTechnicalProfileOverride(deviceProfileId, TechnicianNote: technicianNote ?? string.Empty)
            : existing with { TechnicianNote = technicianNote ?? string.Empty });
    }

    public void ResetOverride(string deviceProfileId)
    {
        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            return;
        }

        var overrides = LoadOverrides()
            .Where(profile => !IsSameDevice(profile.DeviceProfileId, deviceProfileId))
            .OrderBy(profile => profile.DeviceProfileId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        SaveOverrides(overrides);
    }

    private void SaveOverrides(IReadOnlyList<DeviceTechnicalProfileOverride> overrides)
    {
        var directory = Path.GetDirectoryName(_overrideCatalogPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var catalog = new DeviceTechnicalProfileOverrideCatalog(overrides);
        using var stream = File.Create(_overrideCatalogPath);
        JsonSerializer.Serialize(stream, catalog, JsonOptions);
    }

    private static DeviceTechnicalProfile Merge(
        DeviceTechnicalProfile original,
        DeviceTechnicalProfileOverride deviceOverride)
    {
        return original with
        {
            ShortDescription = string.IsNullOrWhiteSpace(deviceOverride.ShortDescription)
                ? original.ShortDescription
                : deviceOverride.ShortDescription.Trim(),
            Measurements = deviceOverride.Measurements ?? original.Measurements,
            ExampleMeasurements = deviceOverride.ExampleMeasurements ?? original.ExampleMeasurements,
            TechnicalNotes = deviceOverride.TechnicalNotes ?? original.TechnicalNotes,
            Limitations = deviceOverride.Limitations ?? original.Limitations,
            TechnicianNote = deviceOverride.TechnicianNote ?? string.Empty
        };
    }

    private static bool IsSameDevice(string? left, string? right)
    {
        return !string.IsNullOrWhiteSpace(left)
            && !string.IsNullOrWhiteSpace(right)
            && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}
