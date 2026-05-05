namespace XdtDeviceBridge.Core;

public sealed record LicensedDeviceState(
    string InterfaceProfileId,
    string DisplayName,
    bool IsActive,
    bool IsLicenseRequired,
    bool IsCoveredByLicense,
    bool IsInGracePeriod,
    DateTime? GracePeriodStartedAt,
    DateTime? GracePeriodEndsAt,
    string StatusMessage);
