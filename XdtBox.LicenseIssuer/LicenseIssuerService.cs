using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtBox.LicenseIssuer;

public sealed class LicenseIssuerService
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions EnvelopeJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly LicenseRequestFileRepository _requestFileRepository;

    public LicenseIssuerService()
        : this(new LicenseRequestFileRepository())
    {
    }

    public LicenseIssuerService(LicenseRequestFileRepository requestFileRepository)
    {
        _requestFileRepository = requestFileRepository ?? throw new ArgumentNullException(nameof(requestFileRepository));
    }

    public LicenseIssuerResult CreateLicense(LicenseIssuerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ValidateOptions(options);

        var request = LoadAndValidateRequest(options.RequestFile);
        var installationId = ResolveInstallationId(options, request);
        var productCode = ResolveProductCode(options, request);

        var payload = new LicensePayload(
            LicenseId: CreateLicenseId(options.ValidFromUtc),
            ProductCode: productCode,
            LicenseeName: options.LicenseeName.Trim(),
            CustomerNumber: string.IsNullOrWhiteSpace(options.CustomerNumber) ? null : options.CustomerNumber.Trim(),
            InstallationId: installationId,
            MaxActiveDeviceConnections: options.MaxActiveDeviceConnections,
            ValidFromUtc: options.ValidFromUtc,
            ValidUntilUtc: options.ValidUntilUtc,
            GraceDays: options.GraceDays,
            IssuedAtUtc: DateTime.UtcNow,
            Issuer: options.Issuer.Trim(),
            LicenseType: options.LicenseType.Trim(),
            Notes: string.IsNullOrWhiteSpace(options.Notes) ? null : options.Notes.Trim());

        var payloadIssues = payload.Validate();
        if (payloadIssues.Count > 0)
        {
            throw new LicenseIssuerException("LicensePayload ist ungueltig: " + string.Join("; ", payloadIssues));
        }

        var payloadBytes = LicensePayloadSerializer.SerializeToUtf8Bytes(payload);
        var signatureBytes = SignPayload(payloadBytes, options.PrivateKeyPath);
        var envelope = new LicenseEnvelope(
            PayloadBase64Url: LicenseBase64Url.Encode(payloadBytes),
            SignatureBase64Url: LicenseBase64Url.Encode(signatureBytes),
            Algorithm: LicenseSignatureAlgorithms.RsaPssSha256,
            KeyId: options.KeyId.Trim(),
            FormatVersion: "1");

        WriteEnvelopeAtomically(options.OutputFile, envelope);

        return new LicenseIssuerResult(payload, envelope, Path.GetFullPath(options.OutputFile));
    }

    private LicenseRequest? LoadAndValidateRequest(string? requestFile)
    {
        if (string.IsNullOrWhiteSpace(requestFile))
        {
            return null;
        }

        LicenseRequest request;
        try
        {
            request = _requestFileRepository.Load(requestFile);
        }
        catch (Exception ex)
        {
            throw new LicenseIssuerException($"Lizenzanforderung konnte nicht gelesen werden: {ex.Message}", ex);
        }

        var requestIssues = request.Validate();
        if (requestIssues.Count > 0)
        {
            throw new LicenseIssuerException("Lizenzanforderung ist ungueltig: " + string.Join("; ", requestIssues));
        }

        if (!string.Equals(request.ProductCode, XdtBoxLicenseConstants.ProductCode, StringComparison.Ordinal))
        {
            throw new LicenseIssuerException($"Lizenzanforderung ist nicht fuer {XdtBoxLicenseConstants.ProductCode} ausgestellt.");
        }

        return request;
    }

    private static string ResolveInstallationId(LicenseIssuerOptions options, LicenseRequest? request)
    {
        var installationId = request?.InstallationId ?? options.InstallationId;
        if (string.IsNullOrWhiteSpace(installationId))
        {
            throw new LicenseIssuerException("InstallationId fehlt. Bitte --request oder --installation-id angeben.");
        }

        return installationId.Trim();
    }

    private static string ResolveProductCode(LicenseIssuerOptions options, LicenseRequest? request)
    {
        var productCode = request?.ProductCode ?? options.ProductCode;
        if (!string.Equals(productCode, XdtBoxLicenseConstants.ProductCode, StringComparison.Ordinal))
        {
            throw new LicenseIssuerException($"ProductCode muss {XdtBoxLicenseConstants.ProductCode} sein.");
        }

        return productCode;
    }

    private static void ValidateOptions(LicenseIssuerOptions options)
    {
        var issues = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.RequestFile) && !string.IsNullOrWhiteSpace(options.InstallationId))
        {
            issues.Add("--installation-id darf nicht zusammen mit --request verwendet werden.");
        }

        if (string.IsNullOrWhiteSpace(options.LicenseeName))
        {
            issues.Add("Lizenznehmer/Praxisname fehlt.");
        }

        if (options.MaxActiveDeviceConnections <= 0)
        {
            issues.Add("MaxActiveDeviceConnections muss groesser als 0 sein.");
        }

        if (options.ValidFromUtc == default)
        {
            issues.Add("ValidFromUtc fehlt.");
        }

        if (options.ValidUntilUtc == default)
        {
            issues.Add("ValidUntilUtc fehlt.");
        }

        if (options.ValidUntilUtc < options.ValidFromUtc)
        {
            issues.Add("ValidUntilUtc darf nicht vor ValidFromUtc liegen.");
        }

        if (options.GraceDays < 0)
        {
            issues.Add("GraceDays darf nicht negativ sein.");
        }

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            issues.Add("Issuer fehlt.");
        }

        if (string.IsNullOrWhiteSpace(options.LicenseType))
        {
            issues.Add("LicenseType fehlt.");
        }

        if (string.IsNullOrWhiteSpace(options.KeyId))
        {
            issues.Add("KeyId fehlt.");
        }

        if (string.IsNullOrWhiteSpace(options.PrivateKeyPath))
        {
            issues.Add("Privater Schluesselpfad fehlt.");
        }
        else if (!File.Exists(options.PrivateKeyPath))
        {
            issues.Add($"Private-Key-Datei nicht gefunden: {options.PrivateKeyPath}");
        }

        if (string.IsNullOrWhiteSpace(options.OutputFile))
        {
            issues.Add("Ausgabepfad fehlt.");
        }

        if (issues.Count > 0)
        {
            throw new LicenseIssuerException(string.Join(Environment.NewLine, issues));
        }
    }

    private static byte[] SignPayload(byte[] payloadBytes, string privateKeyPath)
    {
        try
        {
            var pem = File.ReadAllText(privateKeyPath, Utf8NoBom);
            using var rsa = RSA.Create();
            rsa.ImportFromPem(pem);
            return rsa.SignData(payloadBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }
        catch (Exception ex)
        {
            throw new LicenseIssuerException("Privater Schluessel konnte nicht geladen oder zum Signieren verwendet werden.", ex);
        }
    }

    private static void WriteEnvelopeAtomically(string outputFile, LicenseEnvelope envelope)
    {
        var fullOutputFile = Path.GetFullPath(outputFile);
        var directory = Path.GetDirectoryName(fullOutputFile);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempFile = fullOutputFile + ".tmp";
        try
        {
            var json = JsonSerializer.Serialize(envelope, EnvelopeJsonOptions);
            File.WriteAllText(tempFile, json, Utf8NoBom);
            File.Move(tempFile, fullOutputFile, overwrite: true);
        }
        catch
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            throw;
        }
    }

    private static string CreateLicenseId(DateTime validFromUtc)
    {
        return $"XDTBOX-LIC-{validFromUtc:yyyyMMdd}-{Guid.NewGuid():N}"[..33].ToUpperInvariant();
    }
}
