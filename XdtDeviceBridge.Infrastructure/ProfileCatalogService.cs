using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ProfileCatalogService
{
    private const string AisFolderName = "ais";
    private const string DevicesFolderName = "devices";
    private const string ExportsFolderName = "exports";
    private const string InterfacesFolderName = "interfaces";
    private const string Lm7DefaultExportProfileId = "export-medistar-nidek-lm7-default";
    private const string Nt530PDefaultExportProfileId = "export-medistar-nidek-nt530p-default";

    private readonly ProfileFileRepository _repository;

    public ProfileCatalogService()
        : this(new ProfileFileRepository())
    {
    }

    public ProfileCatalogService(ProfileFileRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public ProfileCatalog Load(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        return new ProfileCatalog(
            AisProfiles: LoadProfiles(GetAisFolder(paths), _repository.LoadAisProfile),
            DeviceProfiles: LoadProfiles(GetDevicesFolder(paths), _repository.LoadDeviceProfileDefinition),
            ExportProfiles: LoadProfiles(GetExportsFolder(paths), _repository.LoadExportProfileDefinition),
            InterfaceProfiles: LoadProfiles(GetInterfacesFolder(paths), _repository.LoadInterfaceProfileDefinition));
    }

    public void Save(AppDataPaths paths, ProfileCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(catalog);

        foreach (var profile in catalog.AisProfiles)
        {
            _repository.SaveAisProfile(CreateProfilePath(GetAisFolder(paths), profile.Metadata.Id), profile);
        }

        foreach (var profile in catalog.DeviceProfiles)
        {
            _repository.SaveDeviceProfileDefinition(CreateProfilePath(GetDevicesFolder(paths), profile.Metadata.Id), profile);
        }

        foreach (var profile in catalog.ExportProfiles)
        {
            _repository.SaveExportProfileDefinition(CreateProfilePath(GetExportsFolder(paths), profile.Metadata.Id), profile);
        }

        foreach (var profile in catalog.InterfaceProfiles)
        {
            _repository.SaveInterfaceProfileDefinition(CreateProfilePath(GetInterfacesFolder(paths), profile.Metadata.Id), profile);
        }
    }

    public void SaveNewExportProfile(AppDataPaths paths, ExportProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        var filePath = CreateProfilePath(GetExportsFolder(paths), profile.Metadata.Id);
        if (File.Exists(filePath))
        {
            throw new InvalidOperationException($"Export profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveExportProfileDefinition(filePath, profile);
    }

    public void SaveExportProfileDefinition(AppDataPaths paths, ExportProfileDefinition profile, bool overwriteExisting)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.Metadata.IsBuiltIn)
        {
            throw new InvalidOperationException("Built-in export profiles cannot be overwritten.");
        }

        var filePath = CreateProfilePath(GetExportsFolder(paths), profile.Metadata.Id);
        if (!overwriteExisting && File.Exists(filePath))
        {
            throw new InvalidOperationException($"Export profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveExportProfileDefinition(filePath, profile);
    }

    public void SaveNewAisProfile(AppDataPaths paths, AisProfile profile)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        var filePath = CreateProfilePath(GetAisFolder(paths), profile.Metadata.Id);
        if (File.Exists(filePath))
        {
            throw new InvalidOperationException($"AIS profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveAisProfile(filePath, profile);
    }

    public void SaveAisProfile(AppDataPaths paths, AisProfile profile, bool overwriteExisting)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.Metadata.IsBuiltIn)
        {
            throw new InvalidOperationException("Built-in AIS profiles cannot be overwritten.");
        }

        if (!profile.Metadata.IsUserDefined)
        {
            throw new InvalidOperationException("Only user-defined AIS profiles can be overwritten.");
        }

        var filePath = CreateProfilePath(GetAisFolder(paths), profile.Metadata.Id);
        if (!overwriteExisting && File.Exists(filePath))
        {
            throw new InvalidOperationException($"AIS profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveAisProfile(filePath, profile);
    }

    public void SaveNewDeviceProfileDefinition(AppDataPaths paths, DeviceProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        var filePath = CreateProfilePath(GetDevicesFolder(paths), profile.Metadata.Id);
        if (File.Exists(filePath))
        {
            throw new InvalidOperationException($"Device profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveDeviceProfileDefinition(filePath, profile);
    }

    public void SaveDeviceProfileDefinition(AppDataPaths paths, DeviceProfileDefinition profile, bool overwriteExisting)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        if (profile.Metadata.IsBuiltIn)
        {
            throw new InvalidOperationException("Built-in device profiles cannot be overwritten.");
        }

        if (!profile.Metadata.IsUserDefined)
        {
            throw new InvalidOperationException("Only user-defined device profiles can be overwritten.");
        }

        var filePath = CreateProfilePath(GetDevicesFolder(paths), profile.Metadata.Id);
        if (!overwriteExisting && File.Exists(filePath))
        {
            throw new InvalidOperationException($"Device profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveDeviceProfileDefinition(filePath, profile);
    }

    public void SaveNewInterfaceProfileDefinition(AppDataPaths paths, InterfaceProfileDefinition profile)
    {
        SaveInterfaceProfileDefinition(paths, profile, overwriteExisting: false);
    }

    public void SaveInterfaceProfileDefinition(AppDataPaths paths, InterfaceProfileDefinition profile, bool overwriteExisting)
    {
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(profile);

        var filePath = CreateProfilePath(GetInterfacesFolder(paths), profile.Metadata.Id);
        if (!overwriteExisting && File.Exists(filePath))
        {
            throw new InvalidOperationException($"Interface profile already exists and will not be overwritten: {profile.Metadata.Id}");
        }

        _repository.SaveInterfaceProfileDefinition(filePath, profile);
    }

    public bool DeleteInterfaceProfile(AppDataPaths paths, string interfaceProfileId)
    {
        ArgumentNullException.ThrowIfNull(paths);
        EnsureProfileId(interfaceProfileId);

        var interfacesFolder = GetInterfacesFolder(paths);
        var filePath = CreateProfilePath(interfacesFolder, interfaceProfileId);
        EnsurePathStaysInFolder(interfacesFolder, filePath);

        if (!File.Exists(filePath))
        {
            return false;
        }

        var profile = _repository.LoadInterfaceProfileDefinition(filePath);
        if (profile.Metadata.IsBuiltIn)
        {
            throw new InvalidOperationException("Built-in interface profiles cannot be deleted.");
        }

        File.Delete(filePath);
        return true;
    }

    public bool DeleteExportProfile(AppDataPaths paths, string exportProfileId)
    {
        ArgumentNullException.ThrowIfNull(paths);
        EnsureExportProfileId(exportProfileId);

        var exportsFolder = GetExportsFolder(paths);
        var filePath = CreateProfilePath(exportsFolder, exportProfileId);
        EnsureExportPathStaysInFolder(exportsFolder, filePath);

        if (!File.Exists(filePath))
        {
            return false;
        }

        var profile = _repository.LoadExportProfileDefinition(filePath);
        if (profile.Metadata.IsBuiltIn)
        {
            throw new InvalidOperationException("Built-in export profiles cannot be deleted.");
        }

        if (!profile.Metadata.IsUserDefined)
        {
            throw new InvalidOperationException("Only user-defined export profiles can be deleted.");
        }

        File.Delete(filePath);
        return true;
    }

    public void EnsureDefaultProfiles(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        foreach (var profile in CreateDefaultAisProfiles())
        {
            SaveDefaultIfMissing(GetAisFolder(paths), profile, _repository.SaveAisProfile);
        }

        foreach (var profile in CreateDefaultDeviceProfiles())
        {
            SaveDefaultIfMissing(GetDevicesFolder(paths), profile, _repository.SaveDeviceProfileDefinition);
        }

        var exportsFolder = GetExportsFolder(paths);
        foreach (var profile in CreateDefaultExportProfiles())
        {
            SaveDefaultIfMissing(exportsFolder, profile, _repository.SaveExportProfileDefinition);
            RepairBuiltInExportProfileIfNeeded(exportsFolder, profile);
        }

        foreach (var profile in CreateDefaultInterfaceProfiles())
        {
            SaveDefaultIfMissing(GetInterfacesFolder(paths), profile, _repository.SaveInterfaceProfileDefinition);
        }
    }

    private static IReadOnlyList<AisProfile> CreateDefaultAisProfiles()
    {
        return new[]
        {
            DefaultAisProfiles.CreateMedistarDefault()
        };
    }

    private static IReadOnlyList<DeviceProfileDefinition> CreateDefaultDeviceProfiles()
    {
        return new[]
        {
            DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(),
            DefaultDeviceProfileDefinitions.CreateNidekAr360Default(),
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(),
            DefaultDeviceProfileDefinitions.CreateTopconCl300Default(),
            DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
            DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault(),
            DefaultDeviceProfileDefinitions.CreateDocumentAttachmentDefault(),
            DefaultDeviceProfileDefinitions.CreateManualDocumentSelectionDefault()
        };
    }

    private static IReadOnlyList<ExportProfileDefinition> CreateDefaultExportProfiles()
    {
        return new[]
        {
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekAr360Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            DefaultExportProfileDefinitions.CreateMedistarManualDocumentTransferDefault()
        };
    }

    private static IReadOnlyList<InterfaceProfileDefinition> CreateDefaultInterfaceProfiles()
    {
        return new[]
        {
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekAr360Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekLm7Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekNt530PDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarDocumentAttachmentDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarManualDocumentTransferDefault()
        };
    }

    private static IReadOnlyList<TProfile> LoadProfiles<TProfile>(
        string folder,
        Func<string, TProfile> loadProfile)
    {
        if (!Directory.Exists(folder))
        {
            return Array.Empty<TProfile>();
        }

        var profiles = new List<TProfile>();
        foreach (var filePath in Directory.EnumerateFiles(folder, "*.json").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                profiles.Add(loadProfile(filePath));
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Invalid profile JSON file '{filePath}': {ex.Message}", ex);
            }
        }

        return profiles;
    }

    private static void SaveDefaultIfMissing<TProfile>(
        string folder,
        TProfile profile,
        Action<string, TProfile> saveProfile)
        where TProfile : notnull
    {
        var id = GetProfileId(profile);
        var filePath = CreateProfilePath(folder, id);
        if (File.Exists(filePath))
        {
            return;
        }

        saveProfile(filePath, profile);
    }

    private void RepairBuiltInExportProfileIfNeeded(string folder, ExportProfileDefinition defaultProfile)
    {
        if (!string.Equals(defaultProfile.Metadata.Id, Lm7DefaultExportProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, Nt530PDefaultExportProfileId, StringComparison.Ordinal))
        {
            return;
        }

        var filePath = CreateProfilePath(folder, defaultProfile.Metadata.Id);
        if (!File.Exists(filePath))
        {
            return;
        }

        var existingProfile = _repository.LoadExportProfileDefinition(filePath);
        if (!existingProfile.Metadata.IsBuiltIn || existingProfile.Metadata.IsUserDefined)
        {
            return;
        }

        if (!NeedsBuiltInExportProfileRepair(existingProfile))
        {
            return;
        }

        _repository.SaveExportProfileDefinition(filePath, defaultProfile);
    }

    private static bool NeedsBuiltInExportProfileRepair(ExportProfileDefinition profile)
    {
        return NeedsLm7ExportProfileRepair(profile)
            || NeedsNt530PExportProfileRepair(profile);
    }

    private static bool NeedsLm7ExportProfileRepair(ExportProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, Lm7DefaultExportProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Rules.Any(rule =>
            ContainsLegacyLm7MedianPath(rule.SourcePath)
            || ContainsLegacyLm7MedianPath(rule.OutputTemplate));
    }

    private static bool ContainsLegacyLm7MedianPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && (value.Contains("Device.R/LM/Median/", StringComparison.OrdinalIgnoreCase)
                || value.Contains("Device.L/LM/Median/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool NeedsNt530PExportProfileRepair(ExportProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, Nt530PDefaultExportProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Rules.Any(rule =>
            string.Equals(rule.TargetFieldCode, "6228", StringComparison.Ordinal)
            || ContainsLegacyNt530PPath(rule.SourcePath)
            || ContainsLegacyNt530PPath(rule.OutputTemplate))
            || !profile.Rules.Any(rule => string.Equals(
                rule.SourcePath,
                "Device.Measure[@Type='NT530P']/Tono/HeaderLine",
                StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.SourcePath,
                "Device.Measure[@Type='NT530P']/Pachy/HeaderLine",
                StringComparison.Ordinal));
    }

    private static bool ContainsLegacyNt530PPath(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains("Device.Data/", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetProfileId<TProfile>(TProfile profile)
    {
        return profile switch
        {
            AisProfile aisProfile => aisProfile.Metadata.Id,
            DeviceProfileDefinition deviceProfile => deviceProfile.Metadata.Id,
            ExportProfileDefinition exportProfile => exportProfile.Metadata.Id,
            InterfaceProfileDefinition interfaceProfile => interfaceProfile.Metadata.Id,
            _ => throw new InvalidOperationException($"Unsupported profile type: {typeof(TProfile).Name}")
        };
    }

    private static string CreateProfilePath(string folder, string id)
    {
        return Path.Combine(folder, $"{id}.json");
    }

    private static void EnsureProfileId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Interface profile id must not be empty.", nameof(id));
        }

        var fileName = $"{id}.json";
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Interface profile id must be a safe file name.", nameof(id));
        }
    }

    private static void EnsureExportProfileId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Export profile id must not be empty.", nameof(id));
        }

        var fileName = $"{id}.json";
        if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal))
        {
            throw new ArgumentException("Export profile id must be a safe file name.", nameof(id));
        }
    }

    private static void EnsurePathStaysInFolder(string folder, string filePath)
    {
        var fullFolder = Path.GetFullPath(folder);
        var fullFilePath = Path.GetFullPath(filePath);
        var folderWithSeparator = fullFolder.EndsWith(Path.DirectorySeparatorChar)
            ? fullFolder
            : fullFolder + Path.DirectorySeparatorChar;

        if (!fullFilePath.StartsWith(folderWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Interface profile path must stay inside the interfaces profile folder.");
        }
    }

    private static void EnsureExportPathStaysInFolder(string folder, string filePath)
    {
        var fullFolder = Path.GetFullPath(folder);
        var fullFilePath = Path.GetFullPath(filePath);
        var folderWithSeparator = fullFolder.EndsWith(Path.DirectorySeparatorChar)
            ? fullFolder
            : fullFolder + Path.DirectorySeparatorChar;

        if (!fullFilePath.StartsWith(folderWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Export profile path must stay inside the exports profile folder.");
        }
    }

    private static string GetAisFolder(AppDataPaths paths)
    {
        return Path.Combine(paths.ProfilesFolder, AisFolderName);
    }

    private static string GetDevicesFolder(AppDataPaths paths)
    {
        return Path.Combine(paths.ProfilesFolder, DevicesFolderName);
    }

    private static string GetExportsFolder(AppDataPaths paths)
    {
        return Path.Combine(paths.ProfilesFolder, ExportsFolderName);
    }

    private static string GetInterfacesFolder(AppDataPaths paths)
    {
        return Path.Combine(paths.ProfilesFolder, InterfacesFolderName);
    }
}
