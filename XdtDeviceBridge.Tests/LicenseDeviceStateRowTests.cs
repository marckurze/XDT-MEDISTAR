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

    [Fact]
    public void FromState_ShouldShowRemainingGraceTimeAndLocation()
    {
        var nowUtc = new DateTime(2026, 6, 9, 8, 0, 0, DateTimeKind.Utc);

        var row = LicenseDeviceStateRow.FromState(
            new LicensedDeviceState(
                InterfaceProfileId: "interface-1",
                DisplayName: "Interface 1",
                IsActive: true,
                IsLicenseRequired: true,
                IsCoveredByLicense: false,
                IsInGracePeriod: true,
                GracePeriodStartedAt: nowUtc.AddDays(-2),
                GracePeriodEndsAt: nowUtc.AddDays(3).AddHours(2),
                StatusMessage: "Karenz aktiv."),
            "Raum 2",
            nowUtc);

        Assert.Equal("Raum 2", row.Standort);
        Assert.Equal("3 Tage", row.KarenzVerbleibendText);
    }
}
