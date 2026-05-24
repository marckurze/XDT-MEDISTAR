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
    private const string TopconCl300DefaultDeviceProfileId = "device-topcon-cl300-default";
    private const string TopconCl300DefaultExportProfileId = "export-medistar-topcon-cl300-default";
    private const string TopconKr800DefaultExportProfileId = "export-medistar-topcon-kr800-default";
    private const string TopconTrk2PDefaultDeviceProfileId = "device-topcon-trk2p-default";
    private const string TopconTrk2PDefaultExportProfileId = "export-medistar-topcon-trk2p-default";
    private const string TopconCv5000DefaultDeviceProfileId = "device-topcon-cv5000-default";
    private const string TopconCv5000DefaultExportProfileId = "export-medistar-topcon-cv5000-default";

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

        var devicesFolder = GetDevicesFolder(paths);
        foreach (var profile in CreateDefaultDeviceProfiles())
        {
            SaveDefaultIfMissing(devicesFolder, profile, _repository.SaveDeviceProfileDefinition);
            RepairBuiltInDeviceProfileIfNeeded(devicesFolder, profile);
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
            DefaultDeviceProfileDefinitions.CreateTopconSolosDefault(),
            DefaultDeviceProfileDefinitions.CreateTopconKr800Default(),
            DefaultDeviceProfileDefinitions.CreateTopconKr1Default(),
            DefaultDeviceProfileDefinitions.CreateTopconTrk2PDefault(),
            DefaultDeviceProfileDefinitions.CreateTopconCt1PDefault(),
            DefaultDeviceProfileDefinitions.CreateTopconCt800ADefault(),
            DefaultDeviceProfileDefinitions.CreateTopconCv5000Default(),
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
            DefaultExportProfileDefinitions.CreateMedistarTopconSolosDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconKr800Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconKr1Default(),
            DefaultExportProfileDefinitions.CreateMedistarTopconTrk2PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCt1PDefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCt800ADefault(),
            DefaultExportProfileDefinitions.CreateMedistarTopconCv5000Default(),
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
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconCl300Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconSolosDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr800Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconKr1Default(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconTrk2PDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt1PDefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconCt800ADefault(),
            DefaultInterfaceProfileDefinitions.CreateMedistarTopconCv5000Default(),
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

    private void RepairBuiltInDeviceProfileIfNeeded(string folder, DeviceProfileDefinition defaultProfile)
    {
        if (!string.Equals(defaultProfile.Metadata.Id, TopconCl300DefaultDeviceProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, TopconTrk2PDefaultDeviceProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, TopconCv5000DefaultDeviceProfileId, StringComparison.Ordinal))
        {
            return;
        }

        var filePath = CreateProfilePath(folder, defaultProfile.Metadata.Id);
        if (!File.Exists(filePath))
        {
            return;
        }

        var existingProfile = _repository.LoadDeviceProfileDefinition(filePath);
        if (!existingProfile.Metadata.IsBuiltIn || existingProfile.Metadata.IsUserDefined)
        {
            return;
        }

        if (!NeedsBuiltInDeviceProfileRepair(existingProfile))
        {
            return;
        }

        _repository.SaveDeviceProfileDefinition(filePath, defaultProfile);
    }

    private void RepairBuiltInExportProfileIfNeeded(string folder, ExportProfileDefinition defaultProfile)
    {
        if (!string.Equals(defaultProfile.Metadata.Id, Lm7DefaultExportProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, Nt530PDefaultExportProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, TopconCl300DefaultExportProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, TopconKr800DefaultExportProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, TopconTrk2PDefaultExportProfileId, StringComparison.Ordinal)
            && !string.Equals(defaultProfile.Metadata.Id, TopconCv5000DefaultExportProfileId, StringComparison.Ordinal))
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
            || NeedsNt530PExportProfileRepair(profile)
            || NeedsTopconCl300ExportProfileRepair(profile)
            || NeedsTopconKr800SExportProfileRepair(profile)
            || NeedsTopconTrk2PExportProfileRepair(profile)
            || NeedsTopconCv5000ExportProfileRepair(profile);
    }

    private static bool NeedsBuiltInDeviceProfileRepair(DeviceProfileDefinition profile)
    {
        return NeedsTopconCl300DeviceProfileRepair(profile)
            || NeedsTopconTrk2PDeviceProfileRepair(profile)
            || NeedsTopconCv5000DeviceProfileRepair(profile);
    }

    private static bool NeedsTopconCl300DeviceProfileRepair(DeviceProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconCl300DefaultDeviceProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Measurements.Any(measurement =>
            measurement.SourcePath.Contains("Ophthalmology/", StringComparison.OrdinalIgnoreCase))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='LM']/LM/R/MedistarLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='LM']/LM/L/MedistarLine",
                StringComparison.Ordinal));
    }

    private static bool NeedsTopconTrk2PDeviceProfileRepair(DeviceProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconTrk2PDefaultDeviceProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Measurements.Any(measurement =>
            measurement.SourcePath.Contains("Ophthalmology/", StringComparison.OrdinalIgnoreCase))
            || !string.Equals(profile.Model, "TRK-2P", StringComparison.Ordinal)
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='REF']/REF/R/MedistarLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/TonoListLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/HeaderLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/MeasuredRightLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/ParameterRightLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/MeasuredLeftLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/ParameterLeftLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='CCT']/Pachy/HeaderLine",
                StringComparison.Ordinal))
            || profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='TM']/Tono/CorrectedLine",
                StringComparison.Ordinal));
    }

    private static bool NeedsTopconCv5000DeviceProfileRepair(DeviceProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconCv5000DefaultDeviceProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Measurements.Any(measurement =>
            measurement.SourcePath.StartsWith("Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='SBJ']/Prescription/HeaderLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='SBJ']/Prescription/R/MedistarLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='SBJ']/FullCorrection/HeaderLine",
                StringComparison.Ordinal))
            || !profile.Measurements.Any(measurement => string.Equals(
                measurement.SourcePath,
                "Measure[@Type='SBJ']/FullCorrection/R/MedistarLine",
                StringComparison.Ordinal));
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

    private static bool NeedsTopconCl300ExportProfileRepair(ExportProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconCl300DefaultExportProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Rules.Any(rule =>
            string.Equals(rule.TargetFieldCode, "6205", StringComparison.Ordinal)
            || string.Equals(rule.TargetFieldCode, "6220", StringComparison.Ordinal)
            || ContainsLegacyTopconCl300Path(rule.SourcePath)
            || ContainsLegacyTopconCl300Path(rule.OutputTemplate))
            || !profile.Rules.Any(rule => string.Equals(
                rule.SourcePath,
                "Device.Measure[@Type='LM']/LM/R/MedistarLine",
                StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.SourcePath,
                "Device.Measure[@Type='LM']/LM/L/MedistarLine",
                StringComparison.Ordinal));
    }

    private static bool ContainsLegacyTopconCl300Path(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && value.Contains("Device.Ophthalmology/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool NeedsTopconKr800SExportProfileRepair(ExportProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconKr800DefaultExportProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Rules.Any(rule =>
            IsLegacyTopconKr800SKeratometryRule(rule)
            || ContainsLegacyTopconKr800SPath(rule.SourcePath)
            || ContainsLegacyTopconKr800SPath(rule.OutputTemplate))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6228",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='REF']/REF/R/MedistarLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6228",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='REF']/REF/L/MedistarLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6221",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='KM']/KM/MedistarLine1",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6221",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='KM']/KM/MedistarLine2",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6227",
                StringComparison.Ordinal)
                && string.Equals(
                rule.SourcePath,
                "Device.Measure[@Type='SBJ']/MedistarLine1",
                StringComparison.Ordinal));
    }

    private static bool NeedsTopconTrk2PExportProfileRepair(ExportProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconTrk2PDefaultExportProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Rules.Any(rule =>
            ContainsLegacyTopconTrk2PPath(rule.SourcePath)
            || ContainsLegacyTopconTrk2PPath(rule.OutputTemplate)
            || IsLegacyTopconTrk2PFieldRule(rule))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6228",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='REF']/REF/R/MedistarLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6221",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='KM']/KM/MedistarLine1",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6220",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='CCT']/Pachy/MedistarLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6205",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='TM']/Tono/TonoListLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6220",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='CCT']/Pachy/HeaderLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6205",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='TM']/Tono/HeaderLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6205",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='TM']/Tono/MeasuredRightLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6205",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='TM']/Tono/ParameterRightLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6205",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='TM']/Tono/MeasuredLeftLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6205",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='TM']/Tono/ParameterLeftLine",
                    StringComparison.Ordinal));
    }

    private static bool NeedsTopconCv5000ExportProfileRepair(ExportProfileDefinition profile)
    {
        if (!string.Equals(profile.Metadata.Id, TopconCv5000DefaultExportProfileId, StringComparison.Ordinal))
        {
            return false;
        }

        return profile.Rules.Any(rule =>
            !string.IsNullOrWhiteSpace(rule.SourcePath)
            && rule.SourcePath.Contains("Device.Measure[@Type='SBJ']/MedistarLine", StringComparison.Ordinal))
            || profile.Rules.Any(rule =>
                string.Equals(rule.TargetFieldCode, "6228", StringComparison.Ordinal)
                && string.Equals(rule.TargetName, "PhoropterSeparator", StringComparison.Ordinal))
            || profile.Rules.Any(rule =>
                string.Equals(rule.TargetFieldCode, "6330", StringComparison.Ordinal)
                && !string.IsNullOrWhiteSpace(rule.SourcePath)
                && rule.SourcePath.Contains("Device.Measure[@Type='SBJ']/FullCorrection", StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6228",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='SBJ']/Prescription/HeaderLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6228",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='SBJ']/Prescription/R/MedistarLine",
                    StringComparison.Ordinal))
            || !profile.Rules.Any(rule => string.Equals(
                rule.TargetFieldCode,
                "6227",
                StringComparison.Ordinal)
                && string.Equals(
                    rule.SourcePath,
                    "Device.Measure[@Type='SBJ']/FullCorrection/R/MedistarLine",
                    StringComparison.Ordinal));
    }

    private static bool IsLegacyTopconTrk2PFieldRule(ExportRuleDefinition rule)
    {
        return string.Equals(rule.TargetFieldCode, "6228", StringComparison.Ordinal)
            && ContainsAny(rule.TargetName, "Tonometry", "Pachymetry", "Tono", "Pachy", "mmHg", "µm");
    }

    private static bool ContainsLegacyTopconTrk2PPath(string? value)
    {
        return ContainsAny(
            value,
            "Device.Ophthalmology/",
            ":Iop",
            ":Pachy",
            "Device.Measure[@Type='TM']/Tono/CorrectedLine",
            "Measure[@Type='TM']/Tono/CorrectedLine");
    }

    private static bool IsLegacyTopconKr800SKeratometryRule(ExportRuleDefinition rule)
    {
        if (!string.Equals(rule.TargetFieldCode, "6228", StringComparison.Ordinal))
        {
            return false;
        }

        return ContainsAny(rule.TargetName, "Keratometry", "Kerato", "KM", "KR:", "KL:", "K1=", "K2=")
            || ContainsAny(rule.Description, "Keratometry", "Kerato", "KM", "KR:", "KL:", "K1=", "K2=")
            || ContainsAny(rule.SourcePath, "Measure[@Type='KM']", "Measure[@type='KM']", "Keratometry", "K1=", "K2=")
            || ContainsAny(rule.OutputTemplate, "Measure[@Type='KM']", "Measure[@type='KM']", "KR:", "KL:", "K1=", "K2=");
    }

    private static bool ContainsLegacyTopconKr800SPath(string? value)
    {
        return ContainsAny(
            value,
            "Device.Ophthalmology/",
            "R.:S= Z=*",
            "L.:S= Z=*",
            "KR: K1=*",
            "KL: K1=*");
    }

    private static bool ContainsAny(string? value, params string[] needles)
    {
        return !string.IsNullOrWhiteSpace(value)
            && needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
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
