using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportExecutor
{
    private readonly ProfileCatalogService _profileCatalogService;

    public TemplatePackageImportExecutor()
        : this(new ProfileCatalogService())
    {
    }

    public TemplatePackageImportExecutor(ProfileCatalogService profileCatalogService)
    {
        _profileCatalogService = profileCatalogService ?? throw new ArgumentNullException(nameof(profileCatalogService));
    }

    public TemplatePackageImportExecutionResult Execute(
        TemplatePackageImportResult importResult,
        TemplatePackageImportPlan plan,
        TemplatePackageImportDryRunResult dryRunResult,
        AppDataPaths paths)
    {
        return Execute(importResult, plan, dryRunResult, paths, DateTimeOffset.Now, Environment.UserName);
    }

    public TemplatePackageImportExecutionResult Execute(
        TemplatePackageImportResult importResult,
        TemplatePackageImportPlan plan,
        TemplatePackageImportDryRunResult dryRunResult,
        AppDataPaths paths,
        DateTimeOffset importedAt,
        string? importedBy)
    {
        ArgumentNullException.ThrowIfNull(importResult);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(dryRunResult);
        ArgumentNullException.ThrowIfNull(paths);

        var warnings = new List<string>(dryRunResult.Warnings ?? Array.Empty<string>());
        var items = new List<TemplatePackageImportExecutionItem>();
        var importedIndex = ImportedProfileIndex.Create(importResult);
        var planIndex = (plan.ProfilePlans ?? Array.Empty<TemplatePackageImportProfilePlan>())
            .ToDictionary(
                profilePlan => CreateProfileKey(profilePlan.ProfileKind, profilePlan.ImportedProfileId),
                StringComparer.OrdinalIgnoreCase);

        foreach (var dryRunItem in (dryRunResult.Items ?? Array.Empty<TemplatePackageImportDryRunItem>()).OrderBy(GetExecutionOrder))
        {
            var item = ExecuteItem(dryRunItem, planIndex, importedIndex, paths, importedAt, importedBy, warnings);
            items.Add(item);
        }

        var imported = items.Where(item => item.WasWritten).ToList();
        var skipped = items.Where(item => item.WasSkipped).ToList();
        var blocked = items.Where(item => item.WasBlocked).ToList();
        var failed = items.Count(item => !item.Success && !item.WasSkipped && !item.WasBlocked);

        return new TemplatePackageImportExecutionResult(
            Success: failed == 0,
            ImportedProfiles: imported,
            SkippedProfiles: skipped,
            BlockedProfiles: blocked,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ErrorMessage: failed == 0 ? null : "At least one profile could not be imported.",
            ImportedAsNew: imported.Count(item => item.Action == TemplatePackageImportAction.ImportAsNew),
            ImportedAsCopy: imported.Count(item => item.Action == TemplatePackageImportAction.ImportAsCopy),
            Skipped: skipped.Count,
            Blocked: blocked.Count,
            Failed: failed);
    }

    private TemplatePackageImportExecutionItem ExecuteItem(
        TemplatePackageImportDryRunItem dryRunItem,
        IReadOnlyDictionary<string, TemplatePackageImportProfilePlan> planIndex,
        ImportedProfileIndex importedIndex,
        AppDataPaths paths,
        DateTimeOffset importedAt,
        string? importedBy,
        List<string> warnings)
    {
        if (!planIndex.TryGetValue(CreateProfileKey(dryRunItem.ProfileKind, dryRunItem.ImportedProfileId), out var profilePlan))
        {
            return CreateBlockedItem(dryRunItem, "Import plan entry is missing.");
        }

        if (dryRunItem.IsBlocking || profilePlan.IsBlocking || profilePlan.PlannedAction == TemplatePackageImportAction.Blocked)
        {
            return CreateBlockedItem(dryRunItem, profilePlan.Message);
        }

        if (profilePlan.PlannedAction == TemplatePackageImportAction.ReplaceExisting)
        {
            return CreateSkippedItem(dryRunItem, "ReplaceExisting wird in diesem Schritt noch nicht unterstützt.");
        }

        if (profilePlan.PlannedAction is TemplatePackageImportAction.KeepExisting or TemplatePackageImportAction.Skip)
        {
            return CreateSkippedItem(dryRunItem, profilePlan.PlannedAction == TemplatePackageImportAction.KeepExisting
                ? "Bestehendes Profil bleibt erhalten."
                : "Profil wurde gemäß Importplan übersprungen.");
        }

        if (profilePlan.PlannedAction is not TemplatePackageImportAction.ImportAsNew
            and not TemplatePackageImportAction.ImportAsCopy)
        {
            return CreateBlockedItem(dryRunItem, $"Unsupported import action: {profilePlan.PlannedAction}.");
        }

        if (!dryRunItem.WouldWrite)
        {
            return CreateBlockedItem(dryRunItem, "Dry-run did not allow writing this profile.");
        }

        if (string.IsNullOrWhiteSpace(dryRunItem.TargetProfileId)
            || string.IsNullOrWhiteSpace(dryRunItem.TargetProfileName))
        {
            return CreateBlockedItem(dryRunItem, "Target profile id or name is missing.");
        }

        if (!IsSafeProfileId(dryRunItem.TargetProfileId))
        {
            return CreateBlockedItem(dryRunItem, "Target profile id is not a safe file name.");
        }

        try
        {
            var written = WriteProfile(
                dryRunItem,
                importedIndex,
                paths,
                importedAt,
                importedBy,
                warnings);

            return new TemplatePackageImportExecutionItem(
                ProfileKind: dryRunItem.ProfileKind,
                SourceProfileId: dryRunItem.ImportedProfileId,
                SourceProfileName: dryRunItem.ImportedProfileName,
                TargetProfileId: dryRunItem.TargetProfileId,
                TargetProfileName: dryRunItem.TargetProfileName,
                Action: dryRunItem.PlannedAction,
                Success: true,
                WasWritten: true,
                WasSkipped: false,
                WasBlocked: false,
                Message: written);
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            return new TemplatePackageImportExecutionItem(
                ProfileKind: dryRunItem.ProfileKind,
                SourceProfileId: dryRunItem.ImportedProfileId,
                SourceProfileName: dryRunItem.ImportedProfileName,
                TargetProfileId: dryRunItem.TargetProfileId,
                TargetProfileName: dryRunItem.TargetProfileName,
                Action: dryRunItem.PlannedAction,
                Success: false,
                WasWritten: false,
                WasSkipped: false,
                WasBlocked: false,
                Message: ex.Message);
        }
    }

    private string WriteProfile(
        TemplatePackageImportDryRunItem dryRunItem,
        ImportedProfileIndex importedIndex,
        AppDataPaths paths,
        DateTimeOffset importedAt,
        string? importedBy,
        List<string> warnings)
    {
        switch (dryRunItem.ProfileKind)
        {
            case ProfileKind.AisProfile:
                var aisProfile = importedIndex.FindAisProfile(dryRunItem.ImportedProfileId)
                    ?? throw new InvalidOperationException($"Imported AIS profile not found: {dryRunItem.ImportedProfileId}");
                _profileCatalogService.SaveNewAisProfile(paths, ToUserDefined(aisProfile, dryRunItem, importedAt, importedBy));
                return "AIS profile imported as UserDefined.";

            case ProfileKind.DeviceProfile:
                var deviceProfile = importedIndex.FindDeviceProfile(dryRunItem.ImportedProfileId)
                    ?? throw new InvalidOperationException($"Imported device profile not found: {dryRunItem.ImportedProfileId}");
                _profileCatalogService.SaveNewDeviceProfileDefinition(paths, ToUserDefined(deviceProfile, dryRunItem, importedAt, importedBy));
                return "Device profile imported as UserDefined.";

            case ProfileKind.ExportProfile:
                var exportProfile = importedIndex.FindExportProfile(dryRunItem.ImportedProfileId)
                    ?? throw new InvalidOperationException($"Imported export profile not found: {dryRunItem.ImportedProfileId}");
                _profileCatalogService.SaveNewExportProfile(paths, ToUserDefined(exportProfile, dryRunItem, importedAt, importedBy));
                return "Export profile imported as UserDefined.";

            case ProfileKind.InterfaceProfile:
                var interfaceProfile = importedIndex.FindInterfaceProfile(dryRunItem.ImportedProfileId)
                    ?? throw new InvalidOperationException($"Imported interface profile not found: {dryRunItem.ImportedProfileId}");
                var safeInterfaceProfile = ToUserDefined(interfaceProfile, dryRunItem, importedAt, importedBy);
                _profileCatalogService.SaveNewInterfaceProfileDefinition(paths, safeInterfaceProfile);
                warnings.Add($"Imported interface profile '{safeInterfaceProfile.Metadata.Name}' was deactivated. Folder paths and XDT attachment settings must be reviewed before activation.");
                return "Interface profile imported as inactive UserDefined profile.";

            default:
                throw new InvalidOperationException($"Unsupported profile kind: {dryRunItem.ProfileKind}");
        }
    }

    private static AisProfile ToUserDefined(
        AisProfile profile,
        TemplatePackageImportDryRunItem dryRunItem,
        DateTimeOffset importedAt,
        string? importedBy)
    {
        return profile with
        {
            Metadata = CreateImportedMetadata(profile.Metadata, dryRunItem, importedAt, importedBy),
            Name = dryRunItem.TargetProfileName ?? profile.Name
        };
    }

    private static DeviceProfileDefinition ToUserDefined(
        DeviceProfileDefinition profile,
        TemplatePackageImportDryRunItem dryRunItem,
        DateTimeOffset importedAt,
        string? importedBy)
    {
        return profile with
        {
            Metadata = CreateImportedMetadata(profile.Metadata, dryRunItem, importedAt, importedBy)
        };
    }

    private static ExportProfileDefinition ToUserDefined(
        ExportProfileDefinition profile,
        TemplatePackageImportDryRunItem dryRunItem,
        DateTimeOffset importedAt,
        string? importedBy)
    {
        return profile with
        {
            Metadata = CreateImportedMetadata(profile.Metadata, dryRunItem, importedAt, importedBy)
        };
    }

    private static InterfaceProfileDefinition ToUserDefined(
        InterfaceProfileDefinition profile,
        TemplatePackageImportDryRunItem dryRunItem,
        DateTimeOffset importedAt,
        string? importedBy)
    {
        var remappedAisProfileId = ResolveDependencyId(dryRunItem, TemplatePackageImportDependencyKind.AisProfile);
        var remappedDeviceProfileId = ResolveDependencyId(dryRunItem, TemplatePackageImportDependencyKind.DeviceProfile);
        var remappedExportProfileId = ResolveDependencyId(dryRunItem, TemplatePackageImportDependencyKind.ExportProfile);

        return profile with
        {
            Metadata = CreateImportedMetadata(profile.Metadata, dryRunItem, importedAt, importedBy),
            AisProfileId = remappedAisProfileId,
            DeviceProfileId = remappedDeviceProfileId,
            ExportProfileId = remappedExportProfileId,
            FolderOptions = profile.FolderOptions with
            {
                IsAttachmentProcessingEnabled = false
            },
            IsActive = false
        };
    }

    private static string ResolveDependencyId(
        TemplatePackageImportDryRunItem dryRunItem,
        TemplatePackageImportDependencyKind dependencyKind)
    {
        var remap = dryRunItem.DependencyRemaps.FirstOrDefault(remap => remap.DependencyKind == dependencyKind);
        if (remap is null || string.IsNullOrWhiteSpace(remap.TargetProfileId))
        {
            throw new InvalidOperationException($"Interface profile dependency cannot be resolved: {dependencyKind}.");
        }

        if (remap.Resolution is TemplatePackageImportDependencyResolution.Missing
            or TemplatePackageImportDependencyResolution.Blocked)
        {
            throw new InvalidOperationException($"Interface profile dependency is blocked: {dependencyKind}.");
        }

        return remap.TargetProfileId;
    }

    private static ProfileMetadata CreateImportedMetadata(
        ProfileMetadata sourceMetadata,
        TemplatePackageImportDryRunItem dryRunItem,
        DateTimeOffset importedAt,
        string? importedBy)
    {
        return sourceMetadata with
        {
            Id = dryRunItem.TargetProfileId ?? sourceMetadata.Id,
            Name = dryRunItem.TargetProfileName ?? sourceMetadata.Name,
            UpdatedAt = importedAt,
            CreatedBy = importedBy,
            IsBuiltIn = false,
            IsUserDefined = true
        };
    }

    private static TemplatePackageImportExecutionItem CreateSkippedItem(
        TemplatePackageImportDryRunItem dryRunItem,
        string message)
    {
        return new TemplatePackageImportExecutionItem(
            ProfileKind: dryRunItem.ProfileKind,
            SourceProfileId: dryRunItem.ImportedProfileId,
            SourceProfileName: dryRunItem.ImportedProfileName,
            TargetProfileId: dryRunItem.TargetProfileId,
            TargetProfileName: dryRunItem.TargetProfileName,
            Action: dryRunItem.PlannedAction,
            Success: true,
            WasWritten: false,
            WasSkipped: true,
            WasBlocked: false,
            Message: message);
    }

    private static TemplatePackageImportExecutionItem CreateBlockedItem(
        TemplatePackageImportDryRunItem dryRunItem,
        string message)
    {
        return new TemplatePackageImportExecutionItem(
            ProfileKind: dryRunItem.ProfileKind,
            SourceProfileId: dryRunItem.ImportedProfileId,
            SourceProfileName: dryRunItem.ImportedProfileName,
            TargetProfileId: dryRunItem.TargetProfileId,
            TargetProfileName: dryRunItem.TargetProfileName,
            Action: dryRunItem.PlannedAction,
            Success: true,
            WasWritten: false,
            WasSkipped: false,
            WasBlocked: true,
            Message: message);
    }

    private static int GetExecutionOrder(TemplatePackageImportDryRunItem dryRunItem)
    {
        return dryRunItem.ProfileKind switch
        {
            ProfileKind.AisProfile => 0,
            ProfileKind.DeviceProfile => 1,
            ProfileKind.ExportProfile => 2,
            ProfileKind.InterfaceProfile => 3,
            _ => 4
        };
    }

    private static string CreateProfileKey(ProfileKind profileKind, string profileId)
    {
        return $"{profileKind}:{profileId}";
    }

    private static bool IsSafeProfileId(string profileId)
    {
        var fileName = $"{profileId}.json";
        return fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0
            && string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal);
    }

    private sealed class ImportedProfileIndex
    {
        private readonly Dictionary<string, AisProfile> _aisProfiles;
        private readonly Dictionary<string, DeviceProfileDefinition> _deviceProfiles;
        private readonly Dictionary<string, ExportProfileDefinition> _exportProfiles;
        private readonly Dictionary<string, InterfaceProfileDefinition> _interfaceProfiles;

        private ImportedProfileIndex(
            Dictionary<string, AisProfile> aisProfiles,
            Dictionary<string, DeviceProfileDefinition> deviceProfiles,
            Dictionary<string, ExportProfileDefinition> exportProfiles,
            Dictionary<string, InterfaceProfileDefinition> interfaceProfiles)
        {
            _aisProfiles = aisProfiles;
            _deviceProfiles = deviceProfiles;
            _exportProfiles = exportProfiles;
            _interfaceProfiles = interfaceProfiles;
        }

        public static ImportedProfileIndex Create(TemplatePackageImportResult importResult)
        {
            return new ImportedProfileIndex(
                CreateIndex(importResult.AisProfiles, profile => profile.Metadata.Id),
                CreateIndex(importResult.DeviceProfiles, profile => profile.Metadata.Id),
                CreateIndex(importResult.ExportProfiles, profile => profile.Metadata.Id),
                CreateIndex(importResult.InterfaceProfiles, profile => profile.Metadata.Id));
        }

        public AisProfile? FindAisProfile(string id)
        {
            return _aisProfiles.TryGetValue(id, out var profile) ? profile : null;
        }

        public DeviceProfileDefinition? FindDeviceProfile(string id)
        {
            return _deviceProfiles.TryGetValue(id, out var profile) ? profile : null;
        }

        public ExportProfileDefinition? FindExportProfile(string id)
        {
            return _exportProfiles.TryGetValue(id, out var profile) ? profile : null;
        }

        public InterfaceProfileDefinition? FindInterfaceProfile(string id)
        {
            return _interfaceProfiles.TryGetValue(id, out var profile) ? profile : null;
        }

        private static Dictionary<string, TProfile> CreateIndex<TProfile>(
            IEnumerable<TProfile> profiles,
            Func<TProfile, string> getId)
        {
            return profiles
                .GroupBy(getId, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        }
    }
}
