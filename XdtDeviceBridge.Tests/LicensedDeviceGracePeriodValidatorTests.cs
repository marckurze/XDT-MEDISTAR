using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicensedDeviceGracePeriodValidatorTests
{
    private static readonly DateTime StartedAtUtc = new(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_ShouldAcceptValidGracePeriod()
    {
        var gracePeriod = CreateGracePeriod();

        var issues = LicensedDeviceGracePeriodValidator.Validate(gracePeriod);

        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_ShouldRejectEmptyInterfaceProfileId()
    {
        var gracePeriod = CreateGracePeriod() with { InterfaceProfileId = " " };

        var issues = LicensedDeviceGracePeriodValidator.Validate(gracePeriod);

        Assert.Contains("InterfaceProfileId must not be empty.", issues);
    }

    [Fact]
    public void Validate_ShouldRejectEndsAtBeforeStartedAt()
    {
        var gracePeriod = CreateGracePeriod() with { EndsAtUtc = StartedAtUtc.AddMinutes(-1) };

        var issues = LicensedDeviceGracePeriodValidator.Validate(gracePeriod);

        Assert.Contains("EndsAtUtc must not be before StartedAtUtc.", issues);
    }

    [Fact]
    public void Validate_ShouldRejectDuplicateInterfaceProfileIdsInStore()
    {
        var store = new LicensedDeviceGracePeriodStore(new[]
        {
            CreateGracePeriod("interface-1"),
            CreateGracePeriod("INTERFACE-1")
        });

        var issues = LicensedDeviceGracePeriodValidator.Validate(store);

        Assert.Contains("Duplicate InterfaceProfileId in grace period store: interface-1.", issues);
    }

    private static LicensedDeviceGracePeriod CreateGracePeriod(string interfaceProfileId = "interface-1")
    {
        return new LicensedDeviceGracePeriod(
            InterfaceProfileId: interfaceProfileId,
            StartedAtUtc: StartedAtUtc,
            EndsAtUtc: StartedAtUtc.AddDays(30),
            Reason: "Test");
    }
}
