namespace XdtDeviceBridge.Core;

public sealed record LicenseRequestDevice(
    string Id,
    string Name,
    string Manufacturer,
    string Model,
    string ProfileId,
    bool IsActive,
    bool IsLicenseRequired);
