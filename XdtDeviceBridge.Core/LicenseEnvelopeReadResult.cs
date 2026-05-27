namespace XdtDeviceBridge.Core;

public sealed record LicenseEnvelopeReadResult(
    LicenseEnvelope? Envelope,
    LicensePayload? Payload,
    byte[] PayloadBytes,
    byte[] SignatureBytes,
    LicenseSignatureVerificationStatus Status,
    IReadOnlyList<string> Messages)
{
    public bool Success => Status == LicenseSignatureVerificationStatus.NotChecked
        && Envelope is not null
        && Payload is not null
        && PayloadBytes.Length > 0
        && SignatureBytes.Length > 0;
}
