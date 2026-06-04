using System.Globalization;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public enum ProfileManagementRowKind
{
    AisProfile,
    DeviceProfile,
    ExportProfile,
    InterfaceProfile,
    TemplatePackage,
    XdtBaukastenTemplate,
    Maintenance
}

public sealed record ProfileManagementRow(
    ProfileManagementRowKind Kind,
    string Id,
    string Name,
    string Type,
    string Owner,
    string Scope,
    string ActiveUsage,
    string UsedBy,
    string UpdatedAt,
    string Actions,
    string Details,
    string? FilePath,
    bool IsBuiltIn,
    bool IsUserDefined,
    bool CanRename,
    bool CanDuplicate,
    bool CanDelete,
    bool CanOpenInWorkbench,
    bool CanOpenInInterfaceProfiles);

public sealed record ProfileManagementActionResult(
    bool Success,
    string Message,
    string? AisProfileId = null,
    string? DeviceProfileId = null,
    string? ExportProfileId = null,
    string? InterfaceProfileId = null);

public sealed class ProfileManagementService
{
    private readonly ProfileCatalogService _profileCatalogService;

    public ProfileManagementService()
        : this(new ProfileCatalogService())
    {
    }

    public ProfileManagementService(ProfileCatalogService profileCatalogService)
    {
        _profileCatalogService = profileCatalogService ?? throw new ArgumentNullException(nameof(profileCatalogService));
    }

    public IReadOnlyList<ProfileManagementRow> BuildRows(
        ProfileCatalog catalog,
        AppDataPaths paths,
        XdtBaukastenTemplateLibraryService templateLibraryService)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(templateLibraryService);

