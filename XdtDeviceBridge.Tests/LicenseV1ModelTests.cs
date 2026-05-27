using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseV1ModelTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void LicensePayload_ShouldUseXdtBoxProductCodeAndMaxActiveDeviceConnections()
    {
        var payload = CreatePayload(maxActiveDeviceConnections: 3);

        Assert.Equal(XdtBoxLicenseConstants.ProductCode, payload.ProductCode);
        Assert.Equal(3, payload.MaxActiveDeviceConnections);
        Assert.Equal(XdtBoxLicenseConstants.DefaultGraceDays, payload.GraceDays);
        Assert.Empty(payload.Validate());
    }

    [Fact]
    public void LicensePayload_ShouldRejectWrongProductCode()
    {
        var payload = CreatePayload(maxActiveDeviceConnections: 3) with
        {
            ProductCode = "XDT_DEVICE_BRIDGE"
        };

        var issues = payload.Validate();

        Assert.Contains($"ProductCode must be {XdtBoxLicenseConstants.ProductCode}.", issues);
    }

    [Fact]
    public void LicensePayload_ShouldRejectNegativeMaxActiveDeviceConnections()
    {
        var payload = CreatePayload(maxActiveDeviceConnections: -1);

        var issues = payload.Validate();

        Assert.Contains("MaxActiveDeviceConnections must not be negative.", issues);
    }

    [Fact]
    public void LicenseEnvelope_ShouldRequireSignedPayloadFields()
    {
        var envelope = new LicenseEnvelope(
            PayloadBase64Url: string.Empty,
            SignatureBase64Url: string.Empty,
            Algorithm: string.Empty,
            KeyId: string.Empty,
            FormatVersion: string.Empty);

        var issues = envelope.Validate();

        Assert.Contains("PayloadBase64Url must not be empty.", issues);
        Assert.Contains("SignatureBase64Url must not be empty.", issues);
        Assert.Contains("Algorithm must not be empty.", issues);
        Assert.Contains("KeyId must not be empty.", issues);
        Assert.Contains("FormatVersion must not be empty.", issues);
    }

    [Fact]
    public void LicensePayload_ShouldNotModelSeparateModuleLicenses()
    {
        Assert.Null(typeof(LicensePayload).GetProperty("Modules"));
        Assert.Null(typeof(LicensePayload).GetProperty("AllowedDeviceProfileIds"));
    }

    [Fact]
    public void CreateSuccessfulLicenseImportMessage_ShouldUseSingularDeviceText()
    {
        var message = XdtBoxLicenseConstants.CreateSuccessfulLicenseImportMessage(1);

        Assert.Equal("Wir haben 1 Gerät für Sie lizenziert. Vielen Dank. Ihr XDTBox Team.", message);
    }

    [Fact]
    public void CreateSuccessfulLicenseImportMessage_ShouldUsePluralDeviceText()
    {
        var message = XdtBoxLicenseConstants.CreateSuccessfulLicenseImportMessage(14);

        Assert.Equal("Wir haben 14 Geräte für Sie lizenziert. Vielen Dank. Ihr XDTBox Team.", message);
    }

    private static LicensePayload CreatePayload(int maxActiveDeviceConnections)
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
}
