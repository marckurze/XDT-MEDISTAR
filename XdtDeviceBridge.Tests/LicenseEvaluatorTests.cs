using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseEvaluatorTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly LicenseEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_ShouldReturnNotLicensedForNullLicense()
    {
        var result = _evaluator.Evaluate(null, CreateInstallationInfo(), activeLicensedDeviceCount: 1, NowUtc);

        Assert.Equal(LicenseStatus.NotLicensed, result.Status);
        Assert.False(result.CanProcessFiles);
        Assert.Contains("Keine Lizenz vorhanden.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldReturnInvalidForWrongInstallationId()
    {
        var license = CreateLicenseInfo() with { InstallationId = "other-installation" };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 1, NowUtc);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        Assert.False(result.CanProcessFiles);
    }

    [Fact]
    public void Evaluate_ShouldReturnInvalidBeforeValidFrom()
    {
        var license = CreateLicenseInfo() with
        {
            ValidFrom = NowUtc.AddDays(1),
            ValidUntil = NowUtc.AddDays(30)
        };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 1, NowUtc);

        Assert.Equal(LicenseStatus.Invalid, result.Status);
        Assert.False(result.CanProcessFiles);
    }

    [Fact]
    public void Evaluate_ShouldReturnExpiredAfterValidUntil()
    {
        var license = CreateLicenseInfo() with
        {
            ValidFrom = NowUtc.AddDays(-30),
            ValidUntil = NowUtc.AddDays(-1)
        };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 1, NowUtc);

        Assert.Equal(LicenseStatus.Expired, result.Status);
        Assert.False(result.CanProcessFiles);
        Assert.Contains("Lizenz ist abgelaufen.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldReturnDeviceLimitExceededWhenDeviceCountIsTooHigh()
    {
        var license = CreateLicenseInfo() with { LicensedDeviceCount = 2 };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 3, NowUtc);

        Assert.Equal(LicenseStatus.DeviceLimitExceeded, result.Status);
        Assert.False(result.CanProcessFiles);
        Assert.Contains("Geraeteanzahl ueberschreitet lizenzierte Anzahl.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldReturnTrialActiveForValidTrialLicense()
    {
        var license = CreateLicenseInfo() with { LicenseType = LicenseType.Trial };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 1, NowUtc);

        Assert.Equal(LicenseStatus.TrialActive, result.Status);
        Assert.True(result.CanProcessFiles);
        Assert.Contains("Testphase aktiv.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldReturnActiveForValidMonthlyLicense()
    {
        var license = CreateLicenseInfo() with { LicenseType = LicenseType.Monthly };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 1, NowUtc);

        Assert.Equal(LicenseStatus.Active, result.Status);
        Assert.True(result.CanProcessFiles);
        Assert.Contains("Lizenz aktiv.", result.Messages);
    }

    [Fact]
    public void Evaluate_ShouldAllowDeviceCountEqualToLicensedDeviceCount()
    {
        var license = CreateLicenseInfo() with { LicensedDeviceCount = 3 };

        var result = _evaluator.Evaluate(license, CreateInstallationInfo(), activeLicensedDeviceCount: 3, NowUtc);

        Assert.Equal(LicenseStatus.Active, result.Status);
        Assert.True(result.CanProcessFiles);
    }

    private static LicenseInfo CreateLicenseInfo()
    {
        return new LicenseInfo(
            LicenseId: "license-1",
            CustomerName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            LicensedDeviceCount: 3,
            ValidFrom: NowUtc.AddDays(-30),
            ValidUntil: NowUtc.AddDays(30),
            LicenseType: LicenseType.Monthly,
            ProductCode: "XDT-DEVICE-BRIDGE",
            MinimumAppVersion: "1.0.0",
            IssuedAt: NowUtc.AddDays(-31),
            Signature: "signed-license");
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
}
