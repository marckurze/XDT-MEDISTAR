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
            Description: "Benutzerdefinierte Schnittstellenkonfiguration. Automatische Verarbeitung ist zunächst deaktiviert.");
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
        ArgumentNullException.ThrowIfNull(originalProfile);
        ArgumentNullException.ThrowIfNull(folderOptions);

        var profile = originalProfile.Metadata.IsBuiltIn
            ? CreateUserDefinedCopy(originalProfile, folderOptions, isActive, isLicenseRequired, timestamp, createdBy, idFactory)
            : UpdateUserDefinedProfile(originalProfile, folderOptions, isActive, isLicenseRequired, timestamp);

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
            AddMissingFolderWarnings(profile.FolderOptions, issues);
        }

        return issues;
    }

    private static InterfaceProfileDefinition CreateUserDefinedCopy(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
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
            IsLicenseRequired = isLicenseRequired
        };
    }

    private static InterfaceProfileDefinition UpdateUserDefinedProfile(
        InterfaceProfileDefinition originalProfile,
        InterfaceFolderOptions folderOptions,
        bool isActive,
        bool isLicenseRequired,
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
            IsLicenseRequired = isLicenseRequired
        };
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
        InterfaceFolderOptions options,
        List<InterfaceProfileConfigurationIssue> issues)
    {
        AddMissingFolderWarning(options.AisImportFolder, "AIS-Importordner existiert aktuell nicht.", issues);
        AddMissingFolderWarning(options.DeviceImportFolder, "Geräte-Importordner existiert aktuell nicht.", issues);
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
