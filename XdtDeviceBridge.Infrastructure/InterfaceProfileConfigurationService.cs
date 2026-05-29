using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class InterfaceProfileConfigurationService
{
    private readonly FolderSafetyValidator _folderSafetyValidator;

    public InterfaceProfileConfigurationService()
        : this(new FolderSafetyValidator())
    {
    }

    public InterfaceProfileConfigurationService(FolderSafetyValidator folderSafetyValidator)
    {
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
    }

    public InterfaceProfileDefinition CreateForExportProfile(
        ExportProfileDefinition exportProfile,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(exportProfile);

        var id = idFactory?.Invoke();
        if (string.IsNullOrWhiteSpace(id))
        {
            id = $"interface-{Guid.NewGuid():N}";
        }

        return new InterfaceProfileDefinition(
            Metadata: new ProfileMetadata(
                Id: id,
                Name: $"Schnittstelle - {exportProfile.Metadata.Name}",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: $"Benutzerdefinierte Schnittstelle für {exportProfile.Metadata.Name}",
                Vendor: exportProfile.Metadata.Vendor,
                Product: exportProfile.Metadata.Product,
                Version: "1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: createdBy,
                IsBuiltIn: false,
                IsUserDefined: true),
            AisProfileId: exportProfile.TargetAisProfileId,
            DeviceProfileId: exportProfile.SourceDeviceProfileId,
            ExportProfileId: exportProfile.Metadata.Id,
            FolderOptions: CreateEmptyFolderOptions(),
            IsActive: false,
            IsLicenseRequired: true,
            Description: "Benutzerdefinierte Schnittstellenkonfiguration. Automatische Verarbeitung ist zunächst deaktiviert.",
            DeviceOutput: CreateDefaultDeviceOutputForExportProfile(exportProfile));
    }

    public InterfaceProfileConfigurationResult CreateConfiguredProfile(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        return CreateConfiguredProfile(
            originalProfile,
            folderOptions,
            isActive,
            isLicenseRequired,
            originalProfile.DeviceOutput,
            originalProfile.SerialSettings,
            originalProfile.NidekRtSerialSendMode,
            timestamp,
            createdBy,
            idFactory);
    }

    public InterfaceProfileConfigurationResult CreateConfiguredProfile(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
        DeviceOutputConfiguration? deviceOutput,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        return CreateConfiguredProfile(
            originalProfile,
            folderOptions,
            isActive,
            isLicenseRequired,
            deviceOutput,
            originalProfile.SerialSettings,
            originalProfile.NidekRtSerialSendMode,
            timestamp,
            createdBy,
            idFactory);
    }

    public InterfaceProfileConfigurationResult CreateConfiguredProfile(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
        DeviceOutputConfiguration? deviceOutput,
        SerialCommunicationSettings? serialSettings,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        return CreateConfiguredProfile(
            originalProfile,
            folderOptions,
            isActive,
            isLicenseRequired,
            deviceOutput,
            serialSettings,
            originalProfile.NidekRtSerialSendMode,
            timestamp,
            createdBy,
            idFactory);
    }

    public InterfaceProfileConfigurationResult CreateConfiguredProfile(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
        DeviceOutputConfiguration? deviceOutput,
        SerialCommunicationSettings? serialSettings,
        NidekRtSerialSendMode? nidekRtSerialSendMode,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(originalProfile);
        ArgumentNullException.ThrowIfNull(folderOptions);

        var profile = originalProfile.Metadata.IsBuiltIn
            ? CreateUserDefinedCopy(originalProfile, folderOptions, isActive, isLicenseRequired, deviceOutput, serialSettings, nidekRtSerialSendMode, timestamp, createdBy, idFactory)
            : UpdateUserDefinedProfile(originalProfile, folderOptions, isActive, isLicenseRequired, deviceOutput, serialSettings, nidekRtSerialSendMode, timestamp);

        var issues = ValidateConfiguration(profile);
        return issues.Any(issue => issue.Severity == InterfaceProfileConfigurationIssueSeverity.Error)
            ? new InterfaceProfileConfigurationResult(null, issues)
            : new InterfaceProfileConfigurationResult(profile, issues);
    }

    public IReadOnlyList<InterfaceProfileConfigurationIssue> ValidateConfiguration(InterfaceProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var issues = new List<InterfaceProfileConfigurationIssue>();
        issues.AddRange(InterfaceProfileDefinitionValidator.Validate(profile)
            .Select(issue => new InterfaceProfileConfigurationIssue(
                InterfaceProfileConfigurationIssueSeverity.Error,
                issue)));

        var folderSafetyResult = _folderSafetyValidator.ValidateInterfaceFolderOptions(profile.FolderOptions);
        issues.AddRange(folderSafetyResult.Issues.Select(issue => new InterfaceProfileConfigurationIssue(
            issue.Severity == FolderSafetyValidationIssueSeverity.Error
                ? InterfaceProfileConfigurationIssueSeverity.Error
                : InterfaceProfileConfigurationIssueSeverity.Warning,
            issue.Message,
            issue.Path)));

        if (!AnyCleanupOptionEnabled(profile.FolderOptions))
        {
            AddMissingFolderWarnings(profile, issues);
        }

        if (profile.DeviceOutput?.IsEnabled == true)
        {
            if (string.IsNullOrWhiteSpace(profile.DeviceOutput.OutputFolder))
            {
                issues.Add(new InterfaceProfileConfigurationIssue(
                    InterfaceProfileConfigurationIssueSeverity.Warning,
                    "Ausgabeordner an Gerät fehlt. Es wird keine CV-5000-Importdatei geschrieben, bis der Ordner gesetzt ist."));
            }
            else
            {
                AddMissingFolderWarning(profile.DeviceOutput.OutputFolder, "Ausgabeordner an Gerät existiert aktuell nicht.", issues);
            }
        }

        return issues;
    }

    private static InterfaceProfileDefinition CreateUserDefinedCopy(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
        DeviceOutputConfiguration? deviceOutput,
        SerialCommunicationSettings? serialSettings,
        NidekRtSerialSendMode? nidekRtSerialSendMode,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory)
    {
        var id = idFactory?.Invoke();
        if (string.IsNullOrWhiteSpace(id))
        {
            id = $"interface-{Guid.NewGuid():N}";
        }

        return originalProfile with
        {
            Metadata = new ProfileMetadata(
                Id: id,
                Name: $"{originalProfile.Metadata.Name} - Konfiguration",
                ProfileKind: ProfileKind.InterfaceProfile,
                Description: $"Benutzerdefinierte Konfiguration von {originalProfile.Metadata.Name}",
                Vendor: originalProfile.Metadata.Vendor,
                Product: originalProfile.Metadata.Product,
                Version: "1.0",
                CreatedAt: timestamp,
                UpdatedAt: timestamp,
                CreatedBy: createdBy,
                IsBuiltIn: false,
                IsUserDefined: true),
            FolderOptions = folderOptions,
            IsActive = isActive,
            IsLicenseRequired = isLicenseRequired,
            DeviceOutput = deviceOutput,
            SerialSettings = serialSettings,
            NidekRtSerialSendMode = nidekRtSerialSendMode
        };
    }

    private static InterfaceProfileDefinition UpdateUserDefinedProfile(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
        DeviceOutputConfiguration? deviceOutput,
        SerialCommunicationSettings? serialSettings,
        NidekRtSerialSendMode? nidekRtSerialSendMode,
        DateTimeOffset timestamp)
    {
        return originalProfile with
        {
            Metadata = originalProfile.Metadata with
            {
                UpdatedAt = timestamp,
                IsBuiltIn = false,
                IsUserDefined = true
            },
            FolderOptions = folderOptions,
            IsActive = isActive,
            IsLicenseRequired = isLicenseRequired,
            DeviceOutput = deviceOutput,
            SerialSettings = serialSettings,
            NidekRtSerialSendMode = nidekRtSerialSendMode
        };
    }

    private static DeviceOutputConfiguration? CreateDefaultDeviceOutputForExportProfile(ExportProfileDefinition exportProfile)
    {
        if (string.Equals(exportProfile.SourceDeviceProfileId, "device-topcon-cv5000-default", StringComparison.OrdinalIgnoreCase)
            || string.Equals(exportProfile.Metadata.Id, "export-medistar-topcon-cv5000-default", StringComparison.OrdinalIgnoreCase))
        {
            return new DeviceOutputConfiguration(
                IsEnabled: false,
                OutputFolder: string.Empty,
                FileNameTemplate: "CVImport.xml",
                Format: "TOPCON CV-5000 XML");
        }

        if (string.Equals(exportProfile.SourceDeviceProfileId, "device-nidek-rt6100-default", StringComparison.OrdinalIgnoreCase)
            || string.Equals(exportProfile.Metadata.Id, "export-medistar-nidek-rt6100-default", StringComparison.OrdinalIgnoreCase))
        {
            return new DeviceOutputConfiguration(
                IsEnabled: false,
                OutputFolder: string.Empty,
                FileNameTemplate: NidekRt6100InputXmlWriter.DefaultFileNameTemplate,
                Format: NidekRt6100InputXmlWriter.DeviceOutputFormat);
        }

        return null;
    }

    private static InterfaceFolderOptions CreateEmptyFolderOptions()
    {
        return new InterfaceFolderOptions(
            AisImportFolder: string.Empty,
            DeviceImportFolder: string.Empty,
            ExportFolder: string.Empty,
            ArchiveFolder: string.Empty,
            ErrorFolder: string.Empty,
            ClearAisImportFolderBeforeProcessing: false,
            ClearDeviceImportFolderBeforeProcessing: false,
            ClearExportFolderAfterSuccessfulTransfer: false,
            ArchiveProcessedFiles: false,
            MoveFailedFilesToErrorFolder: true);
    }

    private static bool AnyCleanupOptionEnabled(InterfaceFolderOptions options)
    {
        return options.ClearAisImportFolderBeforeProcessing
            || options.ClearDeviceImportFolderBeforeProcessing
            || options.ClearExportFolderAfterSuccessfulTransfer;
    }

    private static void AddMissingFolderWarnings(
        InterfaceProfileDefinition profile,
        List<InterfaceProfileConfigurationIssue> issues)
    {
        var options = profile.FolderOptions;
        AddMissingFolderWarning(options.AisImportFolder, "AIS-Importordner existiert aktuell nicht.", issues);
        if (profile.SerialSettings is null
            && options.AttachmentOnlySourceMode != AttachmentOnlySourceMode.ManualUserSelection)
        {
            AddMissingFolderWarning(options.DeviceImportFolder, "Geräte-Importordner existiert aktuell nicht.", issues);
        }
        AddMissingFolderWarning(options.ExportFolder, "Exportordner existiert aktuell nicht.", issues);
        AddMissingFolderWarning(options.ArchiveFolder, "Archivordner existiert aktuell nicht.", issues);
        AddMissingFolderWarning(options.ErrorFolder, "Fehlerordner existiert aktuell nicht.", issues);
        AddMissingFolderWarning(options.AttachmentImportFolder, "GA-Dateianhang Import existiert aktuell nicht.", issues);
        AddMissingFolderWarning(options.AttachmentExportFolder, "GA-Dateianhang Export existiert aktuell nicht.", issues);
    }

    private static void AddMissingFolderWarning(
        string folderPath,
        string message,
        List<InterfaceProfileConfigurationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        if (IsUncPath(folderPath))
        {
            return;
        }

        try
        {
            if (Path.IsPathFullyQualified(folderPath) && !Directory.Exists(folderPath))
            {
                issues.Add(new InterfaceProfileConfigurationIssue(
                    InterfaceProfileConfigurationIssueSeverity.Warning,
                    message,
                    Path.GetFullPath(folderPath)));
            }
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            issues.Add(new InterfaceProfileConfigurationIssue(
                InterfaceProfileConfigurationIssueSeverity.Warning,
                $"Ordnerpfad konnte nicht geprüft werden: {ex.Message}",
                folderPath));
        }
    }

    private static bool IsUncPath(string folderPath)
    {
        return folderPath.StartsWith(@"\\", StringComparison.Ordinal);
    }
}
