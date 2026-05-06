using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class LicenseDeviceStateRowTests
{
    [Fact]
    public void FromState_ShouldFormatBooleanValuesAsJaNein()
    {
        var row = LicenseDeviceStateRow.FromState(new LicensedDeviceState(
            InterfaceProfileId: "interface-1",
            DisplayName: "Interface 1",
            IsActive: true,
            IsLicenseRequired: false,
            IsCoveredByLicense: true,
            IsInGracePeriod: false,
            GracePeriodStartedAt: null,
            GracePeriodEndsAt: null,
            StatusMessage: "Status bleibt erhalten."));

        Assert.Equal("Ja", row.AktivText);
        Assert.Equal("Nein", row.LizenzpflichtigText);
        Assert.Equal("Ja", row.GedecktText);
        Assert.Equal("Nein", row.KarenzText);
    }

    [Fact]
    public void FromState_ShouldKeepStatusMessageComplete()
    {
        const string status = "Lizenzpflichtig, aber nicht aktiv - zählt aktuell nicht.";

        var row = LicenseDeviceStateRow.FromState(new LicensedDeviceState(
            InterfaceProfileId: "interface-1",
            DisplayName: "Interface 1",
            IsActive: false,
            IsLicenseRequired: true,
            IsCoveredByLicense: false,
            IsInGracePeriod: false,
            GracePeriodStartedAt: null,
            GracePeriodEndsAt: null,
            StatusMessage: status));

        Assert.Equal(status, row.Status);
    }
}
