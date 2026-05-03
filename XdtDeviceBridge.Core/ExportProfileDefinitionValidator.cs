namespace XdtDeviceBridge.Core;

public static class ExportProfileDefinitionValidator
{
    public static IReadOnlyList<string> Validate(ExportProfileDefinition profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var issues = new List<string>();

        if (profile.Metadata is null)
        {
            issues.Add("Metadata must not be null.");
        }
        else if (profile.Metadata.ProfileKind != ProfileKind.ExportProfile)
        {
            issues.Add("Metadata.ProfileKind must be ExportProfile.");
        }

        if (string.IsNullOrWhiteSpace(profile.TargetAisProfileId))
        {
            issues.Add("TargetAisProfileId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.SourceDeviceProfileId))
        {
            issues.Add("SourceDeviceProfileId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(profile.OutputEncoding))
        {
            issues.Add("OutputEncoding must not be empty.");
        }

        if (profile.Rules is null)
        {
            issues.Add("Rules must not be null.");
            return issues;
        }

        var activeRules = profile.Rules.Where(rule => rule.IsEnabled).ToList();

        foreach (var rule in activeRules)
        {
            if (string.IsNullOrWhiteSpace(rule.TargetFieldCode))
            {
                issues.Add($"Rule {rule.Id}: TargetFieldCode must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(rule.OutputTemplate))
            {
                issues.Add($"Rule {rule.Id}: OutputTemplate must not be empty.");
            }

            if (rule.RuleType == ExportRuleType.AisField
                && (string.IsNullOrWhiteSpace(rule.SourcePath)
                    || !rule.SourcePath.StartsWith("AIS.", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add($"Rule {rule.Id}: AisField SourcePath must start with AIS.");
            }

            if (rule.RuleType == ExportRuleType.DeviceField
                && (string.IsNullOrWhiteSpace(rule.SourcePath)
                    || !rule.SourcePath.StartsWith("Device.", StringComparison.OrdinalIgnoreCase)))
            {
                issues.Add($"Rule {rule.Id}: DeviceField SourcePath must start with Device.");
            }
        }

        var duplicateSortOrders = activeRules
            .GroupBy(rule => rule.SortOrder)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var duplicateSortOrder in duplicateSortOrders)
        {
            issues.Add($"Active rules contain duplicate SortOrder: {duplicateSortOrder}");
        }

        return issues;
    }
}
