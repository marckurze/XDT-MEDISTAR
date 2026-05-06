using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicensedDeviceGracePeriodServiceTests
{
    private static readonly DateTime NowUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly LicensedDeviceGracePeriodService _service = new();

    [Fact]
    public void EnsureGracePeriodsForUncoveredDevices_ShouldCreateGracePeriodForUncoveredProfile()
    {
        var states = new[] { CreateState("interface-1", isCoveredByLicense: false) };

        var store = _service.EnsureGracePeriodsForUncoveredDevices(
            states,
            LicensedDeviceGracePeriodStore.Empty,
            NowUtc,
            graceDays: 30);

        var gracePeriod = Assert.Single(store.GracePeriods);
        Assert.Equal("interface-1", gracePeriod.InterfaceProfileId);
        Assert.Equal(NowUtc, gracePeriod.StartedAtUtc);
        Assert.Equal(NowUtc.AddDays(30), gracePeriod.EndsAtUtc);
        Assert.Equal("New active license-required interface profile", gracePeriod.Reason);
    }

    [Fact]
    public void EnsureGracePeriodsForUncoveredDevices_ShouldNotExtendExistingGracePeriod()
    {
        var existingGracePeriod = new LicensedDeviceGracePeriod(
            InterfaceProfileId: "interface-1",
            StartedAtUtc: NowUtc.AddDays(-10),
            EndsAtUtc: NowUtc.AddDays(20),
            Reason: "Existing");
        var existingStore = new LicensedDeviceGracePeriodStore(new[] { existingGracePeriod });

        var store = _service.EnsureGracePeriodsForUncoveredDevices(
            new[] { CreateState("interface-1", isCoveredByLicense: false) },
            existingStore,
            NowUtc,
            graceDays: 30);

        var gracePeriod = Assert.Single(store.GracePeriods);
        Assert.Equal(existingGracePeriod.StartedAtUtc, gracePeriod.StartedAtUtc);
        Assert.Equal(existingGracePeriod.EndsAtUtc, gracePeriod.EndsAtUtc);
        Assert.Equal("Existing", gracePeriod.Reason);
    }

    [Fact]
    public void EnsureGracePeriodsForUncoveredDevices_ShouldNotCreateDuplicateGracePeriodOnRepeatedCall()
    {
        var states = new[] { CreateState("interface-1", isCoveredByLicense: false) };
        var firstStore = _service.EnsureGracePeriodsForUncoveredDevices(
            states,
            LicensedDeviceGracePeriodStore.Empty,
            NowUtc,
            graceDays: 30);

        var secondStore = _service.EnsureGracePeriodsForUncoveredDevices(
            states,
            firstStore,
            NowUtc.AddDays(1),
            graceDays: 30);

        var gracePeriod = Assert.Single(secondStore.GracePeriods);
        Assert.Equal(NowUtc, gracePeriod.StartedAtUtc);
        Assert.Equal(NowUtc.AddDays(30), gracePeriod.EndsAtUtc);
    }

    [Fact]
    public void EnsureGracePeriodsForUncoveredDevices_ShouldNotCreateGracePeriodForCoveredProfile()
    {
        var store = _service.EnsureGracePeriodsForUncoveredDevices(
            new[] { CreateState("interface-1", isCoveredByLicense: true) },
            LicensedDeviceGracePeriodStore.Empty,
            NowUtc,
            graceDays: 30);

        Assert.Empty(store.GracePeriods);
    }

    [Fact]
    public void EnsureGracePeriodsForUncoveredDevices_ShouldNotCreateGracePeriodForInactiveProfile()
    {
        var store = _service.EnsureGracePeriodsForUncoveredDevices(
            new[] { CreateState("interface-1", isCoveredByLicense: false, isActive: false) },
            LicensedDeviceGracePeriodStore.Empty,
            NowUtc,
            graceDays: 30);

        Assert.Empty(store.GracePeriods);
    }

    [Fact]
    public void EnsureGracePeriodsForUncoveredDevices_ShouldNotCreateGracePeriodForProfileThatIsNotLicenseRequired()
    {
        var store = _service.EnsureGracePeriodsForUncoveredDevices(
            new[] { CreateState("interface-1", isCoveredByLicense: false, isLicenseRequired: false) },
            LicensedDeviceGracePeriodStore.Empty,
            NowUtc,
            graceDays: 30);

        Assert.Empty(store.GracePeriods);
    }

    private static LicensedDeviceState CreateState(
        string interfaceProfileId,
        bool isCoveredByLicense,
        bool isActive = true,
        bool isLicenseRequired = true)
    {
        return new LicensedDeviceState(
            InterfaceProfileId: interfaceProfileId,
            DisplayName: interfaceProfileId,
            IsActive: isActive,
            IsLicenseRequired: isLicenseRequired,
            IsCoveredByLicense: isCoveredByLicense,
            IsInGracePeriod: false,
            GracePeriodStartedAt: null,
            GracePeriodEndsAt: null,
            StatusMessage: "Test");
    }
}
