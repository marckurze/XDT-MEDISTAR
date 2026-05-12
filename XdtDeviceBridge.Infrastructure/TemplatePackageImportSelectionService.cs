using System.Text;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class TemplatePackageImportSelectionService
{
    public TemplatePackageImportPlan Apply(
        TemplatePackageImportPlan plan,
        IReadOnlyList<TemplatePackageImportUserSelection> selections)
    {
        ArgumentNullException.ThrowIfNull(plan);

        var selectionIndex = (selections ?? Array.Empty<TemplatePackageImportUserSelection>())
            .GroupBy(selection => CreateProfileKey(selection.ProfileKind, selection.ImportedProfileId), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last(), StringComparer.OrdinalIgnoreCase);
        var reservedIds = CreateReservedValues(plan.ProfilePlans, profilePlan => profilePlan.ImportedProfileId, profilePlan => profilePlan.ExistingProfileId);
        var reservedNames = CreateReservedValues(plan.ProfilePlans, profilePlan => profilePlan.ImportedProfileName, profilePlan => profilePlan.ExistingProfileName);
        var updatedPlans = new List<TemplatePackageImportProfilePlan>();

        foreach (var profilePlan in plan.ProfilePlans ?? Array.Empty<TemplatePackageImportProfilePlan>())
        {
            selectionIndex.TryGetValue(CreateProfileKey(profilePlan.ProfileKind, profilePlan.ImportedProfileId), out var selection);
            updatedPlans.Add(ApplySelection(profilePlan, selection, reservedIds, reservedNames));
        }

        var blockingItems = updatedPlans.Where(profilePlan => profilePlan.IsBlocking).ToList();
        return plan with
        {
            ProfilePlans = updatedPlans,
            HasBlockingItems = blockingItems.Count > 0,
            BlockingItems = blockingItems,
            PlannedImportAsNew = updatedPlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.ImportAsNew),
            PlannedImportAsCopy = updatedPlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.ImportAsCopy),
            PlannedReplaceExisting = updatedPlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.ReplaceExisting),
            PlannedKeepExisting = updatedPlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.KeepExisting),
            PlannedSkip = updatedPlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.Skip),
            PlannedBlocked = updatedPlans.Count(profilePlan => profilePlan.PlannedAction == TemplatePackageImportAction.Blocked)
        };
    }

    private static TemplatePackageImportProfilePlan ApplySelection(
        TemplatePackageImportProfilePlan profilePlan,
        TemplatePackageImportUserSelection? selection,
        Dictionary<ProfileKind, HashSet<string>> reservedIds,
        Dictionary<ProfileKind, HashSet<string>> reservedNames)
    {
        if (profilePlan.IsBlocking || profilePlan.PlannedAction == TemplatePackageImportAction.Blocked)
        {
            return profilePlan with
            {
                PlannedAction = TemplatePackageImportAction.Blocked,
                IsBlocking = true,
                RequiresUserDecision = true,
                RequiresRename = false,
                ProposedProfileId = null,
                ProposedProfileName = null
            };
        }

        if (selection is { IsValid: false })
        {
            return CreateBlockedPlan(
                profilePlan,
                string.IsNullOrWhiteSpace(selection.ValidationMessage)
                    ? "Auswahl ist ungueltig."
                    : selection.ValidationMessage);
        }

        var selectedAction = selection?.SelectedAction ?? profilePlan.PlannedAction;
        return selectedAction switch
        {
            TemplatePackageImportAction.ImportAsNew => CreateImportAsNewPlan(profilePlan),
            TemplatePackageImportAction.ImportAsCopy => CreateImportAsCopyPlan(profilePlan, selection, reservedIds, reservedNames),
            TemplatePackageImportAction.KeepExisting => CreateKeepExistingPlan(profilePlan),
            TemplatePackageImportAction.Skip => CreateSkipPlan(profilePlan),
            TemplatePackageImportAction.Blocked => CreateBlockedPlan(profilePlan, "Profile remains blocked by user selection."),
            TemplatePackageImportAction.ReplaceExisting => CreateBlockedPlan(profilePlan, "ReplaceExisting is not supported in this step."),
            _ => CreateBlockedPlan(profilePlan, $"Unsupported user selection: {selectedAction}.")
        };
    }

    private static TemplatePackageImportProfilePlan CreateImportAsNewPlan(TemplatePackageImportProfilePlan profilePlan)
    {
        if (profilePlan.ConflictType != TemplatePackageImportConflictType.None)
        {
            return CreateBlockedPlan(profilePlan, "Neu importieren ist nur ohne ID- oder Namenskonflikt erlaubt.");
        }

        return profilePlan with
        {
            PlannedAction = TemplatePackageImportAction.ImportAsNew,
            IsBlocking = false,
            RequiresUserDecision = false,
            RequiresRename = false,
            ProposedProfileId = profilePlan.ImportedProfileId,
            ProposedProfileName = profilePlan.ImportedProfileName,
            Message = "Profile will be imported as new."
        };
    }

    private static TemplatePackageImportProfilePlan CreateImportAsCopyPlan(
        TemplatePackageImportProfilePlan profilePlan,
        TemplatePackageImportUserSelection? selection,
        Dictionary<ProfileKind, HashSet<string>> reservedIds,
        Dictionary<ProfileKind, HashSet<string>> reservedNames)
    {
        var proposedName = ResolveCopyTargetName(profilePlan, selection, reservedNames);
        if (proposedName.BlockingMessage is not null)
        {
            return CreateBlockedPlan(profilePlan, proposedName.BlockingMessage);
        }

        var proposedId = CreateUniqueId(profilePlan, reservedIds);

        return profilePlan with
        {
            PlannedAction = TemplatePackageImportAction.ImportAsCopy,
            IsBlocking = false,
            RequiresUserDecision = true,
            RequiresRename = true,
            ProposedProfileId = proposedId,
            ProposedProfileName = proposedName.Value,
            Message = "Profile will be imported as a safe UserDefined copy."
        };
    }

    private static CopyTargetNameResolution ResolveCopyTargetName(
        TemplatePackageImportProfilePlan profilePlan,
        TemplatePackageImportUserSelection? selection,
        Dictionary<ProfileKind, HashSet<string>> reservedNames)
    {
        var requestedName = selection?.TargetProfileName;
        if (requestedName is null)
        {
            return new CopyTargetNameResolution(CreateUniqueName(profilePlan, reservedNames), null);
        }

        var trimmedName = requestedName.Trim();
        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            return new CopyTargetNameResolution(
                null,
                "Zielname darf nicht leer sein. Bitte einen eindeutigen Namen fuer die Kopie eingeben.");
        }

        var names = GetReservedSet(profilePlan.ProfileKind, reservedNames);
        if (names.Contains(trimmedName))
        {
            return new CopyTargetNameResolution(
                null,
                $"Zielname '{trimmedName}' ist bereits vorhanden. Bitte einen eindeutigen Namen fuer die Kopie eingeben.");
        }

        names.Add(trimmedName);
        return new CopyTargetNameResolution(trimmedName, null);
    }

    private static TemplatePackageImportProfilePlan CreateKeepExistingPlan(TemplatePackageImportProfilePlan profilePlan)
    {
        if (string.IsNullOrWhiteSpace(profilePlan.ExistingProfileId))
        {
            return CreateBlockedPlan(profilePlan, "Bestehendes behalten ist nur möglich, wenn ein lokales Profil vorhanden ist.");
        }

        return profilePlan with
        {
            PlannedAction = TemplatePackageImportAction.KeepExisting,
            IsBlocking = false,
            RequiresUserDecision = true,
            RequiresRename = false,
            ProposedProfileId = null,
            ProposedProfileName = null,
            Message = "Existing local profile will be kept."
        };
    }

    private static TemplatePackageImportProfilePlan CreateSkipPlan(TemplatePackageImportProfilePlan profilePlan)
    {
        return profilePlan with
        {
            PlannedAction = TemplatePackageImportAction.Skip,
            IsBlocking = false,
            RequiresUserDecision = true,
            RequiresRename = false,
            ProposedProfileId = null,
            ProposedProfileName = null,
            Message = "Imported profile will be skipped."
        };
    }

    private static TemplatePackageImportProfilePlan CreateBlockedPlan(
        TemplatePackageImportProfilePlan profilePlan,
        string message)
    {
        return profilePlan with
        {
            PlannedAction = TemplatePackageImportAction.Blocked,
            IsBlocking = true,
            RequiresUserDecision = true,
            RequiresRename = false,
            ProposedProfileId = null,
            ProposedProfileName = null,
            Message = message
        };
    }

    private static string CreateUniqueId(
        TemplatePackageImportProfilePlan profilePlan,
        Dictionary<ProfileKind, HashSet<string>> reservedIds)
    {
        var baseId = CreateSafeId(string.IsNullOrWhiteSpace(profilePlan.ImportedProfileId)
            ? profilePlan.ImportedProfileName
            : profilePlan.ImportedProfileId);

        return CreateUniqueValue(
            profilePlan.ProfileKind,
            reservedIds,
            $"{baseId}-import",
            index => $"{baseId}-import-{index}");
    }

    private static string CreateUniqueName(
        TemplatePackageImportProfilePlan profilePlan,
        Dictionary<ProfileKind, HashSet<string>> reservedNames)
    {
        return CreateUniqueValue(
            profilePlan.ProfileKind,
            reservedNames,
            $"{profilePlan.ImportedProfileName} (Import)",
            index => $"{profilePlan.ImportedProfileName} (Import {index})");
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
        IEnumerable<TemplatePackageImportProfilePlan> profilePlans,
        Func<TemplatePackageImportProfilePlan, string?> getImportedValue,
        Func<TemplatePackageImportProfilePlan, string?> getExistingValue)
    {
        var result = new Dictionary<ProfileKind, HashSet<string>>();

        foreach (var profilePlan in profilePlans)
        {
            AddReservedValue(result, profilePlan.ProfileKind, getImportedValue(profilePlan));
            AddReservedValue(result, profilePlan.ProfileKind, getExistingValue(profilePlan));
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

    private static string CreateProfileKey(ProfileKind profileKind, string profileId)
    {
        return $"{profileKind}:{profileId}";
    }

    private sealed record CopyTargetNameResolution(
        string? Value,
        string? BlockingMessage);
}
