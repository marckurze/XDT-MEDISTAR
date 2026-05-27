using System.Security.Cryptography;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseSignatureVerifierTests
{
    [Fact]
    public void Verify_ShouldAcceptValidRsaPssSha256Signature()
    {
        using var rsa = RSA.Create(2048);
        var verifier = CreateVerifier(rsa);
        var payloadBytes = CreatePayloadBytes();
        var signatureBytes = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

        var result = verifier.Verify(
            payloadBytes,
            signatureBytes,
            LicenseSignatureAlgorithms.RsaPssSha256,
            "test-key");

        Assert.Equal(LicenseSignatureVerificationStatus.Valid, result.Status);
    }

    [Fact]
    public void Verify_ShouldRejectManipulatedPayload()
    {
        using var rsa = RSA.Create(2048);
        var verifier = CreateVerifier(rsa);
        var payloadBytes = CreatePayloadBytes();
        var signatureBytes = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        payloadBytes[0] ^= 0x01;

        var result = verifier.Verify(
            payloadBytes,
            signatureBytes,
            LicenseSignatureAlgorithms.RsaPssSha256,
            "test-key");

        Assert.Equal(LicenseSignatureVerificationStatus.Invalid, result.Status);
    }

    [Fact]
    public void Verify_ShouldRejectManipulatedSignature()
    {
        using var rsa = RSA.Create(2048);
        var verifier = CreateVerifier(rsa);
        var payloadBytes = CreatePayloadBytes();
        var signatureBytes = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        signatureBytes[0] ^= 0x01;

        var result = verifier.Verify(
            payloadBytes,
            signatureBytes,
            LicenseSignatureAlgorithms.RsaPssSha256,
            "test-key");

        Assert.Equal(LicenseSignatureVerificationStatus.Invalid, result.Status);
    }

    [Fact]
    public void Verify_ShouldReportUnknownKeyId()
    {
        using var rsa = RSA.Create(2048);
        var verifier = CreateVerifier(rsa);

        var result = verifier.Verify(
            CreatePayloadBytes(),
            new byte[] { 1, 2, 3 },
            LicenseSignatureAlgorithms.RsaPssSha256,
            "unknown-key");

        Assert.Equal(LicenseSignatureVerificationStatus.UnknownKeyId, result.Status);
    }

    [Fact]
    public void Verify_ShouldReportUnsupportedAlgorithm()
    {
        using var rsa = RSA.Create(2048);
        var verifier = CreateVerifier(rsa);

        var result = verifier.Verify(
            CreatePayloadBytes(),
            new byte[] { 1, 2, 3 },
            "Ed25519",
            "test-key");

        Assert.Equal(LicenseSignatureVerificationStatus.UnsupportedAlgorithm, result.Status);
    }

    [Fact]
    public void Verify_ShouldReportMissingSignature()
    {
        using var rsa = RSA.Create(2048);
        var verifier = CreateVerifier(rsa);

        var result = verifier.Verify(
            CreatePayloadBytes(),
            Array.Empty<byte>(),
            LicenseSignatureAlgorithms.RsaPssSha256,
            "test-key");

        Assert.Equal(LicenseSignatureVerificationStatus.MissingSignature, result.Status);
    }

    [Fact]
    public void DefaultProvider_ShouldContainProductionPublicKey()
    {
        var provider = new LicensePublicKeyProvider();

        var found = provider.TryGetPublicKey(
            LicensePublicKeyProvider.ProductionKeyId,
            out var publicKeyBytes);

        Assert.True(found);
        Assert.NotEmpty(publicKeyBytes);
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out var bytesRead);
        Assert.Equal(publicKeyBytes.Length, bytesRead);
    }

    private static LicenseSignatureVerifier CreateVerifier(RSA rsa)
    {
        var publicKeyBase64 = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
        var provider = new LicensePublicKeyProvider(
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["test-key"] = publicKeyBase64
            });
        return new LicenseSignatureVerifier(provider);
    }

    private static byte[] CreatePayloadBytes()
    {
        var payload = new LicensePayload(
            LicenseId: "license-v1",
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            LicenseeName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            MaxActiveDeviceConnections: 3,
            ValidFromUtc: new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            ValidUntilUtc: new DateTime(2027, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            GraceDays: XdtBoxLicenseConstants.DefaultGraceDays,
            IssuedAtUtc: new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc),
            Issuer: XdtBoxLicenseConstants.DefaultIssuer,
            LicenseType: "Production",
            Notes: null);

        return LicensePayloadSerializer.SerializeToUtf8Bytes(payload);
    }
}
