namespace XdtDeviceBridge.Core;

public sealed class ExportProfileDraftService
{
    public ExportProfileDraftResult CreateUserDefinedCopy(
        ExportProfileDefinition originalProfile,
        string newProfileName,
        ExportRuleDefinition? draftRule,
        string? replaceRuleId,
        IEnumerable<ExportRuleDefinition> temporaryRules,
        DateTimeOffset timestamp,
        string? createdBy,
        Func<string>? idFactory = null)
    {
        ArgumentNullException.ThrowIfNull(originalProfile);
        ArgumentNullException.ThrowIfNull(temporaryRules);

        var issues = new List<string>();
        var profileName = newProfileName.Trim();
        if (string.IsNullOrWhiteSpace(profileName))
        {
            issues.Add("Profilname darf nicht leer sein.");
        }

        var rules = BuildRules(originalProfile.Rules, draftRule, replaceRuleId, temporaryRules);
        ValidateRulesBeforeSave(rules, issues);

        if (issues.Count > 0)
        {
            return new ExportProfileDraftResult(null, issues);
        }

        var newId = idFactory?.Invoke();
        if (string.IsNullOrWhiteSpace(newId))
        {
            newId = $"export-{Guid.NewGuid():N}";
        }

        var metadata = new ProfileMetadata(
            Id: newId,
            Name: profileName,
            ProfileKind: ProfileKind.ExportProfile,
            Description: $"Benutzerdefinierte Kopie von {originalProfile.Metadata.Name}",
            Vendor: originalProfile.Metadata.Vendor,
            Product: originalProfile.Metadata.Product,
            Version: "1.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: createdBy,
            IsBuiltIn: false,
            IsUserDefined: true);

        var profile = new ExportProfileDefinition(
            Metadata: metadata,
            TargetAisProfileId: originalProfile.TargetAisProfileId,
            SourceDeviceProfileId: originalProfile.SourceDeviceProfileId,
            OutputEncoding: originalProfile.OutputEncoding,
            Rules: rules);

        issues.AddRange(ExportProfileDefinitionValidator.Validate(profile));

        return issues.Count == 0
            ? new ExportProfileDraftResult(profile, Array.Empty<string>())
            : new ExportProfileDraftResult(null, issues);
    }

    private static IReadOnlyList<ExportRuleDefinition> BuildRules(
        IReadOnlyList<ExportRuleDefinition> originalRules,
        ExportRuleDefinition? draftRule,
        string? replaceRuleId,
        IEnumerable<ExportRuleDefinition> temporaryRules)
    {
        var effectiveRules = originalRules
            .Select(rule => draftRule is not null && string.Equals(rule.Id, replaceRuleId, StringComparison.Ordinal)
                ? draftRule
                : rule)
            .ToList();

        var temporaryRuleList = temporaryRules.ToList();
        foreach (var temporaryRule in temporaryRuleList)
        {
            if (draftRule is not null && string.Equals(temporaryRule.Id, draftRule.Id, StringComparison.Ordinal))
            {
                effectiveRules.Add(draftRule);
            }
            else
            {
                effectiveRules.Add(temporaryRule);
            }
        }

        return effectiveRules
            .OrderBy(rule => rule.SortOrder)
            .ThenBy(rule => rule.Id, StringComparer.Ordinal)
            .ToList();
    }

    private static void ValidateRulesBeforeSave(IReadOnlyList<ExportRuleDefinition> rules, List<string> issues)
    {
        var duplicateSortOrders = rules
            .GroupBy(rule => rule.SortOrder)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(sortOrder => sortOrder)
            .ToList();

        foreach (var duplicateSortOrder in duplicateSortOrders)
        {
            issues.Add($"Mehrere Regeln verwenden dieselbe SortOrder: {duplicateSortOrder}");
        }

        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.TargetFieldCode))
            {
                issues.Add($"Regel {rule.Id}: TargetFieldCode darf nicht leer sein.");
            }

            if (string.IsNullOrWhiteSpace(rule.OutputTemplate))
            {
                issues.Add($"Regel {rule.Id}: OutputTemplate darf nicht leer sein.");
            }

            if (!Enum.IsDefined(typeof(ExportRuleType), rule.RuleType))
            {
                issues.Add($"Regel {rule.Id}: RuleType ist ungültig.");
            }

            if (rule.RuleType == ExportRuleType.AisField
                && string.IsNullOrWhiteSpace(rule.SourcePath))
            {
                issues.Add($"Regel {rule.Id}: SourcePath darf bei AisField nicht leer sein.");
            }

            if (rule.RuleType == ExportRuleType.DeviceField
                && string.IsNullOrWhiteSpace(rule.SourcePath))
            {
                issues.Add($"Regel {rule.Id}: SourcePath darf bei DeviceField nicht leer sein.");
            }
        }
    }
}
