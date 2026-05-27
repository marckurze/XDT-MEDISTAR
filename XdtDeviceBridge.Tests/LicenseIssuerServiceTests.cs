using System.Security.Cryptography;
using XdtBox.LicenseIssuer;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseIssuerServiceTests
{
    private static readonly DateTime ValidFromUtc = new(2026, 5, 27, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ValidUntilUtc = new(2027, 5, 27, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateLicense_ShouldCreateSignedXdtboxLicenseWithExpectedPayload()
    {
        using var context = LicenseIssuerTestContext.Create();
        var options = context.CreateOptions();

        var result = context.Service.CreateLicense(options);

        Assert.True(File.Exists(options.OutputFile));
        Assert.Equal(XdtBoxLicenseConstants.ProductCode, result.Payload.ProductCode);
        Assert.Equal("installation-1", result.Payload.InstallationId);
        Assert.Equal("Praxis Muster", result.Payload.LicenseeName);
        Assert.Equal(3, result.Payload.MaxActiveDeviceConnections);
        Assert.Equal(XdtBoxLicenseConstants.DefaultGraceDays, result.Payload.GraceDays);
        Assert.StartsWith("XDTBOX-LIC-", result.Payload.LicenseId, StringComparison.Ordinal);
        Assert.Equal(LicenseSignatureAlgorithms.RsaPssSha256, result.Envelope.Algorithm);
    }

    [Fact]
    public void CreateLicense_ShouldProduceLicenseAcceptedByAppSignatureVerifier()
    {
        using var context = LicenseIssuerTestContext.Create();
        var result = context.Service.CreateLicense(context.CreateOptions());
        var readResult = new LicenseEnvelopeReader().ReadFile(result.OutputFile);
        var verifier = context.CreateVerifier();

        var verification = verifier.Verify(
            readResult.PayloadBytes,
            readResult.SignatureBytes,
            readResult.Envelope!.Algorithm,
            readResult.Envelope.KeyId);

        Assert.Equal(LicenseSignatureVerificationStatus.Valid, verification.Status);
    }

    [Fact]
    public void CreateLicense_ShouldMakeManipulatedPayloadInvalid()
    {
        using var context = LicenseIssuerTestContext.Create();
        var result = context.Service.CreateLicense(context.CreateOptions());
        var readResult = new LicenseEnvelopeReader().ReadFile(result.OutputFile);
        var payloadBytes = readResult.PayloadBytes.ToArray();
        payloadBytes[0] ^= 0x01;

        var verification = context.CreateVerifier().Verify(
            payloadBytes,
            readResult.SignatureBytes,
            readResult.Envelope!.Algorithm,
            readResult.Envelope.KeyId);

        Assert.Equal(LicenseSignatureVerificationStatus.Invalid, verification.Status);
    }

    [Fact]
    public void CreateLicense_ShouldBeEvaluatedAsDifferentInstallationWhenImportedElsewhere()
    {
        using var context = LicenseIssuerTestContext.Create();
        var result = context.Service.CreateLicense(context.CreateOptions());
        var readResult = new LicenseEnvelopeReader().ReadFile(result.OutputFile);
        var importService = new LicenseImportService(
            new LicenseEnvelopeReader(),
            context.CreateVerifier(),
            new LicenseV1PolicyEvaluator());
        var otherInstallation = new InstallationInfo(
            InstallationId: "other-installation",
            MachineName: "TEST",
            UserName: "tester",
            IsTerminalServer: false,
            CreatedAt: ValidFromUtc.AddDays(-1));

        var importResult = importService.Import(
            readResult,
            otherInstallation,
            activeDeviceConnectionCount: 1,
            nowUtc: ValidFromUtc.AddDays(1));

        Assert.Equal(LicenseV1PolicyStatus.InstallationMismatch, importResult.PolicyEvaluation?.Status);
        Assert.Equal("Diese Lizenz wurde für eine andere Installation ausgestellt. Bitte neue Lizenz anfordern.", importResult.UserMessage);
    }

    [Fact]
    public void CreateLicense_ShouldLoadInstallationIdFromRequestFile()
    {
        using var context = LicenseIssuerTestContext.Create();
        var requestFile = context.WriteRequest(CreateRequest("installation-from-request"));
        var options = context.CreateOptions(requestFile: requestFile, installationId: null);

        var result = context.Service.CreateLicense(options);

        Assert.Equal("installation-from-request", result.Payload.InstallationId);
    }

    [Fact]
    public void CreateLicense_ShouldRejectWrongProductCodeInRequestFile()
    {
        using var context = LicenseIssuerTestContext.Create();
        var requestFile = context.WriteRequest(CreateRequest("installation-1") with { ProductCode = "OTHER_PRODUCT" });
        var options = context.CreateOptions(requestFile: requestFile, installationId: null);

        var ex = Assert.Throws<LicenseIssuerException>(() => context.Service.CreateLicense(options));

        Assert.Contains("nicht fuer XDTBOX", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateLicense_ShouldRejectMissingPrivateKeyPath()
    {
        using var context = LicenseIssuerTestContext.Create();
        var options = context.CreateOptions(privateKeyPath: Path.Combine(context.DirectoryPath, "missing.pem"));

        var ex = Assert.Throws<LicenseIssuerException>(() => context.Service.CreateLicense(options));

        Assert.Contains("Private-Key-Datei nicht gefunden", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateLicense_ShouldRejectInvalidPrivateKey()
    {
        using var context = LicenseIssuerTestContext.Create();
        var invalidKeyFile = Path.Combine(context.DirectoryPath, "invalid.pem");
        File.WriteAllText(invalidKeyFile, "not a private key");
        var options = context.CreateOptions(privateKeyPath: invalidKeyFile);

        var ex = Assert.Throws<LicenseIssuerException>(() => context.Service.CreateLicense(options));

        Assert.Contains("Privater Schluessel konnte nicht geladen", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateLicense_ShouldRejectInvalidDateRange()
    {
        using var context = LicenseIssuerTestContext.Create();
        var options = context.CreateOptions(validFromUtc: ValidUntilUtc, validUntilUtc: ValidFromUtc);

        var ex = Assert.Throws<LicenseIssuerException>(() => context.Service.CreateLicense(options));

        Assert.Contains("ValidUntilUtc darf nicht vor ValidFromUtc liegen", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CreateLicense_ShouldRejectZeroDeviceConnections()
    {
        using var context = LicenseIssuerTestContext.Create();
        var options = context.CreateOptions(maxActiveDeviceConnections: 0);

        var ex = Assert.Throws<LicenseIssuerException>(() => context.Service.CreateLicense(options));

        Assert.Contains("MaxActiveDeviceConnections muss groesser als 0 sein", ex.Message, StringComparison.Ordinal);
    }

    private static LicenseRequest CreateRequest(string installationId)
    {
        return new LicenseRequest(
            RequestId: "request-1",
            InstallationId: installationId,
            MachineName: "TEST",
            UserName: "tester",
            IsTerminalServer: false,
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            AppVersion: "0.1.0",
            ActiveLicensedDeviceCount: 1,
            Devices: Array.Empty<LicenseRequestDevice>(),
            CreatedAt: ValidFromUtc.AddDays(-1));
    }

    private sealed class LicenseIssuerTestContext : IDisposable
    {
        private readonly RSA _rsa;

        private LicenseIssuerTestContext(string directoryPath, RSA rsa, string privateKeyPath)
        {
            DirectoryPath = directoryPath;
            _rsa = rsa;
            PrivateKeyPath = privateKeyPath;
            Service = new LicenseIssuerService();
        }

        public string DirectoryPath { get; }

        public string PrivateKeyPath { get; }

        public LicenseIssuerService Service { get; }

        public static LicenseIssuerTestContext Create()
        {
            var directory = Path.Combine(Path.GetTempPath(), $"xdtbox-license-issuer-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            var rsa = RSA.Create(2048);
            var privateKeyPath = Path.Combine(directory, "TEST_ONLY_private.pem");
            File.WriteAllText(privateKeyPath, rsa.ExportPkcs8PrivateKeyPem());
            return new LicenseIssuerTestContext(directory, rsa, privateKeyPath);
        }

        public LicenseIssuerOptions CreateOptions(
            string? requestFile = null,
            string? installationId = "installation-1",
            string? privateKeyPath = null,
            int maxActiveDeviceConnections = 3,
            DateTime? validFromUtc = null,
            DateTime? validUntilUtc = null)
        {
            return new LicenseIssuerOptions(
                RequestFile: requestFile,
                InstallationId: installationId,
                LicenseeName: "Praxis Muster",
                CustomerNumber: "K12345",
                MaxActiveDeviceConnections: maxActiveDeviceConnections,
                ValidFromUtc: validFromUtc ?? ValidFromUtc,
                ValidUntilUtc: validUntilUtc ?? ValidUntilUtc,
                GraceDays: XdtBoxLicenseConstants.DefaultGraceDays,
                LicenseType: "Production",
                Issuer: XdtBoxLicenseConstants.DefaultIssuer,
                ProductCode: XdtBoxLicenseConstants.ProductCode,
                Notes: "TEST ONLY",
                KeyId: "test-key",
                PrivateKeyPath: privateKeyPath ?? PrivateKeyPath,
                OutputFile: Path.Combine(DirectoryPath, "license.xdtboxlic"));
        }

        public string WriteRequest(LicenseRequest request)
        {
            var requestFile = Path.Combine(DirectoryPath, "license-request.json");
            new LicenseRequestFileRepository().Save(requestFile, request);
            return requestFile;
        }

        public LicenseSignatureVerifier CreateVerifier()
        {
            var provider = new LicensePublicKeyProvider(
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["test-key"] = Convert.ToBase64String(_rsa.ExportSubjectPublicKeyInfo())
                });
            return new LicenseSignatureVerifier(provider);
        }

        public void Dispose()
        {
            _rsa.Dispose();
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
        }
    }
}
