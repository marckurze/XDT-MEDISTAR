namespace XdtDeviceBridge.Core;

public sealed record LicenseV1PolicyEvaluationResult(
    LicenseV1PolicyStatus Status,
    bool CanStartDeviceConnections,
    bool IsWarningOnly,
    DateTime? GraceEndsAtUtc,
    string Message)
{
    public bool IsBlocked => !CanStartDeviceConnections;
}
