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

    public void EnsureDefaultProfiles(AppDataPaths paths)
    {
        ArgumentNullException.ThrowIfNull(paths);

        SaveDefaultIfMissing(GetAisFolder(paths), DefaultAisProfiles.CreateMedistarDefault(), _repository.SaveAisProfile);
        SaveDefaultIfMissing(GetDevicesFolder(paths), DefaultDeviceProfileDefinitions.CreateNidekArk1sDefault(), _repository.SaveDeviceProfileDefinition);
        SaveDefaultIfMissing(GetExportsFolder(paths), DefaultExportProfileDefinitions.CreateMedistarNidekArk1sDefault(), _repository.SaveExportProfileDefinition);
        SaveDefaultIfMissing(GetInterfacesFolder(paths), DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault(), _repository.SaveInterfaceProfileDefinition);
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
