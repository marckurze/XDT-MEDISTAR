namespace XdtDeviceBridge.Core;

public enum LicenseSignatureVerificationStatus
{
    NotChecked,
    Valid,
    Invalid,
    UnsupportedAlgorithm,
    UnknownKeyId,
    MalformedEnvelope,
    MalformedPayload,
    MissingSignature,
    VerificationError,
    UnsupportedFormatVersion
}
