using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class ProfileCatalogService
{
    private const string AisFolderName = "ais";
    private const string DevicesFolderName = "devices";
    private const string ExportsFolderName = "exports";
    private const string InterfacesFolderName = "interfaces";

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

        foreach (var profile in CreateDefaultExportProfiles())
        {
            SaveDefaultIfMissing(GetExportsFolder(paths), profile, _repository.SaveExportProfileDefinition);
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
            DefaultDeviceProfileDefinitions.CreateNidekLm7Default(),
            DefaultDeviceProfileDefinitions.CreateNidekNt530PDefault(),
            DefaultDeviceProfileDefinitions.CreateTopconCl300Default(),
            DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
            DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault()
        };
    }

    private static IReadOnlyList<ExportProfileDefinition> CreateDefaultExportProfiles()
    {
        return new[]
        {
            DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(),
            DefaultExportProfileDefinitions.CreateMedistarNidekLm7Default(),
            DefaultExportProfileDefinitions.CreateMedistarNidekNt530PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCl300Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault()
        };
    }

    private static IReadOnlyList<InterfaceProfileDefinition> CreateDefaultInterfaceProfiles()
    {
        return new[]
        {
            DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault()
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
