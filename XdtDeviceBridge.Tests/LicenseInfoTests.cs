using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseInfoTests
{
    [Fact]
    public void Validate_ShouldAcceptValidTrialLicense()
    {
        var licenseInfo = CreateValidLicenseInfo() with
        {
            LicenseType = LicenseType.Trial,
            Signature = ""
        };

        var issues = LicenseInfoValidator.Validate(licenseInfo);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldAcceptValidMonthlyLicenseWithSignature()
    {
        var licenseInfo = CreateValidLicenseInfo() with
        {
            LicenseType = LicenseType.Monthly,
            Signature = "signed-license"
        };

        var issues = LicenseInfoValidator.Validate(licenseInfo);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldReportProductiveLicenseWithoutSignature()
    {
        var licenseInfo = CreateValidLicenseInfo() with
        {
            LicenseType = LicenseType.Annual,
            Signature = ""
        };

        var issues = LicenseInfoValidator.Validate(licenseInfo);

        Assert.Contains("Signature must not be empty for productive licenses.", issues);
    }

    [Fact]
    public void Validate_ShouldReportNegativeDeviceCount()
    {
        var licenseInfo = CreateValidLicenseInfo() with { LicensedDeviceCount = -1 };

        var issues = LicenseInfoValidator.Validate(licenseInfo);

        Assert.Contains("LicensedDeviceCount must not be negative.", issues);
    }

    [Fact]
    public void Validate_ShouldReportValidUntilBeforeValidFrom()
    {
        var licenseInfo = CreateValidLicenseInfo() with
        {
            ValidFrom = new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            ValidUntil = new DateTime(2026, 5, 2, 0, 0, 0, DateTimeKind.Utc)
        };

        var issues = LicenseInfoValidator.Validate(licenseInfo);

        Assert.Contains("ValidUntil must not be before ValidFrom.", issues);
    }

    [Fact]
    public void Validate_ShouldReportEmptyInstallationId()
    {
        var licenseInfo = CreateValidLicenseInfo() with { InstallationId = " " };

        var issues = LicenseInfoValidator.Validate(licenseInfo);

        Assert.Contains("InstallationId must not be empty.", issues);
    }

    [Fact]
    public void LicenseEvaluationResult_ShouldAllowProcessing()
    {
        var result = new LicenseEvaluationResult(
            Status: LicenseStatus.Active,
            ActiveLicensedDeviceCount: 2,
            LicensedDeviceCount: 3,
            ValidUntil: new DateTime(2027, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            Messages: Array.Empty<string>(),
            CanProcessFiles: true);

        Assert.True(result.CanProcessFiles);
        Assert.Equal(LicenseStatus.Active, result.Status);
    }

    [Fact]
    public void LicenseEvaluationResult_ShouldBlockProcessing()
    {
        var result = new LicenseEvaluationResult(
            Status: LicenseStatus.DeviceLimitExceeded,
            ActiveLicensedDeviceCount: 4,
            LicensedDeviceCount: 3,
            ValidUntil: new DateTime(2027, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            Messages: new[] { "Device limit exceeded." },
            CanProcessFiles: false);

        Assert.False(result.CanProcessFiles);
        Assert.Equal(LicenseStatus.DeviceLimitExceeded, result.Status);
    }

    private static LicenseInfo CreateValidLicenseInfo()
    {
        return new LicenseInfo(
            LicenseId: "license-1",
            CustomerName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            LicensedDeviceCount: 3,
            ValidFrom: new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            ValidUntil: new DateTime(2027, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            LicenseType: LicenseType.Monthly,
            ProductCode: "XDT-DEVICE-BRIDGE",
            MinimumAppVersion: "1.0.0",
            IssuedAt: new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc),
            Signature: "signed-license");
    }
}
