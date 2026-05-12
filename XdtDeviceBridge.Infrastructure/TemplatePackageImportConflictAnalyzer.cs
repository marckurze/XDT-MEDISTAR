using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportConflictAnalyzer
{
    private readonly FolderSafetyValidator _folderSafetyValidator;

    public TemplatePackageImportConflictAnalyzer()
        : this(new FolderSafetyValidator())
    {
    }

    public TemplatePackageImportConflictAnalyzer(FolderSafetyValidator folderSafetyValidator)
    {
        _folderSafetyValidator = folderSafetyValidator ?? throw new ArgumentNullException(nameof(folderSafetyValidator));
    }

    public TemplatePackageImportAnalysisResult Analyze(
        TemplatePackageImportResult importResult,
        ProfileCatalog existingCatalog)
    {
        ArgumentNullException.ThrowIfNull(importResult);
        ArgumentNullException.ThrowIfNull(existingCatalog);

        if (importResult.Package is null)
        {
            return CreateResult(
                success: false,
                packageId: null,
                packageName: null,
                decisions: Array.Empty<TemplatePackageImportProfileDecision>(),
                warnings: Array.Empty<string>(),
                errorMessage: "Templatepaket darf nicht leer sein.");
        }

        var localProfiles = ProfileIndex.Create(existingCatalog);
        var availableProfiles = AvailableProfileIds.Create(importResult, existingCatalog);
        var decisions = CreateImportedProfiles(importResult)
            .Select(profile => AnalyzeProfile(profile, localProfiles, availableProfiles))
            .ToList();

        var warnings = CreateWarnings(importResult).ToList();

        return CreateResult(
            success: true,
            packageId: importResult.Package.Metadata.Id,
            packageName: importResult.Package.Metadata.Name,
            decisions: decisions,
            warnings: warnings,
            errorMessage: null);
    }

    private TemplatePackageImportProfileDecision AnalyzeProfile(
        ImportedProfile profile,
        ProfileIndex localProfiles,
        AvailableProfileIds availableProfiles)
    {
        if (profile.Metadata.ProfileKind != profile.ProfileKind)
        {
            return CreateDecision(
                profile,
                existing: null,
                TemplatePackageImportConflictType.UnsupportedProfileKind,
                TemplatePackageImportAction.Blocked,
                isBlocking: true,
                $"Importierte Profilart {profile.Metadata.ProfileKind} passt nicht zur erwarteten Profilart {profile.ProfileKind}.");
        }

        var validationIssues = ValidateProfile(profile).ToList();
        if (validationIssues.Count > 0)
        {
            return CreateDecision(
                profile,
                existing: null,
                TemplatePackageImportConflictType.InvalidProfile,
                TemplatePackageImportAction.Blocked,
                isBlocking: true,
                $"Importiertes Profil ist ungültig: {string.Join("; ", validationIssues)}");
        }

        var dependencyIssue = FindMissingDependency(profile, availableProfiles);
        if (dependencyIssue is not null)
        {
            return CreateDecision(
                profile,
                existing: null,
                TemplatePackageImportConflictType.MissingDependency,
                TemplatePackageImportAction.Blocked,
                isBlocking: true,
                dependencyIssue);
        }

        if (profile.Profile is InterfaceProfileDefinition interfaceProfile)
        {
            var unsafeFolderIssue = FindUnsafeFolderIssue(interfaceProfile);
            if (unsafeFolderIssue is not null)
            {
                return CreateDecision(
                    profile,
                    existing: null,
                    TemplatePackageImportConflictType.UnsafeFolderPath,
                    TemplatePackageImportAction.Blocked,
                    isBlocking: true,
                    $"Importiertes Schnittstellenprofil enthält unsicheren Ordnerpfad '{unsafeFolderIssue.Path}': {unsafeFolderIssue.Message}");
            }
        }

        var existingById = localProfiles.FindById(profile.ProfileKind, profile.Metadata.Id);
        var existingByName = localProfiles.FindByName(profile.ProfileKind, profile.Metadata.Name);
        var existing = existingById ?? existingByName;

        if (existing is not null && existing.IsBuiltIn)
        {
            return CreateDecision(
                profile,
                existing,
                TemplatePackageImportConflictType.BuiltInProtected,
                TemplatePackageImportAction.ImportAsCopy,
                isBlocking: false,
                $"Bestehendes BuiltIn-Profil '{existing.Name}' ist geschützt. Das importierte Profil kann nur bewusst als Kopie übernommen werden.");
        }

        if (existingById is not null)
        {
            var message = string.Equals(existingById.Version, profile.Metadata.Version, StringComparison.OrdinalIgnoreCase)
                ? $"Ein {FormatProfileKind(profile.ProfileKind)} mit derselben ID existiert bereits."
                : $"Ein {FormatProfileKind(profile.ProfileKind)} mit derselben ID existiert bereits mit Version '{existingById.Version}'. Importierte Version ist '{profile.Metadata.Version}'.";

            return CreateDecision(
                profile,
                existingById,
                TemplatePackageImportConflictType.SameIdExists,
                TemplatePackageImportAction.ImportAsCopy,
                isBlocking: false,
                message);
        }

        if (existingByName is not null)
        {
            return CreateDecision(
                profile,
                existingByName,
                TemplatePackageImportConflictType.SameNameExists,
                TemplatePackageImportAction.ImportAsCopy,
                isBlocking: false,
                $"Ein {FormatProfileKind(profile.ProfileKind)} mit demselben Namen existiert bereits.");
        }

        return CreateDecision(
            profile,
            existing: null,
            TemplatePackageImportConflictType.None,
            TemplatePackageImportAction.ImportAsNew,
            isBlocking: false,
            "Importiertes Profil kann neu importiert werden.");
    }

    private FolderSafetyValidationIssue? FindUnsafeFolderIssue(InterfaceProfileDefinition interfaceProfile)
    {
        var options = interfaceProfile.FolderOptions;
        var paths = new[]
        {
            options.AisImportFolder,
            options.DeviceImportFolder,
            options.ExportFolder,
            options.ArchiveFolder,
            options.ErrorFolder,
            options.AttachmentImportFolder,
            options.AttachmentExportFolder
        };

        foreach (var path in paths.Where(path => !string.IsNullOrWhiteSpace(path)))
        {
            var issue = _folderSafetyValidator
                .ValidateFolderForCleanup(path)
                .Issues
                .FirstOrDefault(issue => issue.Severity == FolderSafetyValidationIssueSeverity.Error);

            if (issue is not null)
            {
                return issue;
            }
        }

        return null;
    }

    private static string? FindMissingDependency(
        ImportedProfile profile,
        AvailableProfileIds availableProfiles)
    {
        return profile.Profile switch
        {
            ExportProfileDefinition exportProfile when !availableProfiles.AisProfileIds.Contains(exportProfile.TargetAisProfileId) =>
                $"Exportprofil verweist auf fehlendes AIS-Profil: {exportProfile.TargetAisProfileId}",
            ExportProfileDefinition exportProfile when !availableProfiles.DeviceProfileIds.Contains(exportProfile.SourceDeviceProfileId) =>
                $"Exportprofil verweist auf fehlendes Geräteprofil: {exportProfile.SourceDeviceProfileId}",
            InterfaceProfileDefinition interfaceProfile when !availableProfiles.AisProfileIds.Contains(interfaceProfile.AisProfileId) =>
                $"Schnittstellenprofil verweist auf fehlendes AIS-Profil: {interfaceProfile.AisProfileId}",
            InterfaceProfileDefinition interfaceProfile when !availableProfiles.DeviceProfileIds.Contains(interfaceProfile.DeviceProfileId) =>
                $"Schnittstellenprofil verweist auf fehlendes Geräteprofil: {interfaceProfile.DeviceProfileId}",
            InterfaceProfileDefinition interfaceProfile when !availableProfiles.ExportProfileIds.Contains(interfaceProfile.ExportProfileId) =>
                $"Schnittstellenprofil verweist auf fehlendes Exportprofil: {interfaceProfile.ExportProfileId}",
            _ => null
        };
    }

    private static IEnumerable<string> ValidateProfile(ImportedProfile profile)
    {
        return profile.Profile switch
        {
            AisProfile aisProfile => aisProfile.Validate(),
            DeviceProfileDefinition deviceProfile => deviceProfile.Validate(),
            ExportProfileDefinition exportProfile => exportProfile.Validate(),
            InterfaceProfileDefinition interfaceProfile => interfaceProfile.Validate(),
            _ => new[] { $"Nicht unterstützter Profiltyp: {profile.Profile.GetType().Name}" }
        };
    }

    private static IEnumerable<string> CreateWarnings(TemplatePackageImportResult importResult)
    {
        foreach (var interfaceProfile in importResult.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>())
        {
            yield return $"Importiertes Schnittstellenprofil '{interfaceProfile.Metadata.Name}' muss vor späterer Nutzung geprüft werden.";

            if (interfaceProfile.IsActive)
            {
                yield return $"Importiertes Schnittstellenprofil '{interfaceProfile.Metadata.Name}' ist im Paket als aktiv markiert und wird beim Import nicht automatisch aktiviert.";
            }

            if (!string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentImportFolder)
                || !string.IsNullOrWhiteSpace(interfaceProfile.FolderOptions.AttachmentExportFolder))
            {
                yield return $"Importiertes Schnittstellenprofil '{interfaceProfile.Metadata.Name}' enthält XDT-Anhang-Ordner; Ordnerpfade müssen vor späterer Nutzung geprüft werden.";
            }
        }
    }

    private static string FormatProfileKind(ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.AisProfile => "AIS-Profil",
            ProfileKind.DeviceProfile => "Geräteprofil",
            ProfileKind.ExportProfile => "Exportprofil",
            ProfileKind.InterfaceProfile => "Schnittstellenprofil",
            _ => profileKind.ToString()
        };
    }

    private static IReadOnlyList<ImportedProfile> CreateImportedProfiles(TemplatePackageImportResult importResult)
    {
        var profiles = new List<ImportedProfile>();

        profiles.AddRange((importResult.AisProfiles ?? Array.Empty<AisProfile>())
            .Select(profile => new ImportedProfile(ProfileKind.AisProfile, profile.Metadata, profile)));
        profiles.AddRange((importResult.DeviceProfiles ?? Array.Empty<DeviceProfileDefinition>())
            .Select(profile => new ImportedProfile(ProfileKind.DeviceProfile, profile.Metadata, profile)));
        profiles.AddRange((importResult.ExportProfiles ?? Array.Empty<ExportProfileDefinition>())
            .Select(profile => new ImportedProfile(ProfileKind.ExportProfile, profile.Metadata, profile)));
        profiles.AddRange((importResult.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>())
            .Select(profile => new ImportedProfile(ProfileKind.InterfaceProfile, profile.Metadata, profile)));

        return profiles;
    }

    private static TemplatePackageImportProfileDecision CreateDecision(
        ImportedProfile profile,
        ProfileMetadata? existing,
        TemplatePackageImportConflictType conflictType,
        TemplatePackageImportAction suggestedAction,
        bool isBlocking,
        string message)
    {
        return new TemplatePackageImportProfileDecision(
            ProfileKind: profile.ProfileKind,
            ImportedProfileId: profile.Metadata.Id,
            ImportedProfileName: profile.Metadata.Name,
            ImportedProfileVersion: profile.Metadata.Version,
            ExistingProfileId: existing?.Id,
            ExistingProfileName: existing?.Name,
            ExistingProfileSource: GetExistingProfileSource(existing),
            ExistingProfileVersion: existing?.Version,
            ConflictType: conflictType,
            SuggestedAction: suggestedAction,
            IsBlocking: isBlocking,
            Message: message);
    }

    private static TemplatePackageImportExistingProfileSource GetExistingProfileSource(ProfileMetadata? existing)
    {
        if (existing is null)
        {
            return TemplatePackageImportExistingProfileSource.None;
        }

        return existing.IsBuiltIn
            ? TemplatePackageImportExistingProfileSource.BuiltIn
            : TemplatePackageImportExistingProfileSource.UserDefined;
    }

    private static TemplatePackageImportAnalysisResult CreateResult(
        bool success,
        string? packageId,
        string? packageName,
        IReadOnlyList<TemplatePackageImportProfileDecision> decisions,
        IReadOnlyList<string> warnings,
        string? errorMessage)
    {
        var blockingConflicts = decisions.Where(decision => decision.IsBlocking).ToList();

        return new TemplatePackageImportAnalysisResult(
            Success: success,
            PackageId: packageId,
            PackageName: packageName,
            ProfileDecisions: decisions,
            BlockingConflicts: blockingConflicts,
            Warnings: warnings,
            ErrorMessage: errorMessage,
            TotalProfiles: decisions.Count,
            ImportableProfiles: decisions.Count(decision => decision.SuggestedAction != TemplatePackageImportAction.Blocked),
            ConflictingProfiles: decisions.Count(decision => decision.ConflictType != TemplatePackageImportConflictType.None),
            BlockedProfiles: blockingConflicts.Count);
    }

    private sealed record ImportedProfile(
        ProfileKind ProfileKind,
        ProfileMetadata Metadata,
        object Profile);

    private sealed class ProfileIndex
    {
        private readonly Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>> _byId;
        private readonly Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>> _byName;

        private ProfileIndex(
            Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>> byId,
            Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>> byName)
        {
            _byId = byId;
            _byName = byName;
        }

        public static ProfileIndex Create(ProfileCatalog catalog)
        {
            var byId = new Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>>();
            var byName = new Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>>();

            AddProfiles(byId, byName, ProfileKind.AisProfile, catalog.AisProfiles.Select(profile => profile.Metadata));
            AddProfiles(byId, byName, ProfileKind.DeviceProfile, catalog.DeviceProfiles.Select(profile => profile.Metadata));
            AddProfiles(byId, byName, ProfileKind.ExportProfile, catalog.ExportProfiles.Select(profile => profile.Metadata));
            AddProfiles(byId, byName, ProfileKind.InterfaceProfile, catalog.InterfaceProfiles.Select(profile => profile.Metadata));

            return new ProfileIndex(byId, byName);
        }

        public ProfileMetadata? FindById(ProfileKind profileKind, string id)
        {
            return _byId.TryGetValue(profileKind, out var profiles)
                && profiles.TryGetValue(id, out var metadata)
                    ? metadata
                    : null;
        }

        public ProfileMetadata? FindByName(ProfileKind profileKind, string name)
        {
            return _byName.TryGetValue(profileKind, out var profiles)
                && profiles.TryGetValue(name, out var metadata)
                    ? metadata
                    : null;
        }

        private static void AddProfiles(
            Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>> byId,
            Dictionary<ProfileKind, Dictionary<string, ProfileMetadata>> byName,
            ProfileKind profileKind,
            IEnumerable<ProfileMetadata> profiles)
        {
            byId[profileKind] = profiles
                .Where(profile => !string.IsNullOrWhiteSpace(profile.Id))
                .GroupBy(profile => profile.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            byName[profileKind] = profiles
                .Where(profile => !string.IsNullOrWhiteSpace(profile.Name))
                .GroupBy(profile => profile.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }
    }

    private sealed record AvailableProfileIds(
        HashSet<string> AisProfileIds,
        HashSet<string> DeviceProfileIds,
        HashSet<string> ExportProfileIds)
    {
        public static AvailableProfileIds Create(
            TemplatePackageImportResult importResult,
            ProfileCatalog catalog)
        {
            return new AvailableProfileIds(
                AisProfileIds: CreateSet(
                    catalog.AisProfiles.Select(profile => profile.Metadata.Id),
                    (importResult.AisProfiles ?? Array.Empty<AisProfile>()).Select(profile => profile.Metadata.Id)),
                DeviceProfileIds: CreateSet(
                    catalog.DeviceProfiles.Select(profile => profile.Metadata.Id),
                    (importResult.DeviceProfiles ?? Array.Empty<DeviceProfileDefinition>()).Select(profile => profile.Metadata.Id)),
                ExportProfileIds: CreateSet(
                    catalog.ExportProfiles.Select(profile => profile.Metadata.Id),
                    (importResult.ExportProfiles ?? Array.Empty<ExportProfileDefinition>()).Select(profile => profile.Metadata.Id)));
        }

        private static HashSet<string> CreateSet(
            IEnumerable<string> localIds,
            IEnumerable<string> importedIds)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var id in localIds.Concat(importedIds).Where(id => !string.IsNullOrWhiteSpace(id)))
            {
                result.Add(id);
            }

            return result;
        }
    }
}
