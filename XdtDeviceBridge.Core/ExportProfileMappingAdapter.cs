namespace XdtDeviceBridge.Core;

public sealed class ExportProfileMappingAdapter
{
    private const string DummySourcePath = "AIS.PatientNumber";

    public IReadOnlyList<MappingRule> Adapt(ExportProfileDefinition exportProfile)
    {
        ArgumentNullException.ThrowIfNull(exportProfile);

        return exportProfile.Rules
            .Select(Adapt)
            .ToList();
    }

    private static MappingRule Adapt(ExportRuleDefinition rule)
    {
        var sourcePath = rule.RuleType switch
        {
            ExportRuleType.StaticValue => DummySourcePath,
            ExportRuleType.Template when string.IsNullOrWhiteSpace(rule.SourcePath) => DummySourcePath,
            _ => rule.SourcePath ?? string.Empty
        };

        return new MappingRule(
            Id: rule.Id,
            TargetFieldCode: rule.TargetFieldCode,
            TargetName: rule.TargetName,
            SourcePath: sourcePath,
            OutputTemplate: rule.OutputTemplate,
            SortOrder: rule.SortOrder,
            IsEnabled: rule.IsEnabled);
    }
}
