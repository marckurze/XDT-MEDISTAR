namespace XdtDeviceBridge.Core;

public sealed record DeviceProfileDefinition(
    ProfileMetadata Metadata,
    string Manufacturer,
    string Model,
    string DeviceType,
    string ParserMode,
    IReadOnlyList<DeviceMeasurementDefinition> Measurements,
    IReadOnlyList<string> SupportedExaminationTypes,
    bool CanContainMultipleExaminationTypes)
{
    public IReadOnlyList<string> Validate()
    {
        return DeviceProfileDefinitionValidator.Validate(this);
    }
}
