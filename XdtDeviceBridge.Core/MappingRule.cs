namespace XdtDeviceBridge.Core;

public sealed record MappingRule(
    string Id,
    string TargetFieldCode,
    string TargetName,
    string SourcePath,
    string OutputTemplate,
    int SortOrder,
    bool IsEnabled);
