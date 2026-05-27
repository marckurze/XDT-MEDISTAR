using System.Text;
using System.Text.Json;
using XdtDeviceBridge.Core;
using XdtDeviceBridge.Infrastructure;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseEnvelopeReaderTests
{
    private readonly LicenseEnvelopeReader _reader = new();

    [Fact]
    public void ReadJson_ShouldReadValidEnvelopeWithoutCheckingSignature()
    {
        var payload = CreatePayload();
        var envelope = CreateEnvelope(payload);

        var result = _reader.ReadJson(JsonSerializer.Serialize(envelope));

        Assert.True(result.Success);
        Assert.Equal(LicenseSignatureVerificationStatus.NotChecked, result.Status);
        Assert.Equal(payload.LicenseId, result.Payload?.LicenseId);
        Assert.NotEmpty(result.PayloadBytes);
        Assert.NotEmpty(result.SignatureBytes);
    }

    [Fact]
    public void ReadJson_ShouldRejectMissingPayload()
    {
        var envelope = CreateEnvelope(CreatePayload()) with { PayloadBase64Url = "" };

        var result = _reader.ReadJson(JsonSerializer.Serialize(envelope));

        Assert.False(result.Success);
        Assert.Equal(LicenseSignatureVerificationStatus.MalformedEnvelope, result.Status);
    }

    [Fact]
    public void ReadJson_ShouldRejectMissingSignature()
    {
        var envelope = CreateEnvelope(CreatePayload()) with { SignatureBase64Url = "" };

        var result = _reader.ReadJson(JsonSerializer.Serialize(envelope));

        Assert.False(result.Success);
        Assert.Equal(LicenseSignatureVerificationStatus.MissingSignature, result.Status);
    }

    [Fact]
    public void ReadJson_ShouldRejectInvalidBase64UrlPayload()
    {
        var envelope = CreateEnvelope(CreatePayload()) with { PayloadBase64Url = "%%%not-base64%%%" };

        var result = _reader.ReadJson(JsonSerializer.Serialize(envelope));

        Assert.False(result.Success);
        Assert.Equal(LicenseSignatureVerificationStatus.MalformedEnvelope, result.Status);
    }

    [Fact]
    public void ReadJson_ShouldRejectBrokenPayloadJson()
    {
        var envelope = CreateEnvelope(CreatePayload()) with
        {
            PayloadBase64Url = LicenseBase64Url.Encode(Encoding.UTF8.GetBytes("{ broken json"))
        };

        var result = _reader.ReadJson(JsonSerializer.Serialize(envelope));

        Assert.False(result.Success);
        Assert.Equal(LicenseSignatureVerificationStatus.MalformedPayload, result.Status);
    }

    [Fact]
    public void ReadJson_ShouldRejectUnsupportedFormatVersion()
    {
        var envelope = CreateEnvelope(CreatePayload()) with { FormatVersion = "99" };

        var result = _reader.ReadJson(JsonSerializer.Serialize(envelope));

        Assert.False(result.Success);
        Assert.Equal(LicenseSignatureVerificationStatus.UnsupportedFormatVersion, result.Status);
    }

    private static LicenseEnvelope CreateEnvelope(LicensePayload payload)
    {
        return new LicenseEnvelope(
            PayloadBase64Url: LicenseBase64Url.Encode(LicensePayloadSerializer.SerializeToUtf8Bytes(payload)),
            SignatureBase64Url: LicenseBase64Url.Encode(new byte[] { 1, 2, 3, 4 }),
            Algorithm: LicenseSignatureAlgorithms.RsaPssSha256,
            KeyId: "test-key",
            FormatVersion: "1");
    }

    private static LicensePayload CreatePayload()
    {
        var now = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        return new LicensePayload(
            LicenseId: "license-v1",
            ProductCode: XdtBoxLicenseConstants.ProductCode,
            LicenseeName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            MaxActiveDeviceConnections: 3,
            ValidFromUtc: now.AddDays(-1),
            ValidUntilUtc: now.AddDays(30),
            GraceDays: XdtBoxLicenseConstants.DefaultGraceDays,
            IssuedAtUtc: now.AddDays(-2),
            Issuer: XdtBoxLicenseConstants.DefaultIssuer,
            LicenseType: "Production",
            Notes: null);
    }
}
