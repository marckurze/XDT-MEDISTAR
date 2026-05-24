using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileAutoRedockServiceTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 17, 12, 0, 0, DateTimeKind.Local);

    [Fact]
    public void RecordMonitoringEvent_OpenActivityShouldNotStartCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360");
        service.MarkAutoDetached("interface-ar360", floatingState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-ar360", "package-state", "MEDISTAR + NIDEK AR360: Warte auf Gerätedatei.", BaseTime),
            floatingState);

        Assert.True(decision.IsOpenActivity);
        Assert.False(decision.DidStartCountdown);
        Assert.False(service.HasPendingCountdowns);
    }

    [Fact]
    public void RecordMonitoringEvent_SuccessfulExportShouldStartCountdownAndRedockWhenDue()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360");
        service.MarkAutoDetached("interface-ar360", floatingState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        Assert.True(decision.IsTerminalActivity);
        Assert.True(decision.DidStartCountdown);
        Assert.Equal(BaseTime.AddSeconds(5), decision.RedockDueAt);
        Assert.False(service.EvaluateDue("interface-ar360", floatingState, BaseTime.AddSeconds(4)).ShouldRedockNow);
        Assert.True(service.EvaluateDue("interface-ar360", floatingState, BaseTime.AddSeconds(5)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_FailedExportShouldStartCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ark1s");
        service.MarkAutoDetached("interface-ark1s", floatingState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-ark1s", "pair:patient-device:status", "Schnittstelle: automatische Verarbeitung fehlgeschlagen.", BaseTime),
            floatingState);

        Assert.True(decision.IsTerminalActivity);
        Assert.True(decision.DidStartCountdown);
        Assert.True(service.EvaluateDue("interface-ark1s", floatingState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_RequiredAttachmentTimeoutShouldStartCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ark1s");
        service.MarkAutoDetached("interface-ark1s", floatingState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-ark1s", "pair:patient-device:status", "XDT-Anhang Pflicht: Timeout erreicht, Verarbeitung blockiert.", BaseTime),
            floatingState);

        Assert.True(decision.IsTerminalActivity);
        Assert.True(decision.DidStartCountdown);
    }

    [Fact]
    public void RecordMonitoringEvent_NewActivityAfterTerminalSuccessShouldKeepCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360");
        service.MarkAutoDetached("interface-ar360", floatingState, BaseTime);
        _ = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        var openDecision = service.RecordMonitoringEvent(
            Event("interface-ar360", "scan-ais-detected", "MEDISTAR + NIDEK AR360: AIS-Datei erkannt (1).", BaseTime.AddSeconds(2)),
            floatingState);

        Assert.True(openDecision.IsOpenActivity);
        Assert.False(openDecision.DidCancelCountdown);
        Assert.True(service.HasPendingCountdowns);
        Assert.True(service.EvaluateDue("interface-ar360", floatingState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_VersionedNewActivityAfterTerminalSuccessShouldKeepCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-nt530p");
        service.MarkAutoDetached("interface-nt530p", floatingState, BaseTime);
        _ = service.RecordMonitoringEvent(
            Event("interface-nt530p", "pair:patient-device:status", "MEDISTAR + NIDEK NT530P: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        var openDecision = service.RecordMonitoringEvent(
            Event(
                "interface-nt530p",
                @"scan-device-detected:C:\Import\NIDEK NT530P.xml|2000|2048",
                "MEDISTAR + NIDEK NT530P: Gerätedatei erkannt (1).",
                BaseTime.AddSeconds(2)),
            floatingState);

        Assert.True(openDecision.IsOpenActivity);
        Assert.False(openDecision.DidCancelCountdown);
        Assert.True(service.HasPendingCountdowns);
        Assert.True(service.EvaluateDue("interface-nt530p", floatingState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_PinnedWindowShouldNotStartCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360", isPinned: true);
        service.MarkAutoDetached("interface-ar360", floatingState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        Assert.True(decision.IsTerminalActivity);
        Assert.False(decision.DidStartCountdown);
        Assert.False(service.HasPendingCountdowns);
    }

    [Fact]
    public void NotifyProcessingCompleted_ShouldStartCountdownForAutoDetachedWindow()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-cv5000");
        service.MarkAutoDetached("interface-cv5000", floatingState, BaseTime);

        var decision = service.NotifyProcessingCompleted("interface-cv5000", floatingState, BaseTime);

        Assert.True(decision.IsTerminalActivity);
        Assert.True(decision.DidStartCountdown);
        Assert.Equal(BaseTime.AddSeconds(5), decision.RedockDueAt);
        Assert.True(service.EvaluateDue("interface-cv5000", floatingState, BaseTime.AddSeconds(5)).ShouldRedockNow);
    }

    [Fact]
    public void NotifyProcessingCompleted_PinnedWindowShouldNotStartCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var pinnedState = DetachedState("interface-cv5000", isPinned: true);
        service.MarkAutoDetached("interface-cv5000", pinnedState, BaseTime);

        var decision = service.NotifyProcessingCompleted("interface-cv5000", pinnedState, BaseTime);

        Assert.True(decision.IsTerminalActivity);
        Assert.False(decision.DidStartCountdown);
        Assert.False(service.HasPendingCountdowns);
    }

    [Fact]
    public void RecordMonitoringEvent_RepeatedSuccessfulExportShouldNotPostponeCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-cv5000");
        service.MarkAutoDetached("interface-cv5000", floatingState, BaseTime);
        _ = service.NotifyProcessingCompleted("interface-cv5000", floatingState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-cv5000", "pair:patient-device:status", "MEDISTAR + TOPCON CV-5000: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime.AddSeconds(2)),
            floatingState);

        Assert.True(decision.IsTerminalActivity);
        Assert.False(decision.DidStartCountdown);
        Assert.Equal(BaseTime.AddSeconds(5), decision.RedockDueAt);
        Assert.True(service.EvaluateDue("interface-cv5000", floatingState, BaseTime.AddSeconds(5)).ShouldRedockNow);
    }

    [Fact]
    public void NotifyPinnedChanged_ShouldCancelCountdownWhenPinIsEnabled()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360");
        service.MarkAutoDetached("interface-ar360", floatingState, BaseTime);
        _ = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        var decision = service.NotifyPinnedChanged("interface-ar360", isPinned: true, DetachedState("interface-ar360", isPinned: true), BaseTime.AddSeconds(1));

        Assert.True(decision.DidCancelCountdown);
        Assert.False(service.HasPendingCountdowns);
        Assert.False(service.EvaluateDue("interface-ar360", DetachedState("interface-ar360", isPinned: true), BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void NotifyPinnedChanged_ShouldRestartCountdownWhenUnpinnedAfterTerminalCompletion()
    {
        var service = new InterfaceProfileAutoRedockService();
        var pinnedState = DetachedState("interface-ar360", isPinned: true);
        service.MarkAutoDetached("interface-ar360", pinnedState, BaseTime);
        _ = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            pinnedState);

        var decision = service.NotifyPinnedChanged("interface-ar360", isPinned: false, DetachedState("interface-ar360"), BaseTime.AddSeconds(2));

        Assert.True(decision.DidStartCountdown);
        Assert.Equal(BaseTime.AddSeconds(7), decision.RedockDueAt);
    }

    [Fact]
    public void NotifyDocked_ShouldCancelCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360");
        service.MarkAutoDetached("interface-ar360", floatingState, BaseTime);
        _ = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        service.NotifyDocked("interface-ar360");

        Assert.False(service.HasPendingCountdowns);
        Assert.False(service.EvaluateDue("interface-ar360", floatingState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_ShouldKeepProfilesIndependent()
    {
        var service = new InterfaceProfileAutoRedockService();
        var ar360State = DetachedState("interface-ar360");
        var ark1sState = DetachedState("interface-ark1s");
        service.MarkAutoDetached("interface-ar360", ar360State, BaseTime);
        service.MarkAutoDetached("interface-ark1s", ark1sState, BaseTime);

        _ = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            ar360State);

        Assert.True(service.EvaluateDue("interface-ar360", ar360State, BaseTime.AddSeconds(6)).ShouldRedockNow);
        Assert.False(service.EvaluateDue("interface-ark1s", ark1sState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_DockedWindowShouldNotStartCountdown()
    {
        var service = new InterfaceProfileAutoRedockService();
        var dockedState = new InterfaceProfileFloatingWindowState("interface-ar360");
        service.MarkAutoDetached("interface-ar360", dockedState, BaseTime);

        var decision = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            dockedState);

        Assert.True(decision.IsTerminalActivity);
        Assert.False(decision.DidStartCountdown);
        Assert.False(service.EvaluateDue("interface-ar360", dockedState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    [Fact]
    public void RecordMonitoringEvent_ManualDetachedWindowShouldNotAutoRedock()
    {
        var service = new InterfaceProfileAutoRedockService();
        var floatingState = DetachedState("interface-ar360");

        var decision = service.RecordMonitoringEvent(
            Event("interface-ar360", "pair:patient-device:status", "MEDISTAR + NIDEK AR360: automatisch verarbeitet. Exportdatei: out.gdt", BaseTime),
            floatingState);

        Assert.True(decision.IsTerminalActivity);
        Assert.False(decision.DidStartCountdown);
        Assert.False(service.EvaluateDue("interface-ar360", floatingState, BaseTime.AddSeconds(6)).ShouldRedockNow);
    }

    private static InterfaceProfileFloatingWindowState DetachedState(string interfaceProfileId, bool isPinned = false)
    {
        return new InterfaceProfileFloatingWindowState(
            interfaceProfileId,
            IsDetached: true,
            IsPinned: isPinned);
    }

    private static InterfaceMonitoringEventEntry Event(
        string scopeId,
        string eventKey,
        string message,
        DateTime timestamp)
    {
        return new InterfaceMonitoringEventEntry(
            timestamp,
            scopeId,
            eventKey,
            message,
            InterfaceMonitoringEventSeverity.Info);
    }
}
