using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportDryRunService
{
    public TemplatePackageImportDryRunResult Preview(
        TemplatePackageImportResult importResult,
        TemplatePackageImportPlan plan,
        ProfileCatalog existingCatalog)
    {
        ArgumentNullException.ThrowIfNull(importResult);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(existingCatalog);

        var profilePlans = (plan.ProfilePlans ?? Array.Empty<TemplatePackageImportProfilePlan>()).ToList();
        var planIndex = ProfilePlanIndex.Create(profilePlans);
        var importedProfiles = ImportedProfileIndex.Create(importResult);
        var localProfiles = LocalProfileIndex.Create(existingCatalog);
        var warnings = new List<string>(plan.Warnings ?? Array.Empty<string>());
        var items = new List<TemplatePackageImportDryRunItem>();

        foreach (var profilePlan in profilePlans)
        {
            var item = CreateItem(profilePlan, importedProfiles, planIndex, localProfiles, warnings);
            items.Add(item);
        }

        var blockingItems = items.Where(item => item.IsBlocking).ToList();

        return new TemplatePackageImportDryRunResult(
            Success: true,
            PackageId: plan.PackageId,
            PackageName: plan.PackageName,
            Items: items,
            BlockingItems: blockingItems,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            ErrorMessage: null,
            TotalItems: items.Count,
            WouldImportAsNew: items.Count(item => item.WouldWrite && item.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            WouldImportAsCopy: items.Count(item => item.WouldWrite && item.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            WouldReplaceExisting: items.Count(item => item.WouldReplace),
            WouldKeepExisting: items.Count(item => item.WouldKeepExisting),
            WouldSkip: items.Count(item => item.WouldSkip),
            WouldBlock: blockingItems.Count);
    }

    private static TemplatePackageImportDryRunItem CreateItem(
        TemplatePackageImportProfilePlan profilePlan,
        ImportedProfileIndex importedProfiles,
        ProfilePlanIndex planIndex,
        LocalProfileIndex localProfiles,
        List<string> globalWarnings)
    {
        var isIllegalBuiltInReplace = profilePlan.PlannedAction == TemplatePackageImportAction.ReplaceExisting
            && profilePlan.ExistingProfileSource == TemplatePackageImportExistingProfileSource.BuiltIn;
        var targetId = GetTargetProfileId(profilePlan, isIllegalBuiltInReplace);
        var targetName = GetTargetProfileName(profilePlan, isIllegalBuiltInReplace);
        var dependencyRemaps = CreateDependencyRemaps(profilePlan, importedProfiles, planIndex, localProfiles).ToList();
        var shouldValidateDependencies = profilePlan.PlannedAction is TemplatePackageImportAction.ImportAsNew
            or TemplatePackageImportAction.ImportAsCopy
            or TemplatePackageImportAction.ReplaceExisting;
        var hasBlockedDependency = shouldValidateDependencies && dependencyRemaps.Any(remap =>
            remap.Resolution is TemplatePackageImportDependencyResolution.Missing
                or TemplatePackageImportDependencyResolution.Blocked);
        var isBlocking = profilePlan.IsBlocking
            || profilePlan.PlannedAction == TemplatePackageImportAction.Blocked
            || isIllegalBuiltInReplace
            || hasBlockedDependency;
        var wouldWrite = !isBlocking
            && profilePlan.PlannedAction is TemplatePackageImportAction.ImportAsNew
                or TemplatePackageImportAction.ImportAsCopy
                or TemplatePackageImportAction.ReplaceExisting;
        var wouldReplace = wouldWrite && profilePlan.PlannedAction == TemplatePackageImportAction.ReplaceExisting;
        var wouldKeepExisting = profilePlan.PlannedAction == TemplatePackageImportAction.KeepExisting;
        var wouldSkip = profilePlan.PlannedAction is TemplatePackageImportAction.Skip
            or TemplatePackageImportAction.KeepExisting;
        var dependencyWarnings = CreateDependencyWarnings(profilePlan, dependencyRemaps).ToList();
        var interfaceWarnings = CreateInterfaceWarnings(profilePlan, importedProfiles).ToList();

        dependencyWarnings.AddRange(interfaceWarnings);
        globalWarnings.AddRange(interfaceWarnings);

        if (isIllegalBuiltInReplace)
        {
            dependencyWarnings.Add("BuiltIn profile replacement is not allowed.");
        }

        var requiresDependencyRemap = dependencyRemaps.Any(remap =>
            remap.Resolution is TemplatePackageImportDependencyResolution.ImportedAsCopy
                or TemplatePackageImportDependencyResolution.Missing
                or TemplatePackageImportDependencyResolution.Blocked
            || !string.Equals(remap.OriginalProfileId, remap.TargetProfileId, StringComparison.OrdinalIgnoreCase));

        return new TemplatePackageImportDryRunItem(
            ProfileKind: profilePlan.ProfileKind,
            ImportedProfileId: profilePlan.ImportedProfileId,
            ImportedProfileName: profilePlan.ImportedProfileName,
            PlannedAction: profilePlan.PlannedAction,
            TargetProfileId: targetId,
            TargetProfileName: targetName,
            ExistingProfileId: profilePlan.ExistingProfileId,
            ExistingProfileName: profilePlan.ExistingProfileName,
            IsBlocking: isBlocking,
            WouldWrite: wouldWrite,
            WouldReplace: wouldReplace,
            WouldKeepExisting: wouldKeepExisting,
            WouldSkip: wouldSkip,
            RequiresDependencyRemap: requiresDependencyRemap,
            DependencyRemaps: dependencyRemaps,
            DependencyRemapWarnings: dependencyWarnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            Message: CreateMessage(profilePlan, isIllegalBuiltInReplace, dependencyWarnings));
    }

    private static string? GetTargetProfileId(
        TemplatePackageImportProfilePlan profilePlan,
        bool isIllegalBuiltInReplace)
    {
        if (isIllegalBuiltInReplace)
        {
            return null;
        }

        return profilePlan.PlannedAction switch
        {
            TemplatePackageImportAction.ImportAsNew => profilePlan.ProposedProfileId ?? profilePlan.ImportedProfileId,
            TemplatePackageImportAction.ImportAsCopy => profilePlan.ProposedProfileId,
            TemplatePackageImportAction.ReplaceExisting => profilePlan.ExistingProfileId ?? profilePlan.ProposedProfileId,
            TemplatePackageImportAction.KeepExisting => profilePlan.ExistingProfileId,
            _ => null
        };
    }

    private static string? GetTargetProfileName(
        TemplatePackageImportProfilePlan profilePlan,
        bool isIllegalBuiltInReplace)
    {
        if (isIllegalBuiltInReplace)
        {
            return null;
        }

        return profilePlan.PlannedAction switch
        {
            TemplatePackageImportAction.ImportAsNew => profilePlan.ProposedProfileName ?? profilePlan.ImportedProfileName,
            TemplatePackageImportAction.ImportAsCopy => profilePlan.ProposedProfileName,
            TemplatePackageImportAction.ReplaceExisting => profilePlan.ExistingProfileName ?? profilePlan.ProposedProfileName,
            TemplatePackageImportAction.KeepExisting => profilePlan.ExistingProfileName,
            _ => null
        };
    }

    private static IEnumerable<TemplatePackageImportDependencyRemap> CreateDependencyRemaps(
        TemplatePackageImportProfilePlan profilePlan,
        ImportedProfileIndex importedProfiles,
        ProfilePlanIndex planIndex,
        LocalProfileIndex localProfiles)
    {
        if (profilePlan.ProfileKind != ProfileKind.InterfaceProfile
            || profilePlan.PlannedAction is TemplatePackageImportAction.Skip
                or TemplatePackageImportAction.KeepExisting
                or TemplatePackageImportAction.Blocked
            || importedProfiles.FindInterfaceProfile(profilePlan.ImportedProfileId) is not { } interfaceProfile)
        {
            return Array.Empty<TemplatePackageImportDependencyRemap>();
        }

        return new[]
        {
            CreateDependencyRemap(TemplatePackageImportDependencyKind.AisProfile, ProfileKind.AisProfile, interfaceProfile.AisProfileId, planIndex, localProfiles),
            CreateDependencyRemap(TemplatePackageImportDependencyKind.DeviceProfile, ProfileKind.DeviceProfile, interfaceProfile.DeviceProfileId, planIndex, localProfiles),
            CreateDependencyRemap(TemplatePackageImportDependencyKind.ExportProfile, ProfileKind.ExportProfile, interfaceProfile.ExportProfileId, planIndex, localProfiles)
        };
    }

    private static TemplatePackageImportDependencyRemap CreateDependencyRemap(
        TemplatePackageImportDependencyKind dependencyKind,
        ProfileKind profileKind,
        string dependencyProfileId,
        ProfilePlanIndex planIndex,
        LocalProfileIndex localProfiles)
    {
        if (planIndex.Find(profileKind, dependencyProfileId) is { } dependencyPlan)
        {
            return CreateImportedDependencyRemap(dependencyKind, dependencyPlan);
        }

        if (localProfiles.Find(profileKind, dependencyProfileId) is { } localProfile)
        {
            return new TemplatePackageImportDependencyRemap(
                DependencyKind: dependencyKind,
                OriginalProfileId: dependencyProfileId,
                OriginalProfileName: localProfile.Name,
                TargetProfileId: localProfile.Id,
                TargetProfileName: localProfile.Name,
                Resolution: TemplatePackageImportDependencyResolution.LocalExisting,
                Message: $"Dependency uses existing local {dependencyKind}: {localProfile.Name}.");
        }

        return new TemplatePackageImportDependencyRemap(
            DependencyKind: dependencyKind,
            OriginalProfileId: dependencyProfileId,
            OriginalProfileName: null,
            TargetProfileId: null,
            TargetProfileName: null,
            Resolution: TemplatePackageImportDependencyResolution.Missing,
            Message: $"Dependency is missing: {dependencyKind} {dependencyProfileId}.");
    }

    private static TemplatePackageImportDependencyRemap CreateImportedDependencyRemap(
        TemplatePackageImportDependencyKind dependencyKind,
        TemplatePackageImportProfilePlan dependencyPlan)
    {
        var targetId = GetTargetProfileId(dependencyPlan, isIllegalBuiltInReplace: false);
        var targetName = GetTargetProfileName(dependencyPlan, isIllegalBuiltInReplace: false);
        var resolution = dependencyPlan.PlannedAction switch
        {
            TemplatePackageImportAction.ImportAsNew => TemplatePackageImportDependencyResolution.ImportedAsNew,
            TemplatePackageImportAction.ImportAsCopy => TemplatePackageImportDependencyResolution.ImportedAsCopy,
            TemplatePackageImportAction.ReplaceExisting => TemplatePackageImportDependencyResolution.LocalExisting,
            TemplatePackageImportAction.KeepExisting => TemplatePackageImportDependencyResolution.LocalExisting,
            TemplatePackageImportAction.Blocked => TemplatePackageImportDependencyResolution.Blocked,
            _ => TemplatePackageImportDependencyResolution.Missing
        };

        if (dependencyPlan.PlannedAction == TemplatePackageImportAction.Blocked)
        {
            targetId = null;
            targetName = null;
        }

        return new TemplatePackageImportDependencyRemap(
            DependencyKind: dependencyKind,
            OriginalProfileId: dependencyPlan.ImportedProfileId,
            OriginalProfileName: dependencyPlan.ImportedProfileName,
            TargetProfileId: targetId,
            TargetProfileName: targetName,
            Resolution: resolution,
            Message: CreateDependencyMessage(dependencyKind, resolution, dependencyPlan));
    }

    private static string CreateDependencyMessage(
        TemplatePackageImportDependencyKind dependencyKind,
        TemplatePackageImportDependencyResolution resolution,
        TemplatePackageImportProfilePlan dependencyPlan)
    {
        return resolution switch
        {
            TemplatePackageImportDependencyResolution.ImportedAsNew => $"Dependency {dependencyKind} will use imported profile '{dependencyPlan.ImportedProfileName}'.",
            TemplatePackageImportDependencyResolution.ImportedAsCopy => $"Dependency {dependencyKind} must be remapped to copied profile '{dependencyPlan.ProposedProfileName}'.",
            TemplatePackageImportDependencyResolution.LocalExisting => $"Dependency {dependencyKind} will use existing target '{dependencyPlan.ExistingProfileName ?? dependencyPlan.ProposedProfileName}'.",
            TemplatePackageImportDependencyResolution.Blocked => $"Dependency {dependencyKind} is blocked and prevents safe use.",
            _ => $"Dependency {dependencyKind} cannot be resolved."
        };
    }

    private static IEnumerable<string> CreateDependencyWarnings(
        TemplatePackageImportProfilePlan profilePlan,
        IReadOnlyList<TemplatePackageImportDependencyRemap> dependencyRemaps)
    {
        if (profilePlan.ProfileKind != ProfileKind.InterfaceProfile)
        {
            return Array.Empty<string>();
        }

        var warnings = new List<string>();
        foreach (var remap in dependencyRemaps)
        {
            if (remap.Resolution == TemplatePackageImportDependencyResolution.ImportedAsCopy)
            {
                warnings.Add($"{remap.DependencyKind} dependency must be remapped from '{remap.OriginalProfileId}' to '{remap.TargetProfileId}'.");
            }

            if (remap.Resolution is TemplatePackageImportDependencyResolution.Missing
                or TemplatePackageImportDependencyResolution.Blocked)
            {
                warnings.Add(remap.Message);
            }
        }

        return warnings;
    }

    private static IEnumerable<string> CreateInterfaceWarnings(
        TemplatePackageImportProfilePlan profilePlan,
        ImportedProfileIndex importedProfiles)
    {
        if (profilePlan.ProfileKind != ProfileKind.InterfaceProfile
            || importedProfiles.FindInterfaceProfile(profilePlan.ImportedProfileId) is not { } interfaceProfile)
        {
            return Array.Empty<string>();
        }

        var warnings = new List<string>
        {
            $"Imported interface profile '{interfaceProfile.Metadata.Name}' will not be activated automatically."
        };

        if (HasAttachmentSettings(interfaceProfile))
        {
            warnings.Add($"Imported interface profile '{interfaceProfile.Metadata.Name}' contains XDT attachment settings; folder paths and 6302/6303/6304/6305 must be reviewed before activation.");
        }

        return warnings;
    }

    private static bool HasAttachmentSettings(InterfaceProfileDefinition interfaceProfile)
    {
        var options = interfaceProfile.FolderOptions;

        return options.IsAttachmentProcessingEnabled
            || !string.IsNullOrWhiteSpace(options.AttachmentImportFolder)
            || !string.IsNullOrWhiteSpace(options.AttachmentExportFolder)
            || !string.IsNullOrWhiteSpace(options.AttachmentExternalLinkDescription);
    }

    private static string CreateMessage(
        TemplatePackageImportProfilePlan profilePlan,
        bool isIllegalBuiltInReplace,
        IReadOnlyList<string> warnings)
    {
        if (isIllegalBuiltInReplace)
        {
            return "BuiltIn profile replacement is not allowed. Dry-run blocks this item.";
        }

        if (warnings.Count == 0)
        {
            return profilePlan.Message;
        }

        return $"{profilePlan.Message} {string.Join(" ", warnings)}";
    }

    private sealed class ImportedProfileIndex
    {
        private readonly Dictionary<string, InterfaceProfileDefinition> _interfaceProfiles;

        private ImportedProfileIndex(Dictionary<string, InterfaceProfileDefinition> interfaceProfiles)
        {
            _interfaceProfiles = interfaceProfiles;
        }

        public static ImportedProfileIndex Create(TemplatePackageImportResult importResult)
        {
            return new ImportedProfileIndex(
                (importResult.InterfaceProfiles ?? Array.Empty<InterfaceProfileDefinition>())
                    .GroupBy(profile => profile.Metadata.Id, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase));
        }

        public InterfaceProfileDefinition? FindInterfaceProfile(string profileId)
        {
            return _interfaceProfiles.TryGetValue(profileId, out var profile)
                ? profile
                : null;
        }
    }

    private sealed class ProfilePlanIndex
    {
        private readonly Dictionary<ProfileKind, Dictionary<string, TemplatePackageImportProfilePlan>> _plans;

        private ProfilePlanIndex(Dictionary<ProfileKind, Dictionary<string, TemplatePackageImportProfilePlan>> plans)
        {
            _plans = plans;
        }

        public static ProfilePlanIndex Create(IEnumerable<TemplatePackageImportProfilePlan> profilePlans)
        {
            return new ProfilePlanIndex(
                profilePlans
                    .GroupBy(plan => plan.ProfileKind)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .GroupBy(plan => plan.ImportedProfileId, StringComparer.OrdinalIgnoreCase)
                            .ToDictionary(innerGroup => innerGroup.Key, innerGroup => innerGroup.First(), StringComparer.OrdinalIgnoreCase)));
        }

        public TemplatePackageImportProfilePlan? Find(ProfileKind profileKind, string profileId)
        {
            return _plans.TryGetValue(profileKind, out var byId)
                && byId.TryGetValue(profileId, out var plan)
                    ? plan
                    : null;
        }
    }

    private sealed class LocalProfileIndex
    {
        private readonly Dictionary<ProfileKind, Dictionary<string, LocalProfile>> _profiles;

        private LocalProfileIndex(Dictionary<ProfileKind, Dictionary<string, LocalProfile>> profiles)
        {
            _profiles = profiles;
        }

        public static LocalProfileIndex Create(ProfileCatalog catalog)
        {
            var profiles = new Dictionary<ProfileKind, Dictionary<string, LocalProfile>>();

            AddProfiles(profiles, ProfileKind.AisProfile, catalog.AisProfiles.Select(profile => profile.Metadata));
            AddProfiles(profiles, ProfileKind.DeviceProfile, catalog.DeviceProfiles.Select(profile => profile.Metadata));
            AddProfiles(profiles, ProfileKind.ExportProfile, catalog.ExportProfiles.Select(profile => profile.Metadata));
            AddProfiles(profiles, ProfileKind.InterfaceProfile, catalog.InterfaceProfiles.Select(profile => profile.Metadata));

            return new LocalProfileIndex(profiles);
        }

        public LocalProfile? Find(ProfileKind profileKind, string profileId)
        {
            return _profiles.TryGetValue(profileKind, out var byId)
                && byId.TryGetValue(profileId, out var profile)
                    ? profile
                    : null;
        }

        private static void AddProfiles(
            Dictionary<ProfileKind, Dictionary<string, LocalProfile>> profiles,
            ProfileKind profileKind,
            IEnumerable<ProfileMetadata> metadata)
        {
            profiles[profileKind] = metadata
                .Where(profile => !string.IsNullOrWhiteSpace(profile.Id))
                .GroupBy(profile => profile.Id, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => new LocalProfile(group.First().Id, group.First().Name),
                    StringComparer.OrdinalIgnoreCase);
        }
    }

    private sealed record LocalProfile(
        string Id,
        string Name);
}
