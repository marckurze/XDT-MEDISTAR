namespace XdtDeviceBridge.Core;

public enum LicenseV1PolicyStatus
{
    Valid,
    MissingLicense,
    InvalidSignature,
    WrongProduct,
    InstallationMismatch,
    NotYetValid,
    ExpiredWithinGrace,
    ExpiredAfterGrace,
    DeviceLimitExceededWithinGrace,
    DeviceLimitExceededAfterGrace
}
