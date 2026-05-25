using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed record DeviceProfileImageManagementItem(
    string DeviceProfileId,
    string ProfileName,
    string Manufacturer,
    string Model,
    string DeviceType,
    string ParserMode,
    bool IsBidirectional,
    bool IsBuiltIn,
    string EffectiveImagePath,
    bool HasLocalOverride)
{
    public string ProfileKindDisplay => IsBuiltIn ? "BuiltIn" : "UserDefined";
}

public sealed record DeviceProfileImageUpdateResult(
    bool Success,
    string? ImagePath,
    string? ErrorMessage);

public sealed class DeviceProfileImageOverrideService
{
    private const string DeviceImagesFolderName = "DeviceImages";
    private const string OverridesFileName = "device-image-overrides.json";

    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png",
        ".jpg",
        ".jpeg"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public IReadOnlyDictionary<string, string> LoadOverrides(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var filePath = GetOverridesFilePath(paths);
        if (!File.Exists(filePath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var raw = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            return raw
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (IOException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
        catch (UnauthorizedAccessException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public IReadOnlyList<DeviceProfileImageManagementItem> BuildManagementItems(
        AppDataPaths paths,
        IEnumerable<DeviceProfileDefinition> deviceProfiles)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(deviceProfiles);

        var overrides = LoadOverrides(paths);
        return deviceProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase)
            .Select(profile =>
            {
                overrides.TryGetValue(profile.Metadata.Id, out var overridePath);
                return new DeviceProfileImageManagementItem(
                    DeviceProfileId: profile.Metadata.Id,
                    ProfileName: profile.Metadata.Name,
                    Manufacturer: profile.Manufacturer,
                    Model: profile.Model,
                    DeviceType: profile.DeviceType,
                    ParserMode: profile.ParserMode,
                    IsBidirectional: profile.IsBidirectional,
                    IsBuiltIn: profile.Metadata.IsBuiltIn,
                    EffectiveImagePath: ResolveEffectiveImagePath(profile, overridePath),
                    HasLocalOverride: HasExistingLocalOverride(overridePath));
            })
            .ToList();
    }

    public DeviceProfileImageUpdateResult SaveImageOverride(
        AppDataPaths paths,
        string deviceProfileId,
        string sourceImagePath)
    {
        ArgumentNullException.ThrowIfNull(paths);

        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            return new DeviceProfileImageUpdateResult(false, null, "Geräteprofil-ID fehlt.");
        }

        if (string.IsNullOrWhiteSpace(sourceImagePath))
        {
            return new DeviceProfileImageUpdateResult(false, null, "Bildpfad fehlt.");
        }

        if (!File.Exists(sourceImagePath))
        {
            return new DeviceProfileImageUpdateResult(false, null, "Bilddatei wurde nicht gefunden.");
        }

        var extension = Path.GetExtension(sourceImagePath);
        if (!SupportedImageExtensions.Contains(extension))
        {
            return new DeviceProfileImageUpdateResult(false, null, "Unterstützt werden PNG, JPG und JPEG.");
        }

        var targetFolder = GetDeviceImagesFolder(paths);
        Directory.CreateDirectory(targetFolder);

        var targetPath = Path.Combine(targetFolder, $"{CreateSafeFileStem(deviceProfileId)}{extension.ToLowerInvariant()}");
        try
        {
            File.Copy(sourceImagePath, targetPath, overwrite: true);

            var overrides = LoadOverrides(paths).ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);
            overrides[deviceProfileId] = targetPath;
            SaveOverrides(paths, overrides);

            return new DeviceProfileImageUpdateResult(true, targetPath, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return new DeviceProfileImageUpdateResult(false, null, ex.Message);
        }
    }

    public void RemoveImageOverride(AppDataPaths paths, string deviceProfileId)
    {
        ArgumentNullException.ThrowIfNull(paths);
        if (string.IsNullOrWhiteSpace(deviceProfileId))
        {
            return;
        }

        var overrides = LoadOverrides(paths).ToDictionary(
            pair => pair.Key,
            pair => pair.Value,
            StringComparer.OrdinalIgnoreCase);

        if (!overrides.Remove(deviceProfileId, out var imagePath))
        {
            return;
        }

        SaveOverrides(paths, overrides);
        DeleteLocalImageIfSafe(paths, imagePath);
    }

    public string ResolveEffectiveImagePath(DeviceProfileDefinition profile, string? overridePath)
    {
        ArgumentNullException.ThrowIfNull(profile);

        if (HasExistingLocalOverride(overridePath))
        {
            return overridePath!.Trim();
        }

        return string.IsNullOrWhiteSpace(profile.DeviceImagePath)
            ? string.Empty
            : profile.DeviceImagePath.Trim();
    }

    public bool HasExistingLocalOverride(string? overridePath)
    {
        if (string.IsNullOrWhiteSpace(overridePath))
        {
            return false;
        }

        try
        {
            return File.Exists(overridePath.Trim());
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
    }

    public string GetDeviceImagesFolder(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        return Path.Combine(paths.BaseFolder, DeviceImagesFolderName);
    }

    private void SaveOverrides(AppDataPaths paths, IReadOnlyDictionary<string, string> overrides)
    {
        var filePath = GetOverridesFilePath(paths);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var ordered = overrides
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        File.WriteAllText(filePath, JsonSerializer.Serialize(ordered, JsonOptions));
    }

    private string GetOverridesFilePath(AppDataPaths paths)
    {
        return Path.Combine(paths.BaseFolder, OverridesFileName);
    }

    private static string CreateSafeFileStem(string deviceProfileId)
    {
        var safeChars = deviceProfileId
            .Trim()
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '_')
            .ToArray();
        var fileStem = new string(safeChars);
        return string.IsNullOrWhiteSpace(fileStem)
            ? "device-image"
            : fileStem;
    }

    private void DeleteLocalImageIfSafe(AppDataPaths paths, string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return;
        }

        try
        {
            var imageFolder = Path.GetFullPath(GetDeviceImagesFolder(paths));
            var imageFolderWithSeparator = imageFolder.EndsWith(Path.DirectorySeparatorChar)
                ? imageFolder
                : imageFolder + Path.DirectorySeparatorChar;
            var fullImagePath = Path.GetFullPath(imagePath);

            if (fullImagePath.StartsWith(imageFolderWithSeparator, StringComparison.OrdinalIgnoreCase)
                && File.Exists(fullImagePath))
            {
                File.Delete(fullImagePath);
            }
        }
        catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
        {
            // Lokale Bild-Overrides sind Komfortdaten. Ein Löschproblem darf die Profilverwaltung nicht blockieren.
        }
    }
}
