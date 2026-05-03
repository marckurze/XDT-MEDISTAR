namespace XdtDeviceBridge.Core;

public sealed record ExportRuleDefinition(
    string Id,
    string TargetFieldCode,
    string TargetName,
    ExportRuleType RuleType,
    string? SourcePath,
    string OutputTemplate,
    int SortOrder,
    bool IsEnabled,
    string? Description);
