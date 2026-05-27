using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseV1PolicyEvaluatorTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly LicenseV1PolicyEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_ShouldAllowActiveDeviceConnectionsWithinMaxActiveDeviceConnections()
    {
        var result = _evaluator.Evaluate(
            CreatePayload(maxActiveDeviceConnections: 3),
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Valid,
            activeDeviceConnectionCount: 3,
            NowUtc);

        Assert.Equal(LicenseV1PolicyStatus.Valid, result.Status);
        Assert.True(result.CanStartDeviceConnections);
        Assert.False(result.IsWarningOnly);
    }

    [Fact]
    public void Evaluate_ShouldBlockInvalidSignatureWithRequiredMessage()
    {
        var result = _evaluator.Evaluate(
            CreatePayload(maxActiveDeviceConnections: 3),
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Invalid,
            activeDeviceConnectionCount: 1,
            NowUtc);

        Assert.Equal(LicenseV1PolicyStatus.InvalidSignature, result.Status);
        Assert.False(result.CanStartDeviceConnections);
        Assert.Equal(XdtBoxLicenseConstants.InvalidSignatureMessage, result.Message);
    }

    [Fact]
    public void Evaluate_ShouldWarnWithinGracePeriodWhenDeviceLimitIsExceeded()
    {
        var graceStartedAtUtc = NowUtc.AddDays(-2);

        var result = _evaluator.Evaluate(
            CreatePayload(maxActiveDeviceConnections: 1),
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Valid,
            activeDeviceConnectionCount: 2,
            NowUtc,
            graceStartedAtUtc);

        Assert.Equal(LicenseV1PolicyStatus.DeviceLimitExceededWithinGrace, result.Status);
        Assert.True(result.CanStartDeviceConnections);
        Assert.True(result.IsWarningOnly);
        Assert.Equal(graceStartedAtUtc.AddDays(XdtBoxLicenseConstants.DefaultGraceDays), result.GraceEndsAtUtc);
    }

    [Fact]
    public void Evaluate_ShouldBlockAfterGracePeriodWhenDeviceLimitIsExceeded()
    {
        var result = _evaluator.Evaluate(
            CreatePayload(maxActiveDeviceConnections: 1),
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Valid,
            activeDeviceConnectionCount: 2,
            NowUtc,
            gracePeriodStartedAtUtc: NowUtc.AddDays(-8));

        Assert.Equal(LicenseV1PolicyStatus.DeviceLimitExceededAfterGrace, result.Status);
        Assert.False(result.CanStartDeviceConnections);
        Assert.False(result.IsWarningOnly);
    }

    [Fact]
    public void Evaluate_ShouldWarnWithinGracePeriodWhenLicenseExpired()
    {
        var payload = CreatePayload(maxActiveDeviceConnections: 3) with
        {
            ValidUntilUtc = NowUtc.AddDays(-2)
        };

        var result = _evaluator.Evaluate(
            payload,
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Valid,
            activeDeviceConnectionCount: 1,
            NowUtc);

        Assert.Equal(LicenseV1PolicyStatus.ExpiredWithinGrace, result.Status);
        Assert.True(result.CanStartDeviceConnections);
        Assert.True(result.IsWarningOnly);
        Assert.Equal(payload.ValidUntilUtc.AddDays(XdtBoxLicenseConstants.DefaultGraceDays), result.GraceEndsAtUtc);
    }

    [Fact]
    public void Evaluate_ShouldBlockAfterGracePeriodWhenLicenseExpired()
    {
        var payload = CreatePayload(maxActiveDeviceConnections: 3) with
        {
            ValidUntilUtc = NowUtc.AddDays(-8)
        };

        var result = _evaluator.Evaluate(
            payload,
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Valid,
            activeDeviceConnectionCount: 1,
            NowUtc);

        Assert.Equal(LicenseV1PolicyStatus.ExpiredAfterGrace, result.Status);
        Assert.False(result.CanStartDeviceConnections);
    }

    [Fact]
    public void Evaluate_ShouldBlockHardwareMismatchBecauseInstallationIdLeads()
    {
        var result = _evaluator.Evaluate(
            CreatePayload(maxActiveDeviceConnections: 3) with { InstallationId = "other-installation" },
            CreateInstallationInfo(),
            LicenseSignatureVerificationStatus.Valid,
            activeDeviceConnectionCount: 1,
            NowUtc);

        Assert.Equal(LicenseV1PolicyStatus.InstallationMismatch, result.Status);
        Assert.False(result.CanStartDeviceConnections);
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
