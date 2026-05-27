using System.Security.Cryptography;

namespace XdtDeviceBridge.Core;

public sealed class LicenseSignatureVerifier : ILicenseSignatureVerifier
{
    private readonly ILicensePublicKeyProvider _publicKeyProvider;

    public LicenseSignatureVerifier()
        : this(new LicensePublicKeyProvider())
    {
    }

    public LicenseSignatureVerifier(ILicensePublicKeyProvider publicKeyProvider)
    {
        _publicKeyProvider = publicKeyProvider ?? throw new ArgumentNullException(nameof(publicKeyProvider));
    }

    public LicenseSignatureVerificationResult Verify(
        byte[] payloadBytes,
        byte[] signatureBytes,
        string algorithm,
        string keyId)
    {
        ArgumentNullException.ThrowIfNull(payloadBytes);
        ArgumentNullException.ThrowIfNull(signatureBytes);

        if (signatureBytes.Length == 0)
        {
            return new LicenseSignatureVerificationResult(
                LicenseSignatureVerificationStatus.MissingSignature,
                "License signature is missing.");
        }

        if (!string.Equals(algorithm, LicenseSignatureAlgorithms.RsaPssSha256, StringComparison.Ordinal))
        {
            return new LicenseSignatureVerificationResult(
                LicenseSignatureVerificationStatus.UnsupportedAlgorithm,
                $"Unsupported license signature algorithm: {algorithm}.");
        }

        if (!_publicKeyProvider.TryGetPublicKey(keyId, out var publicKeyBytes))
        {
            return new LicenseSignatureVerificationResult(
                LicenseSignatureVerificationStatus.UnknownKeyId,
                $"Unknown license public key id: {keyId}.");
        }

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
            var isValid = rsa.VerifyData(
                payloadBytes,
                signatureBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pss);

            return isValid
                ? new LicenseSignatureVerificationResult(LicenseSignatureVerificationStatus.Valid, "License signature valid.")
                : new LicenseSignatureVerificationResult(LicenseSignatureVerificationStatus.Invalid, XdtBoxLicenseConstants.InvalidSignatureMessage);
        }
        catch (CryptographicException ex)
        {
            return new LicenseSignatureVerificationResult(
                LicenseSignatureVerificationStatus.VerificationError,
                $"License signature verification failed: {ex.Message}");
        }
    }
}
