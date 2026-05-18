using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceMonitoringEventDeduplicationServiceTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 10, 12, 0, 0, DateTimeKind.Local);

    [Fact]
    public void Record_ShouldReturnEventForFirstMessage()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        var entry = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime);

        Assert.NotNull(entry);
        Assert.Equal("interface-1", entry.ScopeId);
        Assert.Equal("package-state", entry.EventKey);
        Assert.Equal("Warte auf Gerätedatei.", entry.Message);
        Assert.Equal(InterfaceMonitoringEventSeverity.Info, entry.Severity);
    }

    [Fact]
    public void Record_ShouldSuppressSameMessageForSameEventKey()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        var first = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime);
        var second = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime.AddSeconds(5));

        Assert.NotNull(first);
        Assert.Null(second);
    }

    [Fact]
    public void Record_ShouldReturnEventWhenStateMessageChanges()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        _ = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime);
        var next = service.Record("interface-1", "package-state", "AIS-/Geräte-Paar vollständig.", BaseTime.AddSeconds(5));

        Assert.NotNull(next);
        Assert.Equal("AIS-/Geräte-Paar vollständig.", next.Message);
    }

    [Fact]
    public void Record_ShouldTrackDifferentScopesSeparately()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        _ = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime);
        var otherScope = service.Record("interface-2", "package-state", "Warte auf Gerätedatei.", BaseTime.AddSeconds(5));

        Assert.NotNull(otherScope);
    }

    [Fact]
    public void Reset_ShouldAllowSameMessageAgain()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        _ = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime);
        service.Reset();
        var next = service.Record("interface-1", "package-state", "Warte auf Gerätedatei.", BaseTime.AddSeconds(5));

        Assert.NotNull(next);
    }

    [Fact]
    public void ResetProfile_ShouldAllowSameProfileMessageAgain()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        _ = service.Record("interface-1", "scan-ais-detected:C:\\Import\\Patient.XDT|1000|200", "AIS-Datei erkannt (1).", BaseTime);
        service.ResetProfile("interface-1");
        var next = service.Record("interface-1", "scan-ais-detected:C:\\Import\\Patient.XDT|1000|200", "AIS-Datei erkannt (1).", BaseTime.AddSeconds(5));

        Assert.NotNull(next);
    }

    [Fact]
    public void ResetProfile_ShouldKeepOtherProfilesDeduplicated()
    {
        var service = new InterfaceMonitoringEventDeduplicationService();

        _ = service.Record("interface-1", "scan-ais-detected", "AIS-Datei erkannt (1).", BaseTime);
        _ = service.Record("interface-2", "scan-ais-detected", "AIS-Datei erkannt (1).", BaseTime);
        service.ResetProfile("interface-1");

        var resetProfileEntry = service.Record("interface-1", "scan-ais-detected", "AIS-Datei erkannt (1).", BaseTime.AddSeconds(5));
        var otherProfileEntry = service.Record("interface-2", "scan-ais-detected", "AIS-Datei erkannt (1).", BaseTime.AddSeconds(5));

        Assert.NotNull(resetProfileEntry);
        Assert.Null(otherProfileEntry);
    }
}
