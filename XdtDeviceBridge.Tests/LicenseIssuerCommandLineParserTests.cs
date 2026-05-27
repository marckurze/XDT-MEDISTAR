using XdtBox.LicenseIssuer;
using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseIssuerCommandLineParserTests
{
    private readonly LicenseIssuerCommandLineParser _parser = new();

    [Fact]
    public void Parse_ShouldUseDefaultsForOptionalFields()
    {
        var result = _parser.Parse(new[]
        {
            "--installation-id", "installation-1",
            "--licensee", "Praxis Muster",
            "--max-active-device-connections", "3",
            "--valid-from", "2026-05-27",
            "--valid-until", "2027-05-27",
            "--key-id", "xdtbox-prod-2026-01",
            "--private-key", @"C:\keys\xdtbox_private.pem",
            "--out", @"C:\licenses\praxis.xdtboxlic"
        });

        Assert.Empty(result.Errors);
        Assert.NotNull(result.Options);
        Assert.Equal(XdtBoxLicenseConstants.ProductCode, result.Options!.ProductCode);
        Assert.Equal(XdtBoxLicenseConstants.DefaultIssuer, result.Options.Issuer);
        Assert.Equal(XdtBoxLicenseConstants.DefaultGraceDays, result.Options.GraceDays);
        Assert.Equal("Production", result.Options.LicenseType);
    }

    [Fact]
    public void Parse_ShouldRejectRequestAndInstallationIdTogether()
    {
        var result = _parser.Parse(new[]
        {
            "--request", @"C:\requests\license-request.json",
            "--installation-id", "installation-1",
            "--licensee", "Praxis Muster",
            "--max-active-device-connections", "3",
            "--valid-from", "2026-05-27",
            "--valid-until", "2027-05-27",
            "--key-id", "xdtbox-prod-2026-01",
            "--private-key", @"C:\keys\xdtbox_private.pem",
            "--out", @"C:\licenses\praxis.xdtboxlic"
        });

        Assert.Contains(result.Errors, error => error.Contains("--installation-id darf nicht zusammen mit --request", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void Parse_ShouldRejectInvalidMaxActiveDeviceConnections(string value)
    {
        var result = _parser.Parse(new[]
        {
            "--installation-id", "installation-1",
            "--licensee", "Praxis Muster",
            "--max-active-device-connections", value,
            "--valid-from", "2026-05-27",
            "--valid-until", "2027-05-27",
            "--key-id", "xdtbox-prod-2026-01",
            "--private-key", @"C:\keys\xdtbox_private.pem",
            "--out", @"C:\licenses\praxis.xdtboxlic"
        });

        Assert.Contains(result.Errors, error => error.Contains("--max-active-device-connections muss groesser als 0", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_ShouldRejectMissingInstallationSource()
    {
        var result = _parser.Parse(new[]
        {
            "--licensee", "Praxis Muster",
            "--max-active-device-connections", "3",
            "--valid-from", "2026-05-27",
            "--valid-until", "2027-05-27",
            "--key-id", "xdtbox-prod-2026-01",
            "--private-key", @"C:\keys\xdtbox_private.pem",
            "--out", @"C:\licenses\praxis.xdtboxlic"
        });

        Assert.Contains(result.Errors, error => error.Contains("Entweder --request oder --installation-id", StringComparison.Ordinal));
    }
}
