using System.Security.Cryptography;
using System.Text.Json;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseImportServiceTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Import_ShouldAcceptValidSignedXdtboxLicense()
    {
        using var context = SignedLicenseContext.Create(CreatePayload());

        var result = context.Import(activeDeviceConnectionCount: 2);

        Assert.True(result.HasVerifiedSignature);
        Assert.True(result.CanPersistLicenseFile);
        Assert.Equal("Lizenz wurde erfolgreich geprüft und importiert.", result.UserMessage);
        Assert.Equal(LicenseV1PolicyStatus.Valid, result.PolicyEvaluation?.Status);
    }

    [Fact]
    public void Import_ShouldRejectInvalidSignatureWithRequiredMessage()
    {
        using var context = SignedLicenseContext.Create(CreatePayload(), mutateSignature: true);

        var result = context.Import(activeDeviceConnectionCount: 1);

        Assert.False(result.CanPersistLicenseFile);
        Assert.Equal(LicenseSignatureVerificationStatus.Invalid, result.SignatureStatus);
        Assert.Equal(XdtBoxLicenseConstants.InvalidSignatureMessage, result.UserMessage);
    }

    [Fact]
    public void Import_ShouldRejectDifferentInstallationIdWithRequiredMessage()
    {
        using var context = SignedLicenseContext.Create(CreatePayload() with { InstallationId = "other-installation" });

        var result = context.Import(activeDeviceConnectionCount: 1);

        Assert.True(result.HasVerifiedSignature);
        Assert.False(result.CanPersistLicenseFile);
        Assert.Equal(LicenseV1PolicyStatus.InstallationMismatch, result.PolicyEvaluation?.Status);
        Assert.Equal("Diese Lizenz wurde für eine andere Installation ausgestellt. Bitte neue Lizenz anfordern.", result.UserMessage);
    }

    [Fact]
    public void Import_ShouldRejectDifferentProductCodeWithRequiredMessage()
    {
        using var context = SignedLicenseContext.Create(CreatePayload() with { ProductCode = "OTHER_PRODUCT" });

        var result = context.Import(activeDeviceConnectionCount: 1);

        Assert.True(result.HasVerifiedSignature);
        Assert.False(result.CanPersistLicenseFile);
        Assert.Equal(LicenseV1PolicyStatus.WrongProduct, result.PolicyEvaluation?.Status);
        Assert.Equal("Diese Lizenz ist nicht für XDTBox ausgestellt.", result.UserMessage);
    }

    [Fact]
    public void Import_ShouldReportExpiredLicenseWithinGracePeriod()
    {
        using var context = SignedLicenseContext.Create(CreatePayload() with { ValidUntilUtc = NowUtc.AddDays(-2) });

        var result = context.Import(activeDeviceConnectionCount: 1);

        Assert.True(result.HasVerifiedSignature);
        Assert.Equal(LicenseV1PolicyStatus.ExpiredWithinGrace, result.PolicyEvaluation?.Status);
        Assert.Contains("Lizenz ist abgelaufen. Karenzzeit aktiv bis", result.UserMessage);
    }

    [Fact]
    public void Import_ShouldReportExpiredLicenseAfterGracePeriod()
    {
        using var context = SignedLicenseContext.Create(CreatePayload() with { ValidUntilUtc = NowUtc.AddDays(-10) });

        var result = context.Import(activeDeviceConnectionCount: 1);

        Assert.True(result.HasVerifiedSignature);
        Assert.Equal(LicenseV1PolicyStatus.ExpiredAfterGrace, result.PolicyEvaluation?.Status);
        Assert.Equal("Lizenz ist abgelaufen.", result.UserMessage);
    }

    [Fact]
    public void Import_ShouldKeepEvaluatingMaxActiveDeviceConnections()
    {
        using var context = SignedLicenseContext.Create(CreatePayload(maxActiveDeviceConnections: 1));

        var result = context.Import(activeDeviceConnectionCount: 2);

        Assert.True(result.HasVerifiedSignature);
        Assert.Equal(LicenseV1PolicyStatus.DeviceLimitExceededAfterGrace, result.PolicyEvaluation?.Status);
        Assert.Contains("Anzahl aktiver Geräteanbindungen überschreitet die Lizenz", result.UserMessage);
    }

    [Fact]
    public void ImportFromFile_ShouldReadXdtboxlicFile()
    {
        using var context = SignedLicenseContext.Create(CreatePayload());
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xdtboxlic");
        try
        {
            File.WriteAllText(filePath, context.EnvelopeJson);

            var result = context.Service.ImportFromFile(
                filePath,
                CreateInstallationInfo(),
                activeDeviceConnectionCount: 1,
                NowUtc);

            Assert.True(result.HasVerifiedSignature);
            Assert.Equal("Lizenz wurde erfolgreich geprüft und importiert.", result.UserMessage);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    private static LicensePayload CreatePayload(int maxActiveDeviceConnections = 3)
    {
        return new LicensePayload(
            LicenseId: "license-v1",
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            LicenseeName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            MaxActiveDeviceConnections: maxActiveDeviceConnections,
            ValidFromUtc: NowUtc.AddDays(-30),
            ValidUntilUtc: NowUtc.AddDays(30),
            GraceDays: XdtBoxLicenseConstants.DefaultGraceDays,
            IssuedAtUtc: NowUtc.AddDays(-31),
            Issuer: XdtBoxLicenseConstants.DefaultIssuer,
            LicenseType: "Production",
            Notes: null);
    }

    private static InstallationInfo CreateInstallationInfo()
    {
        return new InstallationInfo(
            InstallationId: "installation-1",
            MachineName: "TEST-MACHINE",
            UserName: "test-user",
            IsTerminalServer: false,
            CreatedAt: NowUtc.AddDays(-31));
    }

    private sealed class SignedLicenseContext : IDisposable
    {
        private readonly RSA _rsa;

        private SignedLicenseContext(RSA rsa, LicenseImportService service, string envelopeJson)
        {
            _rsa = rsa;
            Service = service;
            EnvelopeJson = envelopeJson;
        }

        public LicenseImportService Service { get; }

        public string EnvelopeJson { get; }

        public static SignedLicenseContext Create(LicensePayload payload, bool mutateSignature = false)
        {
            var rsa = RSA.Create(2048);
            var payloadBytes = LicensePayloadSerializer.SerializeToUtf8Bytes(payload);
            var signatureBytes = rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
            if (mutateSignature)
            {
                signatureBytes[0] ^= 0x01;
            }

            var envelope = new LicenseEnvelope(
                PayloadBase64Url: LicenseBase64Url.Encode(payloadBytes),
                SignatureBase64Url: LicenseBase64Url.Encode(signatureBytes),
                Algorithm: LicenseSignatureAlgorithms.RsaPssSha256,
                KeyId: "test-key",
                FormatVersion: "1");
            var provider = new LicensePublicKeyProvider(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["test-key"] = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo())
                });
            var service = new LicenseImportService(
                new LicenseEnvelopeReader(),
                new LicenseSignatureVerifier(provider),
                new LicenseV1PolicyEvaluator());

            return new SignedLicenseContext(rsa, service, JsonSerializer.Serialize(envelope));
        }

        public LicenseImportResult Import(int activeDeviceConnectionCount)
        {
            var readResult = new LicenseEnvelopeReader().ReadJson(EnvelopeJson);
            return Service.Import(
                readResult,
                CreateInstallationInfo(),
                activeDeviceConnectionCount,
                NowUtc);
        }

        public void Dispose()
        {
            _rsa.Dispose();
        }
    }
}
