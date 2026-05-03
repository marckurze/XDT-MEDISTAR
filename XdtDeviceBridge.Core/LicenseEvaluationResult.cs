namespace XdtDeviceBridge.Core;

public sealed record LicenseEvaluationResult(
    LicenseStatus Status,
    int ActiveLicensedDeviceCount,
    int LicensedDeviceCount,
    DateTime? ValidUntil,
    IReadOnlyList<string> Messages,
    bool CanProcessFiles);
