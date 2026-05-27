namespace XdtDeviceBridge.Core;

public sealed record LicenseImportResult(
    bool CanPersistLicenseFile,
    LicenseEnvelope? Envelope,
    LicensePayload? Payload,
    LicenseSignatureVerificationStatus SignatureStatus,
    LicenseV1PolicyEvaluationResult? PolicyEvaluation,
    string UserMessage)
{
    public bool HasVerifiedSignature => SignatureStatus == LicenseSignatureVerificationStatus.Valid;
}
