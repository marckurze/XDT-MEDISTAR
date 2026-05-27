namespace XdtDeviceBridge.Core;

public sealed record LicenseRequestDevice(
    string Id,
    string Name,
    string Manufacturer,
    string Model,
    string ProfileId,
    bool IsActive,
    bool IsLicenseRequired,
    string InterfaceProfileId = "",
    string DisplayName = "",
    string DeviceProfileId = "",
    string DeviceDisplayName = "",
    DeviceConnectionKind ConnectionKind = DeviceConnectionKind.NetworkLan);
