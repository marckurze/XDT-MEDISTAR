namespace XdtDeviceBridge.Infrastructure;

public sealed record DeviceTechnicalProfileCatalog(
    IReadOnlyList<DeviceTechnicalProfile> Profiles);

public sealed record DeviceTechnicalProfile(
    string DeviceProfileId,
    string Manufacturer,
    string Model,
    string DeviceType,
    string ConnectionType,
    bool IsBidirectional,
    string ImagePath,
    string ShortDescription,
    IReadOnlyList<DeviceTechnicalProfileMeasurement> Measurements,
    IReadOnlyList<DeviceTechnicalProfileExampleMeasurement>? ExampleMeasurements,
    IReadOnlyList<string>? TechnicalNotes,
    IReadOnlyList<string>? Limitations,
    string TechnicianNote = "");

public sealed record DeviceTechnicalProfileMeasurement(
    string Name,
    string Description,
    string UsedFor);

public sealed record DeviceTechnicalProfileExampleMeasurement(
    string Label,
    string Value);

public sealed record DeviceTechnicalProfileOverride(
    string DeviceProfileId,
    string? ShortDescription = null,
    IReadOnlyList<DeviceTechnicalProfileMeasurement>? Measurements = null,
    IReadOnlyList<DeviceTechnicalProfileExampleMeasurement>? ExampleMeasurements = null,
    IReadOnlyList<string>? TechnicalNotes = null,
    IReadOnlyList<string>? Limitations = null,
    string? TechnicianNote = null);

public sealed record DeviceTechnicalProfileOverrideCatalog(
    IReadOnlyList<DeviceTechnicalProfileOverride> Profiles);
