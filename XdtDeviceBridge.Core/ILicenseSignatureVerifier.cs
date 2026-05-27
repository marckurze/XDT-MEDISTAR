namespace XdtDeviceBridge.Core;

public interface ILicenseSignatureVerifier
{
    LicenseSignatureVerificationResult Verify(
        byte[] payloadBytes,
        byte[] signatureBytes,
        string algorithm,
        string keyId);
}
