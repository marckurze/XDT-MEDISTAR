using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicensedDeviceStateEvaluatorTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly LicensedDeviceStateEvaluator _evaluator = new();

    [Fact]
    public void Evaluate_ShouldNotCountInactiveLicenseRequiredProfile()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Inactive Interface", isActive: false, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        Assert.DoesNotContain(states, state => IsCountedLicensedInterface(state));
        var state = Assert.Single(states);
        Assert.False(state.IsCoveredByLicense);
        Assert.Equal("Lizenzpflichtig, aber nicht aktiv - zählt aktuell nicht.", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldNotCountActiveProfileThatIsNotLicenseRequired()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Free Interface", isActive: true, isLicenseRequired: false) },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        Assert.DoesNotContain(states, state => IsCountedLicensedInterface(state));
        var state = Assert.Single(states);
        Assert.False(state.IsCoveredByLicense);
        Assert.Contains("Nicht lizenzpflichtig", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldCountActiveLicenseRequiredProfile()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Licensed Interface", isActive: true, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        Assert.Single(states, state => IsCountedLicensedInterface(state));
        var state = Assert.Single(states);
        Assert.True(state.IsCoveredByLicense);
        Assert.Contains("gedeckt", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldMarkActiveLicenseRequiredProfileAsUncoveredWithoutLicense()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Licensed Interface", isActive: true, isLicenseRequired: true) },
            license: null,
            NowUtc);

        var state = Assert.Single(states);
        Assert.True(IsCountedLicensedInterface(state));
        Assert.False(state.IsCoveredByLicense);
        Assert.Contains("keine Lizenz", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldCoverTwoActiveProfilesWithTwoLicensedDevices()
    {
        var states = _evaluator.Evaluate(
            new[]
            {
                CreateInterfaceProfile("interface-1", "Interface 1", isActive: true, isLicenseRequired: true),
                CreateInterfaceProfile("interface-2", "Interface 2", isActive: true, isLicenseRequired: true)
            },
            CreateLicenseInfo(licensedDeviceCount: 2),
            NowUtc);

        Assert.Equal(2, states.Count(IsCountedLicensedInterface));
        Assert.All(states, state => Assert.True(state.IsCoveredByLicense));
    }

    [Fact]
    public void Evaluate_ShouldMarkAdditionalProfilesAsUncoveredWhenLicenseLimitIsExceeded()
    {
        var states = _evaluator.Evaluate(
            new[]
            {
                CreateInterfaceProfile("interface-1", "Interface 1", isActive: true, isLicenseRequired: true),
                CreateInterfaceProfile("interface-2", "Interface 2", isActive: true, isLicenseRequired: true),
                CreateInterfaceProfile("interface-3", "Interface 3", isActive: true, isLicenseRequired: true)
            },
            CreateLicenseInfo(licensedDeviceCount: 2),
            NowUtc);

        Assert.Equal(2, states.Count(state => state.IsCoveredByLicense));
        var uncovered = Assert.Single(states, state => !state.IsCoveredByLicense);
        Assert.Equal("interface-3", uncovered.InterfaceProfileId);
        Assert.Contains("lizenzierte Anzahl", uncovered.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldCreateUnderstandableStatusMessages()
    {
        var states = _evaluator.Evaluate(
            new[]
            {
                CreateInterfaceProfile("interface-covered", "Covered Interface", isActive: true, isLicenseRequired: true),
                CreateInterfaceProfile("interface-inactive", "Inactive Interface", isActive: false, isLicenseRequired: true),
                CreateInterfaceProfile("interface-free", "Free Interface", isActive: true, isLicenseRequired: false)
            },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        Assert.Contains(states, state => state.StatusMessage.Contains("Durch Lizenz gedeckt.", StringComparison.Ordinal));
        Assert.Contains(states, state => state.StatusMessage.Contains("Lizenzpflichtig, aber nicht aktiv", StringComparison.Ordinal));
        Assert.Contains(states, state => state.StatusMessage.Contains("Nicht lizenzpflichtig", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_ShouldKeepInactiveLicenseRequiredProfilesVisibleButNotCounted()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Inactive Licensed Interface", isActive: false, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        var state = Assert.Single(states);
        Assert.True(state.IsLicenseRequired);
        Assert.False(state.IsActive);
        Assert.False(IsCountedLicensedInterface(state));
        Assert.Equal("Lizenzpflichtig, aber nicht aktiv - zählt aktuell nicht.", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldShowProfileThatIsNotLicenseRequiredAsNotLicenseRequired()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Free Interface", isActive: false, isLicenseRequired: false) },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        var state = Assert.Single(states);
        Assert.False(state.IsLicenseRequired);
        Assert.Equal("Nicht lizenzpflichtig.", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldSupportTotalAndActiveLicenseRequiredCounts()
    {
        var states = _evaluator.Evaluate(
            new[]
            {
                CreateInterfaceProfile("interface-active", "Active Interface", isActive: true, isLicenseRequired: true),
                CreateInterfaceProfile("interface-inactive", "Inactive Interface", isActive: false, isLicenseRequired: true),
                CreateInterfaceProfile("interface-free", "Free Interface", isActive: true, isLicenseRequired: false)
            },
            CreateLicenseInfo(licensedDeviceCount: 1),
            NowUtc);

        Assert.Equal(2, states.Count(state => state.IsLicenseRequired));
        Assert.Single(states, IsCountedLicensedInterface);
    }

    [Fact]
    public void Evaluate_ShouldLeaveGracePeriodFieldsEmptyForNow()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Licensed Interface", isActive: true, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 0),
            NowUtc);

        var state = Assert.Single(states);
        Assert.False(state.IsInGracePeriod);
        Assert.Null(state.GracePeriodStartedAt);
        Assert.Null(state.GracePeriodEndsAt);
    }

    [Fact]
    public void Evaluate_ShouldMarkUncoveredProfileWithValidGracePeriodAsInGracePeriod()
    {
        var gracePeriod = CreateGracePeriod("interface-1", NowUtc.AddDays(-1), NowUtc.AddDays(29));

        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Licensed Interface", isActive: true, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 0),
            new[] { gracePeriod },
            NowUtc);

        var state = Assert.Single(states);
        Assert.False(state.IsCoveredByLicense);
        Assert.True(state.IsInGracePeriod);
        Assert.Equal(gracePeriod.StartedAtUtc, state.GracePeriodStartedAt);
        Assert.Equal(gracePeriod.EndsAtUtc, state.GracePeriodEndsAt);
        Assert.Contains("in Karenzzeit bis", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldNotMarkExpiredGracePeriodAsActive()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Licensed Interface", isActive: true, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 0),
            new[] { CreateGracePeriod("interface-1", NowUtc.AddDays(-31), NowUtc.AddDays(-1)) },
            NowUtc);

        var state = Assert.Single(states);
        Assert.False(state.IsCoveredByLicense);
        Assert.False(state.IsInGracePeriod);
        Assert.NotNull(state.GracePeriodEndsAt);
        Assert.Contains("Karenzzeit abgelaufen", state.StatusMessage);
    }

    [Fact]
    public void Evaluate_ShouldIgnoreGracePeriodForCoveredProfile()
    {
        var states = _evaluator.Evaluate(
            new[] { CreateInterfaceProfile("interface-1", "Licensed Interface", isActive: true, isLicenseRequired: true) },
            CreateLicenseInfo(licensedDeviceCount: 1),
            new[] { CreateGracePeriod("interface-1", NowUtc.AddDays(-1), NowUtc.AddDays(29)) },
            NowUtc);

        var state = Assert.Single(states);
        Assert.True(state.IsCoveredByLicense);
        Assert.False(state.IsInGracePeriod);
        Assert.Null(state.GracePeriodStartedAt);
        Assert.Null(state.GracePeriodEndsAt);
        Assert.Equal("Durch Lizenz gedeckt.", state.StatusMessage);
    }

    private static bool IsCountedLicensedInterface(LicensedDeviceState state)
    {
        return state.IsActive && state.IsLicenseRequired;
    }

    private static InterfaceProfileDefinition CreateInterfaceProfile(
        string id,
        string name,
        bool isActive,
        bool isLicenseRequired)
    {
        return DefaultInterfaceProfileDefinitions.CreateMedistarNidekArk1sDefault() with
        {
            Metadata = CreateMetadata(id, name),
            IsActive = isActive,
            IsLicenseRequired = isLicenseRequired
        };
    }

    private static ProfileMetadata CreateMetadata(string id, string name)
    {
        var timestamp = new DateTimeOffset(NowUtc);

        return new ProfileMetadata(
            Id: id,
            Name: name,
            ProfileKind: ProfileKind.InterfaceProfile,
            Description: null,
            Vendor: null,
            Product: null,
            Version: "1.0.0",
            CreatedAt: timestamp,
            UpdatedAt: timestamp,
            CreatedBy: "XdtDeviceBridge",
            IsBuiltIn: false,
            IsUserDefined: true);
    }

    private static LicenseInfo CreateLicenseInfo(int licensedDeviceCount)
    {
        return new LicenseInfo(
            LicenseId: "license-1",
            CustomerName: "Musterpraxis",
            CustomerNumber: "C-1000",
            InstallationId: "installation-1",
            LicensedDeviceCount: licensedDeviceCount,
            ValidFrom: NowUtc.AddDays(-30),
            ValidUntil: NowUtc.AddDays(30),
            LicenseType: LicenseType.Monthly,
            ProductCode: "XDT-DEVICE-BRIDGE",
            MinimumAppVersion: "1.0.0",
            IssuedAt: NowUtc.AddDays(-31),
            Signature: "signed-license");
    }

    private static LicensedDeviceGracePeriod CreateGracePeriod(
        string interfaceProfileId,
        DateTime startedAtUtc,
        DateTime endsAtUtc)
    {
        return new LicensedDeviceGracePeriod(
            InterfaceProfileId: interfaceProfileId,
            StartedAtUtc: startedAtUtc,
            EndsAtUtc: endsAtUtc,
            Reason: "Test");
    }
}
