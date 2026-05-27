namespace XdtDeviceBridge.Core;

public enum LicenseV1PolicyStatus
{
    Valid,
    MissingLicense,
    InvalidSignature,
    InstallationMismatch,
    NotYetValid,
    ExpiredWithinGrace,
    ExpiredAfterGrace,
    DeviceLimitExceededWithinGrace,
    DeviceLimitExceededAfterGrace
}
