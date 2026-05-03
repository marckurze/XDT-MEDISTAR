namespace XdtDeviceBridge.Core;

public sealed record ExportProfileDefinition(
    ProfileMetadata Metadata,
    string TargetAisProfileId,
    string SourceDeviceProfileId,
    string OutputEncoding,
    IReadOnlyList<ExportRuleDefinition> Rules)
{
    public IReadOnlyList<string> Validate()
    {
        return ExportProfileDefinitionValidator.Validate(this);
    }
}
