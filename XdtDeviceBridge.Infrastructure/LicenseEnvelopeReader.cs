using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Infrastructure;

public sealed class LicenseEnvelopeReader
{
    private const string SupportedFormatVersion = "1";
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public LicenseEnvelopeReadResult ReadFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"License file not found: {filePath}", filePath);
        }

        var json = File.ReadAllText(filePath, Utf8NoBom);
        return ReadJson(json);
    }

    public LicenseEnvelopeReadResult ReadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedEnvelope,
                "License envelope JSON must not be empty.");
        }

        LicenseEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<LicenseEnvelope>(json, Options);
        }
        catch (JsonException ex)
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedEnvelope,
                $"License envelope JSON is invalid: {ex.Message}");
        }

        if (envelope is null)
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedEnvelope,
                "License envelope did not contain valid data.");
        }

        var envelopeIssues = envelope.Validate();
        if (envelopeIssues.Count > 0)
        {
            var status = string.IsNullOrWhiteSpace(envelope.SignatureBase64Url)
                ? LicenseSignatureVerificationStatus.MissingSignature
                : LicenseSignatureVerificationStatus.MalformedEnvelope;
            return Failed(status, envelopeIssues, envelope: envelope);
        }

        if (!string.Equals(envelope.FormatVersion, SupportedFormatVersion, StringComparison.Ordinal))
        {
            return Failed(
                LicenseSignatureVerificationStatus.UnsupportedFormatVersion,
                $"Unsupported license format version: {envelope.FormatVersion}.",
                envelope);
        }

        if (!LicenseBase64Url.TryDecode(envelope.PayloadBase64Url, out var payloadBytes))
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedEnvelope,
                "PayloadBase64Url is not valid Base64Url.",
                envelope);
        }

        if (!LicenseBase64Url.TryDecode(envelope.SignatureBase64Url, out var signatureBytes))
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedEnvelope,
                "SignatureBase64Url is not valid Base64Url.",
                envelope,
                payloadBytes: payloadBytes);
        }

        LicensePayload? payload;
        try
        {
            payload = LicensePayloadSerializer.Deserialize(payloadBytes);
        }
        catch (JsonException ex)
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedPayload,
                $"License payload JSON is invalid: {ex.Message}",
                envelope,
                payloadBytes: payloadBytes,
                signatureBytes: signatureBytes);
        }

        if (payload is null)
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedPayload,
                "License payload did not contain valid data.",
                envelope,
                payloadBytes: payloadBytes,
                signatureBytes: signatureBytes);
        }

        var payloadIssues = ValidatePayloadEnvelopeShape(payload);
        if (payloadIssues.Count > 0)
        {
            return Failed(
                LicenseSignatureVerificationStatus.MalformedPayload,
                payloadIssues,
                envelope,
                payload,
                payloadBytes,
                signatureBytes);
        }

        return new LicenseEnvelopeReadResult(
            Envelope: envelope,
            Payload: payload,
            PayloadBytes: payloadBytes,
            SignatureBytes: signatureBytes,
            Status: LicenseSignatureVerificationStatus.NotChecked,
            Messages: Array.Empty<string>());
    }

    private static LicenseEnvelopeReadResult Failed(
        LicenseSignatureVerificationStatus status,
        string message,
        LicenseEnvelope? envelope = null,
        LicensePayload? payload = null,
        byte[]? payloadBytes = null,
        byte[]? signatureBytes = null)
    {
        return Failed(
            status,
            new[] { message },
            envelope,
            payload,
            payloadBytes,
            signatureBytes);
    }

    private static LicenseEnvelopeReadResult Failed(
        LicenseSignatureVerificationStatus status,
        IReadOnlyList<string> messages,
        LicenseEnvelope? envelope = null,
        LicensePayload? payload = null,
        byte[]? payloadBytes = null,
        byte[]? signatureBytes = null)
    {
        return new LicenseEnvelopeReadResult(
            Envelope: envelope,
            Payload: payload,
            PayloadBytes: payloadBytes ?? Array.Empty<byte>(),
            SignatureBytes: signatureBytes ?? Array.Empty<byte>(),
            Status: status,
            Messages: messages);
    }

    private static IReadOnlyList<string> ValidatePayloadEnvelopeShape(LicensePayload payload)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(payload.LicenseId))
        {
            issues.Add("LicenseId must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(payload.ProductCode))
        {
            issues.Add("ProductCode must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(payload.LicenseeName))
        {
            issues.Add("LicenseeName must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(payload.InstallationId))
        {
            issues.Add("InstallationId must not be empty.");
        }

        if (payload.MaxActiveDeviceConnections < 0)
        {
            issues.Add("MaxActiveDeviceConnections must not be negative.");
        }

        if (payload.ValidFromUtc == default)
        {
            issues.Add("ValidFromUtc must not be default.");
        }

        if (payload.ValidUntilUtc == default)
        {
            issues.Add("ValidUntilUtc must not be default.");
        }

        if (payload.ValidUntilUtc < payload.ValidFromUtc)
        {
            issues.Add("ValidUntilUtc must not be before ValidFromUtc.");
        }

        if (payload.GraceDays < 0)
        {
            issues.Add("GraceDays must not be negative.");
        }

        if (payload.IssuedAtUtc == default)
        {
            issues.Add("IssuedAtUtc must not be default.");
        }

        if (string.IsNullOrWhiteSpace(payload.Issuer))
        {
            issues.Add("Issuer must not be empty.");
        }

        if (string.IsNullOrWhiteSpace(payload.LicenseType))
        {
            issues.Add("LicenseType must not be empty.");
        }

        return issues;
    }
}
