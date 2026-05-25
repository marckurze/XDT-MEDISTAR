namespace XdtDeviceBridge.Core;

public sealed record DeviceProfileDefinition(
    ProfileMetadata Metadata,
    string Manufacturer,
    string Model,
    string DeviceType,
    string ParserMode,
    IReadOnlyList<DeviceMeasurementDefinition> Measurements,
    IReadOnlyList<string> SupportedExaminationTypes,
    bool CanContainMultipleExaminationTypes,
    bool IsBidirectional = false,
    string DeviceImagePath = "")
{
    public IReadOnlyList<string> Validate()
    {
        return DeviceProfileDefinitionValidator.Validate(this);
    }
}

public sealed record DeviceOutputConfiguration(
    bool IsEnabled,
    string OutputFolder,
    string FileNameTemplate,
    string Format)
{
    public static DeviceOutputConfiguration Disabled { get; } = new(
        IsEnabled: false,
        OutputFolder: string.Empty,
        FileNameTemplate: string.Empty,
        Format: string.Empty);
}