        var rows = new List<ProfileManagementRow>();
        rows.AddRange(catalog.AisProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => CreateAisRow(catalog, profile)));
        rows.AddRange(catalog.DeviceProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => CreateDeviceRow(catalog, profile)));
        rows.AddRange(catalog.ExportProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => CreateExportRow(catalog, profile)));
        rows.AddRange(catalog.InterfaceProfiles
            .OrderBy(profile => profile.Metadata.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(profile => CreateInterfaceRow(catalog, profile)));
        rows.AddRange(CreateTemplatePackageRows(paths));
        rows.AddRange(CreateXdtBaukastenTemplateRows(templateLibraryService, paths));
        rows.Add(CreateMaintenanceRow(catalog, paths));

        return rows;
    }

    public ProfileManagementActionResult EvaluateDelete(ProfileCatalog catalog, ProfileManagementRow row)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(row);

        return row.Kind switch
        {
            ProfileManagementRowKind.AisProfile => EvaluateDeleteProfile(
                row,
                "AIS-Profil",
                catalog.InterfaceProfiles
                    .Where(profile => IdEquals(profile.AisProfileId, row.Id))
                    .Select(profile => profile.Metadata.Name)
                    .Concat(catalog.ExportProfiles
                        .Where(profile => IdEquals(profile.TargetAisProfileId, row.Id))
                        .Select(profile => profile.Metadata.Name))
                    .ToList()),
            ProfileManagementRowKind.DeviceProfile => EvaluateDeleteProfile(
                row,
                "Geräteprofil",
                catalog.InterfaceProfiles
                    .Where(profile => IdEquals(profile.DeviceProfileId, row.Id))
                    .Select(profile => profile.Metadata.Name)
                    .Concat(catalog.ExportProfiles
                        .Where(profile => IdEquals(profile.SourceDeviceProfileId, row.Id))
                        .Select(profile => profile.Metadata.Name))
                    .ToList()),
            ProfileManagementRowKind.ExportProfile => EvaluateDeleteProfile(
                row,
                "Exportprofil",
                catalog.InterfaceProfiles
                    .Where(profile => IdEquals(profile.ExportProfileId, row.Id))
                    .Select(profile => profile.Metadata.Name)
                    .ToList()),
            ProfileManagementRowKind.InterfaceProfile when row.IsBuiltIn =>
                new ProfileManagementActionResult(false, "BuiltIn-Schnittstellenprofile können nicht gelöscht werden."),
            ProfileManagementRowKind.InterfaceProfile when !row.IsUserDefined =>
                new ProfileManagementActionResult(false, "Nur UserDefined-Schnittstellenprofile können gelöscht werden."),
            ProfileManagementRowKind.InterfaceProfile when row.ActiveUsage.Equals("Aktiv", StringComparison.OrdinalIgnoreCase) =>
                new ProfileManagementActionResult(false, "Aktive Schnittstellenprofile können nicht ohne vorheriges Deaktivieren gelöscht werden."),
            ProfileManagementRowKind.InterfaceProfile =>
                new ProfileManagementActionResult(true, "Schnittstellenprofil kann gelöscht werden."),
            ProfileManagementRowKind.TemplatePackage or ProfileManagementRowKind.XdtBaukastenTemplate when string.IsNullOrWhiteSpace(row.FilePath) =>
                new ProfileManagementActionResult(false, "Keine lokale Datei für diesen Eintrag gefunden."),
            ProfileManagementRowKind.TemplatePackage or ProfileManagementRowKind.XdtBaukastenTemplate =>
                new ProfileManagementActionResult(true, "Lokale Template-Datei kann gelöscht werden."),
            _ => new ProfileManagementActionResult(false, "Dieser Eintrag kann nicht gelöscht werden.")
        };
    }

    public ProfileManagementActionResult Delete(ProfileCatalog catalog, AppDataPaths paths, ProfileManagementRow row)
    {
        ArgumentNullException.ThrowIfNull(paths);

        var evaluation = EvaluateDelete(catalog, row);
        if (!evaluation.Success)
        {
            return evaluation;
        }

        var deleted = row.Kind switch
        {
            ProfileManagementRowKind.AisProfile => _profileCatalogService.DeleteAisProfile(paths, row.Id),
            ProfileManagementRowKind.DeviceProfile => _profileCatalogService.DeleteDeviceProfileDefinition(paths, row.Id),
            ProfileManagementRowKind.ExportProfile => _profileCatalogService.DeleteExportProfile(paths, row.Id),
            ProfileManagementRowKind.InterfaceProfile => _profileCatalogService.DeleteInterfaceProfile(paths, row.Id),
            ProfileManagementRowKind.TemplatePackage or ProfileManagementRowKind.XdtBaukastenTemplate => DeleteLocalFile(row.FilePath),
            _ => false
        };

        return deleted
            ? new ProfileManagementActionResult(true, $"{row.Type} gelöscht: {row.Name}.")
            : new ProfileManagementActionResult(false, $"{row.Type} wurde nicht gefunden: {row.Name}.");
    }

    public ProfileManagementActionResult Duplicate(
        ProfileCatalog catalog,
        AppDataPaths paths,
        ProfileManagementRow row,
        DateTimeOffset timestamp,
        string? createdBy)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(row);

        return row.Kind switch
        {
            ProfileManagementRowKind.AisProfile => DuplicateAisProfile(catalog, paths, row.Id, timestamp, createdBy),
            ProfileManagementRowKind.DeviceProfile => DuplicateDeviceProfile(catalog, paths, row.Id, timestamp, createdBy),
            ProfileManagementRowKind.ExportProfile => DuplicateExportProfile(catalog, paths, row.Id, timestamp, createdBy),
            ProfileManagementRowKind.InterfaceProfile => DuplicateInterfaceProfile(catalog, paths, row.Id, timestamp, createdBy),
            _ => new ProfileManagementActionResult(false, "Nur AIS-, Geräte-, Export- und Schnittstellenprofile können dupliziert werden.")
        };
    }

    private static ProfileManagementRow CreateAisRow(ProfileCatalog catalog, AisProfile profile)
    {
        var usedBy = catalog.InterfaceProfiles
            .Where(item => IdEquals(item.AisProfileId, profile.Metadata.Id))
            .Select(item => item.Metadata.Name)
            .Concat(catalog.ExportProfiles
                .Where(item => IdEquals(item.TargetAisProfileId, profile.Metadata.Id))
                .Select(item => item.Metadata.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return CreateProfileRow(
            ProfileManagementRowKind.AisProfile,
            profile.Metadata,
            "AIS",
            profile.Vendor,
            usedBy,
            Details: string.Join(Environment.NewLine, new[]
            {
                $"AIS-Profil: {profile.Metadata.Name}",
                $"System: {profile.Name}",
                $"Hersteller: {profile.Vendor}",
                $"Standard-Encoding: {profile.DefaultEncoding}",
                $"8402 erforderlich: {DisplayBool(profile.RequiresExaminationType8402)}"
            }));
    }

    private static ProfileManagementRow CreateDeviceRow(ProfileCatalog catalog, DeviceProfileDefinition profile)
    {
        var usedBy = catalog.InterfaceProfiles
            .Where(item => IdEquals(item.DeviceProfileId, profile.Metadata.Id))
            .Select(item => item.Metadata.Name)
            .Concat(catalog.ExportProfiles
                .Where(item => IdEquals(item.SourceDeviceProfileId, profile.Metadata.Id))
                .Select(item => item.Metadata.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return CreateProfileRow(
            ProfileManagementRowKind.DeviceProfile,
            profile.Metadata,
            "Gerät",
            $"{profile.Manufacturer} / {profile.DeviceType}",
            usedBy,
            Details: string.Join(Environment.NewLine, new[]
            {
                $"Geräteprofil: {profile.Metadata.Name}",
                $"Hersteller: {profile.Manufacturer}",
                $"Modell: {profile.Model}",
                $"Gerätetyp: {profile.DeviceType}",
                $"Verbindung: {profile.ConnectionKind}",
                $"Bidirektional: {DisplayBool(profile.IsBidirectional)}",
                $"Parser: {profile.ParserMode}",
                $"Messwertdefinitionen: {profile.Measurements.Count}"
            }));
    }

    private static ProfileManagementRow CreateExportRow(ProfileCatalog catalog, ExportProfileDefinition profile)
    {
        var usedBy = catalog.InterfaceProfiles
            .Where(item => IdEquals(item.ExportProfileId, profile.Metadata.Id))
            .Select(item => item.Metadata.Name)
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        var ais = catalog.AisProfiles.FirstOrDefault(item => IdEquals(item.Metadata.Id, profile.TargetAisProfileId));
        var device = catalog.DeviceProfiles.FirstOrDefault(item => IdEquals(item.Metadata.Id, profile.SourceDeviceProfileId));

        return CreateProfileRow(
            ProfileManagementRowKind.ExportProfile,
            profile.Metadata,
            "Export",
            ais?.Metadata.Name ?? "-",
            usedBy,
            Details: string.Join(Environment.NewLine, new[]
            {
                $"Exportprofil: {profile.Metadata.Name}",
                $"Ziel-AIS: {ais?.Metadata.Name ?? profile.TargetAisProfileId}",
                $"Quellgerät: {device?.Metadata.Name ?? profile.SourceDeviceProfileId}",
                $"Ausgabe-Encoding: {profile.OutputEncoding}",
                $"Exportregeln: {profile.Rules.Count}"
            }));
    }

    private static ProfileManagementRow CreateInterfaceRow(ProfileCatalog catalog, InterfaceProfileDefinition profile)
    {
        var ais = catalog.AisProfiles.FirstOrDefault(item => IdEquals(item.Metadata.Id, profile.AisProfileId));
        var device = catalog.DeviceProfiles.FirstOrDefault(item => IdEquals(item.Metadata.Id, profile.DeviceProfileId));
        var export = catalog.ExportProfiles.FirstOrDefault(item => IdEquals(item.Metadata.Id, profile.ExportProfileId));
        var actions = profile.Metadata.IsBuiltIn
            ? "Details, duplizieren, im Schnittstellenprofil öffnen"
            : "Details, umbenennen, duplizieren, löschen, im Schnittstellenprofil öffnen";

        return new ProfileManagementRow(
            ProfileManagementRowKind.InterfaceProfile,
            profile.Metadata.Id,
            profile.Metadata.Name,
            "Schnittstelle",
            device?.Metadata.Name ?? "-",
            Scope: GetScope(profile.Metadata),
            ActiveUsage: profile.IsActive ? "Aktiv" : "Inaktiv",
            UsedBy: string.Join(", ", new[] { ais?.Metadata.Name, device?.Metadata.Name, export?.Metadata.Name }.Where(value => !string.IsNullOrWhiteSpace(value))),
            UpdatedAt: FormatDate(profile.Metadata.UpdatedAt),
            Actions: actions,
            Details: string.Join(Environment.NewLine, new[]
            {
                $"Schnittstellenprofil: {profile.Metadata.Name}",
                $"Aktiv: {DisplayBool(profile.IsActive)}",
                $"Lizenzpflichtig: {DisplayBool(profile.IsLicenseRequired)}",
                $"AIS: {ais?.Metadata.Name ?? profile.AisProfileId}",
                $"Gerät: {device?.Metadata.Name ?? profile.DeviceProfileId}",
                $"Export: {export?.Metadata.Name ?? profile.ExportProfileId}",
                $"COM-Port: {profile.SerialSettings?.PortName ?? "-"}"
            }),
            FilePath: null,
            profile.Metadata.IsBuiltIn,
            profile.Metadata.IsUserDefined,
            CanRename: profile.Metadata.IsUserDefined && !profile.Metadata.IsBuiltIn,
            CanDuplicate: true,
            CanDelete: profile.Metadata.IsUserDefined && !profile.Metadata.IsBuiltIn && !profile.IsActive,
            CanOpenInWorkbench: false,
            CanOpenInInterfaceProfiles: true);
    }

    private static ProfileManagementRow CreateProfileRow(
        ProfileManagementRowKind kind,
        ProfileMetadata metadata,
        string type,
        string owner,
        IReadOnlyList<string> usedBy,
        string Details)
    {
        var canModify = metadata.IsUserDefined && !metadata.IsBuiltIn;
        var actions = metadata.IsBuiltIn
            ? "Details, duplizieren, im Baukasten öffnen"
            : "Details, umbenennen, duplizieren, löschen, im Baukasten öffnen";

        return new ProfileManagementRow(
            kind,
            metadata.Id,
            metadata.Name,
            type,
            owner,
            Scope: GetScope(metadata),
            ActiveUsage: usedBy.Count > 0 ? "Genutzt" : "-",
            UsedBy: usedBy.Count == 0 ? "-" : string.Join(", ", usedBy),
            UpdatedAt: FormatDate(metadata.UpdatedAt),
            Actions: actions,
            Details: Details,
            FilePath: null,
            metadata.IsBuiltIn,
            metadata.IsUserDefined,
            CanRename: canModify,
            CanDuplicate: true,
            CanDelete: canModify && usedBy.Count == 0,
            CanOpenInWorkbench: true,
            CanOpenInInterfaceProfiles: false);
    }

    private static IEnumerable<ProfileManagementRow> CreateTemplatePackageRows(AppDataPaths paths)
    {
        if (!Directory.Exists(paths.TemplatePackagesFolder))
        {
            yield break;
        }

        foreach (var filePath in Directory
                     .EnumerateFiles(paths.TemplatePackagesFolder)
                     .Where(filePath => !filePath.EndsWith(".xdtbaukasten.template.json", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(filePath => Path.GetFileName(filePath), StringComparer.CurrentCultureIgnoreCase))
        {
            var info = new FileInfo(filePath);
            yield return new ProfileManagementRow(
                ProfileManagementRowKind.TemplatePackage,
                filePath,
                Path.GetFileNameWithoutExtension(filePath),
                "Templatepaket",
                "Lokal",
                "Lokal",
                "-",
                "-",
                info.LastWriteTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                "Details, löschen, im Baukasten importieren",
                $"Templatepaket-Datei:{Environment.NewLine}{filePath}{Environment.NewLine}{Environment.NewLine}Import und Konfliktprüfung laufen über den Baukasten-Importdialog.",
                filePath,
                IsBuiltIn: false,
                IsUserDefined: true,
                CanRename: false,
                CanDuplicate: false,
                CanDelete: true,
                CanOpenInWorkbench: true,
                CanOpenInInterfaceProfiles: false);
        }
    }

    private static IEnumerable<ProfileManagementRow> CreateXdtBaukastenTemplateRows(
        XdtBaukastenTemplateLibraryService templateLibraryService,
        AppDataPaths paths)
    {
        foreach (var filePath in templateLibraryService.ListTemplateFiles(paths))
        {
            var info = new FileInfo(filePath);
            yield return new ProfileManagementRow(
                ProfileManagementRowKind.XdtBaukastenTemplate,
                filePath,
                Path.GetFileNameWithoutExtension(filePath).Replace(".xdtbaukasten.template", string.Empty, StringComparison.OrdinalIgnoreCase),
                "Baukasten-Template",
                "Lokal",
                "Lokal",
                "-",
                "-",
                info.LastWriteTime.ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                "Details, löschen, im Baukasten laden",
                $"Lokales Baukasten-Template:{Environment.NewLine}{filePath}{Environment.NewLine}{Environment.NewLine}Diese Datei enthält eine Baukasten-Arbeitskopie, nicht die Originalprofile.",
                filePath,
                IsBuiltIn: false,
                IsUserDefined: true,
                CanRename: false,
                CanDuplicate: false,
                CanDelete: true,
                CanOpenInWorkbench: true,
                CanOpenInInterfaceProfiles: false);
        }
    }

    private static ProfileManagementRow CreateMaintenanceRow(ProfileCatalog catalog, AppDataPaths paths)
    {
        return new ProfileManagementRow(
            ProfileManagementRowKind.Maintenance,
            "profile-maintenance",
            "BuiltIn-Profile reparieren / Katalog neu laden",
            "Wartung",
            "XDTBox",
            "Wartung",
            "-",
            "-",
            "-",
            "Profile neu laden, BuiltIn reparieren",
            string.Join(Environment.NewLine, new[]
            {
                "Wartung / Reparatur",
                $"Profilordner: {paths.ProfilesFolder}",
                $"AIS-Profile: {catalog.AisProfiles.Count}",
                $"Geräteprofile: {catalog.DeviceProfiles.Count}",
                $"Exportprofile: {catalog.ExportProfiles.Count}",
                $"Schnittstellenprofile: {catalog.InterfaceProfiles.Count}",
                "BuiltIn-Reparatur lässt UserDefined-Profile unverändert."
            }),
            FilePath: null,
            IsBuiltIn: false,
            IsUserDefined: false,
            CanRename: false,
            CanDuplicate: false,
            CanDelete: false,
            CanOpenInWorkbench: false,
            CanOpenInInterfaceProfiles: false);
    }

    private ProfileManagementActionResult DuplicateAisProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        DateTimeOffset timestamp,
        string? createdBy)
    {
        var source = catalog.AisProfiles.FirstOrDefault(profile => IdEquals(profile.Metadata.Id, profileId));
        if (source is null)
        {
            return new ProfileManagementActionResult(false, "AIS-Profil nicht gefunden.");
        }

        var name = UserDefinedProfileCreationService.CreateAvailableProfileName(
            catalog.AisProfiles.Select(profile => profile.Metadata.Name),
            $"{source.Metadata.Name} - Kopie");
        var metadata = CreateCopyMetadata(source.Metadata, ProfileKind.AisProfile, "ais", name, catalog.AisProfiles.Select(profile => profile.Metadata), timestamp, createdBy);
        var copy = source with { Metadata = metadata, Name = name };
        _profileCatalogService.SaveNewAisProfile(paths, copy);
        return new ProfileManagementActionResult(true, $"AIS-Profil dupliziert: {name}.", AisProfileId: copy.Metadata.Id);
    }

    private ProfileManagementActionResult DuplicateDeviceProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        DateTimeOffset timestamp,
        string? createdBy)
    {
        var source = catalog.DeviceProfiles.FirstOrDefault(profile => IdEquals(profile.Metadata.Id, profileId));
        if (source is null)
        {
            return new ProfileManagementActionResult(false, "Geräteprofil nicht gefunden.");
        }

        var name = UserDefinedProfileCreationService.CreateAvailableProfileName(
            catalog.DeviceProfiles.Select(profile => profile.Metadata.Name),
            $"{source.Metadata.Name} - Kopie");
        var metadata = CreateCopyMetadata(source.Metadata, ProfileKind.DeviceProfile, "device", name, catalog.DeviceProfiles.Select(profile => profile.Metadata), timestamp, createdBy);
        var copy = source with { Metadata = metadata };
        _profileCatalogService.SaveNewDeviceProfileDefinition(paths, copy);
        return new ProfileManagementActionResult(true, $"Geräteprofil dupliziert: {name}.", DeviceProfileId: copy.Metadata.Id);
    }

    private ProfileManagementActionResult DuplicateExportProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        DateTimeOffset timestamp,
        string? createdBy)
    {
        var source = catalog.ExportProfiles.FirstOrDefault(profile => IdEquals(profile.Metadata.Id, profileId));
        if (source is null)
        {
            return new ProfileManagementActionResult(false, "Exportprofil nicht gefunden.");
        }

        var name = UserDefinedProfileCreationService.CreateAvailableProfileName(
            catalog.ExportProfiles.Select(profile => profile.Metadata.Name),
            $"{source.Metadata.Name} - Kopie");
        var metadata = CreateCopyMetadata(source.Metadata, ProfileKind.ExportProfile, "export", name, catalog.ExportProfiles.Select(profile => profile.Metadata), timestamp, createdBy);
        var copy = source with { Metadata = metadata };
        _profileCatalogService.SaveNewExportProfile(paths, copy);
        return new ProfileManagementActionResult(true, $"Exportprofil dupliziert: {name}.", ExportProfileId: copy.Metadata.Id);
    }

    private ProfileManagementActionResult DuplicateInterfaceProfile(
        ProfileCatalog catalog,
        AppDataPaths paths,
        string profileId,
        DateTimeOffset timestamp,
        string? createdBy)
    {
        var source = catalog.InterfaceProfiles.FirstOrDefault(profile => IdEquals(profile.Metadata.Id, profileId));
        if (source is null)
        {
            return new ProfileManagementActionResult(false, "Schnittstellenprofil nicht gefunden.");
        }

        var name = UserDefinedProfileCreationService.CreateAvailableProfileName(
            catalog.InterfaceProfiles.Select(profile => profile.Metadata.Name),
            $"{source.Metadata.Name} - Kopie");
        var metadata = CreateCopyMetadata(source.Metadata, ProfileKind.InterfaceProfile, "interface", name, catalog.InterfaceProfiles.Select(profile => profile.Metadata), timestamp, createdBy);
        var copy = source with { Metadata = metadata, IsActive = false };
        _profileCatalogService.SaveNewInterfaceProfileDefinition(paths, copy);
        return new ProfileManagementActionResult(true, $"Schnittstellenprofil dupliziert und inaktiv gespeichert: {name}.", InterfaceProfileId: copy.Metadata.Id);
    }

    private static ProfileMetadata CreateCopyMetadata(
        ProfileMetadata source,
        ProfileKind profileKind,
        string idPrefix,
        string name,
        IEnumerable<ProfileMetadata> existingMetadata,
        DateTimeOffset timestamp,
        string? createdBy)
    {
        return source with
        {
            Id = UserDefinedProfileCreationService.CreateUniqueProfileId(idPrefix, name, existingMetadata.Select(metadata => metadata.Id)),
            Name = name,
            ProfileKind = profileKind,
            Description = $"UserDefined-Kopie von {source.Name}.",
            CreatedAt = timestamp,
            UpdatedAt = timestamp,
            CreatedBy = createdBy,
            IsBuiltIn = false,
            IsUserDefined = true
        };
    }

    private static ProfileManagementActionResult EvaluateDeleteProfile(
        ProfileManagementRow row,
        string profileType,
        IReadOnlyList<string> references)
    {
        if (row.IsBuiltIn)
        {
            return new ProfileManagementActionResult(false, $"BuiltIn-{profileType}e können nicht gelöscht werden.");
        }

        if (!row.IsUserDefined)
        {
            return new ProfileManagementActionResult(false, $"Nur UserDefined-{profileType}e können gelöscht werden.");
        }

        if (references.Count > 0)
        {
            return new ProfileManagementActionResult(
                false,
                $"{profileType} wird noch verwendet und kann nicht blind gelöscht werden: {string.Join(", ", references)}.");
        }

        return new ProfileManagementActionResult(true, $"{profileType} kann gelöscht werden.");
    }

    private static bool DeleteLocalFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        File.Delete(filePath);
        return true;
    }

    private static string GetScope(ProfileMetadata metadata)
    {
        if (metadata.IsBuiltIn)
        {
            return "BuiltIn";
        }

        return metadata.IsUserDefined ? "UserDefined" : "Extern";
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return value == default
            ? "-"
            : value.ToLocalTime().ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture);
    }

    private static string DisplayBool(bool value)
    {
        return value ? "Ja" : "Nein";
    }

    private static bool IdEquals(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    }
}
