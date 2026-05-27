namespace XdtDeviceBridge.Core;

public sealed record IssuedLicenseDeviceRecord(
    string DisplayName,
    string DeviceDisplayName,
    string InterfaceProfileId,
    string DeviceProfileId,
    DeviceConnectionKind ConnectionKind);
