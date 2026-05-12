using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportPlanBuilder
{
    public TemplatePackageImportPlan Build(TemplatePackageImportAnalysisResult analysisResult)
    {
        return Build(analysisResult, DateTimeOffset.UtcNow);
    }

    public TemplatePackageImportPlan Build(
        TemplatePackageImportAnalysisResult analysisResult,
        DateTimeOffset generatedAt)
    {
        ArgumentNullException.ThrowIfNull(analysisResult);

        var reservedIds = CreateReservedValues(analysisResult.ProfileDecisions, decision => decision.ImportedProfileId, decision => decision.ExistingProfileId);
        var reservedNames = CreateReservedValues(analysisResult.ProfileDecisions, decision => decision.ImportedProfileName, decision => decision.ExistingProfileName);
        var warnings = new List<string>(analysisResult.Warnings ?? Array.Empty<string>());
        var plans = new List<TemplatePackageImportProfilePlan>();

        foreach (var decision in analysisResult.ProfileDecisions ?? Array.Empty<TemplatePackageImportProfileDecision>())
        {
            var plan = CreateProfilePlan(decision, reservedIds, reservedNames);
            plans.Add(plan);

            if (decision.ProfileKind == ProfileKind.InterfaceProfile)
            {
                warnings.Add($"Interface profile '{decision.ImportedProfileName}' dependency remapping must be reviewed before productive import.");
            }

            if (decision.ConflictType == TemplatePackageImportConflictType.UnsafeFolderPath)
            {
                warnings.Add($"Profile '{decision.ImportedProfileName}' contains folder paths that must be reviewed before activation.");
            }
        }

        var blockingItems = plans.Where(plan => plan.IsBlocking).ToList();

        return new TemplatePackageImportPlan(
            PackageId: analysisResult.PackageId,
            PackageName: analysisResult.PackageName,
            GeneratedAt: generatedAt,
            ProfilePlans: plans,
            HasBlockingItems: blockingItems.Count > 0,
            BlockingItems: blockingItems,
            Warnings: warnings.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            TotalProfiles: plans.Count,
            PlannedImportAsNew: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            PlannedImportAsCopy: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            PlannedReplaceExisting: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.ReplaceExisting),
            PlannedKeepExisting: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.KeepExisting),
            PlannedSkip: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Skip),
            PlannedBlocked: plans.Count(plan => plan.PlannedAction == TemplatePackageImportAction.Blocked));
    }

    private static TemplatePackageImportProfilePlan CreateProfilePlan(
        TemplatePackageImportProfileDecision decision,
        Dictionary<ProfileKind, HashSet<string>> reservedIds,
        Dictionary<ProfileKind, HashSet<string>> reservedNames)
    {
        var action = DetermineAction(decision);
        var isBlocking = action == TemplatePackageImportAction.Blocked;
        var requiresRename = action == TemplatePackageImportAction.ImportAsCopy;
        var requiresUserDecision = RequiresUserDecision(decision, action);
        var proposedId = action switch
        {
            TemplatePackageImportAction.ImportAsNew => decision.ImportedProfileId,
            TemplatePackageImportAction.ImportAsCopy => CreateUniqueId(decision, reservedIds),
            _ => null
        };
        var proposedName = action switch
        {
            TemplatePackageImportAction.ImportAsNew => decision.ImportedProfileName,
            TemplatePackageImportAction.ImportAsCopy => CreateUniqueName(decision, reservedNames),
            _ => null
        };

        return new TemplatePackageImportProfilePlan(
            ProfileKind: decision.ProfileKind,
            ImportedProfileId: decision.ImportedProfileId,
            ImportedProfileName: decision.ImportedProfileName,
            ExistingProfileId: decision.ExistingProfileId,
            ExistingProfileName: decision.ExistingProfileName,
            ExistingProfileSource: decision.ExistingProfileSource,
            ConflictType: decision.ConflictType,
            PlannedAction: action,
            IsBlocking: isBlocking,
            RequiresUserDecision: requiresUserDecision,
            RequiresRename: requiresRename,
            ProposedProfileId: proposedId,
            ProposedProfileName: proposedName,
            Message: CreateMessage(decision, action));
    }

    private static TemplatePackageImportAction DetermineAction(TemplatePackageImportProfileDecision decision)
    {
        return decision.ConflictType switch
        {
            TemplatePackageImportConflictType.None => TemplatePackageImportAction.ImportAsNew,
            TemplatePackageImportConflictType.BuiltInProtected => TemplatePackageImportAction.Skip,
            TemplatePackageImportConflictType.SameIdExists => TemplatePackageImportAction.Skip,
            TemplatePackageImportConflictType.SameNameExists => TemplatePackageImportAction.Skip,
            TemplatePackageImportConflictType.VersionMismatch => TemplatePackageImportAction.Skip,
            TemplatePackageImportConflictType.UnsafeFolderPath => TemplatePackageImportAction.ImportAsNew,
            TemplatePackageImportConflictType.MissingDependency => TemplatePackageImportAction.Blocked,
            TemplatePackageImportConflictType.InvalidProfile => TemplatePackageImportAction.Blocked,
            TemplatePackageImportConflictType.UnsupportedProfileKind => TemplatePackageImportAction.Blocked,
            _ => decision.SuggestedAction == TemplatePackageImportAction.Blocked
                ? TemplatePackageImportAction.Blocked
                : decision.SuggestedAction
        };
    }

    private static bool RequiresUserDecision(
        TemplatePackageImportProfileDecision decision,
        TemplatePackageImportAction action)
    {
        return action == TemplatePackageImportAction.Blocked
            || action == TemplatePackageImportAction.ImportAsCopy
            || action == TemplatePackageImportAction.Skip
            || decision.ConflictType == TemplatePackageImportConflictType.UnsafeFolderPath
            || decision.ConflictType == TemplatePackageImportConflictType.VersionMismatch;
    }

    private static string CreateMessage(
        TemplatePackageImportProfileDecision decision,
        TemplatePackageImportAction action)
    {
        return decision.ConflictType switch
        {
            TemplatePackageImportConflictType.None => "Profile can be imported as new.",
            TemplatePackageImportConflictType.BuiltInProtected => $"BuiltIn profile '{decision.ExistingProfileName}' is protected. Safe default is skip; choose import as copy consciously if needed.",
            TemplatePackageImportConflictType.SameIdExists when decision.ExistingProfileSource == TemplatePackageImportExistingProfileSource.BuiltIn => $"BuiltIn profile '{decision.ExistingProfileName}' is protected. Safe default is skip; choose import as copy consciously if needed.",
            TemplatePackageImportConflictType.SameNameExists when decision.ExistingProfileSource == TemplatePackageImportExistingProfileSource.BuiltIn => $"BuiltIn profile '{decision.ExistingProfileName}' is protected. Safe default is skip; choose import as copy consciously if needed.",
            TemplatePackageImportConflictType.SameIdExists => "A local profile with the same Id exists. Safe default is skip; choose import as copy consciously if needed.",
            TemplatePackageImportConflictType.SameNameExists => "A local profile with the same name exists. Safe default is skip; choose import as copy consciously if needed.",
            TemplatePackageImportConflictType.VersionMismatch => "A local profile with a different version exists. Safe default is skip; choose import as copy consciously if needed.",
            TemplatePackageImportConflictType.UnsafeFolderPath => "Folder paths must be reviewed before productive activation. The profile is only planned for import and must not be activated automatically.",
            TemplatePackageImportConflictType.MissingDependency => decision.Message,
            TemplatePackageImportConflictType.InvalidProfile => decision.Message,
            TemplatePackageImportConflictType.UnsupportedProfileKind => decision.Message,
            _ => action == TemplatePackageImportAction.Blocked ? decision.Message : $"{decision.Message} Planned action: {action}."
        };
    }

    private static string CreateUniqueId(
        TemplatePackageImportProfileDecision decision,
        Dictionary<ProfileKind, HashSet<string>> reservedIds)
    {
        var baseId = CreateSafeId(string.IsNullOrWhiteSpace(decision.ImportedProfileId)
            ? decision.ImportedProfileName
            : decision.ImportedProfileId);

        return CreateUniqueValue(
            decision.ProfileKind,
            reservedIds,
            $"{baseId}-import",
            index => $"{baseId}-import-{index}");
    }

    private static string CreateUniqueName(
        TemplatePackageImportProfileDecision decision,
        Dictionary<ProfileKind, HashSet<string>> reservedNames)
    {
        return CreateUniqueValue(
            decision.ProfileKind,
            reservedNames,
            $"{decision.ImportedProfileName} (Import)",
            index => $"{decision.ImportedProfileName} (Import {index})");
    }

    private static string CreateUniqueValue(
        ProfileKind profileKind,
        Dictionary<ProfileKind, HashSet<string>> reservedValues,
        string firstCandidate,
        Func<int, string> createNumberedCandidate)
    {
        var values = GetReservedSet(profileKind, reservedValues);

        if (!values.Contains(firstCandidate))
        {
            values.Add(firstCandidate);
            return firstCandidate;
        }

        var index = 2;
        while (true)
        {
            var candidate = createNumberedCandidate(index);
            if (!values.Contains(candidate))
            {
                values.Add(candidate);
                return candidate;
            }

            index++;
        }
    }

    private static string CreateSafeId(string value)
    {
        var builder = new StringBuilder();
        var previousWasSeparator = false;

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousWasSeparator = false;
            }
            else if (!previousWasSeparator)
            {
                builder.Append('-');
                previousWasSeparator = true;
            }
        }

        var result = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(result) ? "profile" : result;
    }

    private static Dictionary<ProfileKind, HashSet<string>> CreateReservedValues(
        IEnumerable<TemplatePackageImportProfileDecision> decisions,
        Func<TemplatePackageImportProfileDecision, string?> getImportedValue,
        Func<TemplatePackageImportProfileDecision, string?> getExistingValue)
    {
        var result = new Dictionary<ProfileKind, HashSet<string>>();

        foreach (var decision in decisions)
        {
            AddReservedValue(result, decision.ProfileKind, getImportedValue(decision));
            AddReservedValue(result, decision.ProfileKind, getExistingValue(decision));
        }

        return result;
    }

    private static void AddReservedValue(
        Dictionary<ProfileKind, HashSet<string>> reservedValues,
        ProfileKind profileKind,
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        GetReservedSet(profileKind, reservedValues).Add(value);
    }

    private static HashSet<string> GetReservedSet(
        ProfileKind profileKind,
        Dictionary<ProfileKind, HashSet<string>> reservedValues)
    {
        if (!reservedValues.TryGetValue(profileKind, out var values))
        {
            values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            reservedValues[profileKind] = values;
        }

        return values;
    }
}
