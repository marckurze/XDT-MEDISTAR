using XdtDeviceBridge.Core;

namespace XdtDeviceBridge.Tests;

public sealed class InterfaceProfileAutoDetachServiceTests
{
    private static readonly DateTime BaseTime = new(2026, 5, 17, 10, 0, 0, DateTimeKind.Local);

    [Fact]
    public void Evaluate_ShouldAutoDetachOnlyAr360ForAr360Activity()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        var autoDetachService = new InterfaceProfileAutoDetachService();

        ApplyDecision(stateService, autoDetachService, Event("interface-ar360", "scan-ais-detected", BaseTime));

        Assert.True(stateService.GetOrCreate("interface-ar360").IsDetached);
        Assert.False(stateService.GetOrCreate("interface-ark1s").IsDetached);
    }

    [Fact]
    public void Evaluate_ShouldAutoDetachOnlyArk1sForArk1sActivity()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        var autoDetachService = new InterfaceProfileAutoDetachService();

        ApplyDecision(stateService, autoDetachService, Event("interface-ark1s", "scan-device-detected", BaseTime));

        Assert.True(stateService.GetOrCreate("interface-ark1s").IsDetached);
        Assert.False(stateService.GetOrCreate("interface-ar360").IsDetached);
    }

    [Fact]
    public void Evaluate_ShouldBringAlreadyDetachedWindowToFrontWithoutSecondDetach()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        stateService.Detach("interface-ar360");
        var autoDetachService = new InterfaceProfileAutoDetachService();

        var decision = autoDetachService.Evaluate(
            Event("interface-ar360", "scan-ready-pair", BaseTime),
            stateService.GetOrCreate("interface-ar360"));

        Assert.True(decision.IsRelevantActivity);
        Assert.False(decision.ShouldDetach);
        Assert.True(decision.ShouldBringToFront);
        Assert.True(stateService.GetOrCreate("interface-ar360").IsDetached);
    }

    [Fact]
    public void Evaluate_ShouldSuppressRepeatedActivityInsideCooldown()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        var autoDetachService = new InterfaceProfileAutoDetachService();

        var first = autoDetachService.Evaluate(
            Event("interface-ar360", "scan-ais-detected", BaseTime),
            stateService.GetOrCreate("interface-ar360"));
        if (first.ShouldDetach)
        {
            stateService.Detach("interface-ar360");
        }

        var second = autoDetachService.Evaluate(
            Event("interface-ar360", "scan-device-detected", BaseTime.AddSeconds(1)),
            stateService.GetOrCreate("interface-ar360"));

        Assert.True(first.ShouldDetach);
        Assert.True(second.IsRelevantActivity);
        Assert.True(second.IsSuppressedByCooldown);
        Assert.False(second.ShouldDetach);
        Assert.False(second.ShouldBringToFront);
    }

    [Fact]
    public void Evaluate_ShouldPreservePinnedStateWhenAutoDetaching()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        stateService.SetPinned("interface-ar360", true);
        var autoDetachService = new InterfaceProfileAutoDetachService();

        ApplyDecision(stateService, autoDetachService, Event("interface-ar360", "package-state", BaseTime));

        var state = stateService.GetOrCreate("interface-ar360");
        Assert.True(state.IsDetached);
        Assert.True(state.IsPinned);
    }

    [Fact]
    public void Evaluate_ShouldPreservePositionMemoryAndBoundsWhenAutoDetaching()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        stateService.RememberPosition("interface-ar360", 10, 20, 360, 240);
        var autoDetachService = new InterfaceProfileAutoDetachService();

        ApplyDecision(stateService, autoDetachService, Event("interface-ar360", "scan-device-detected", BaseTime));

        var state = stateService.GetOrCreate("interface-ar360");
        Assert.True(state.IsDetached);
        Assert.True(state.IsPositionMemoryEnabled);
        Assert.Equal(new InterfaceProfileFloatingWindowBounds(10, 20, 360, 240), state.Bounds);
    }

    [Fact]
    public void Evaluate_ShouldAllowLaterActivityAfterManualDock()
    {
        var stateService = new InterfaceProfileFloatingWindowStateService();
        var autoDetachService = new InterfaceProfileAutoDetachService();

        ApplyDecision(stateService, autoDetachService, Event("interface-ar360", "scan-ais-detected", BaseTime));
        stateService.Dock("interface-ar360");
        ApplyDecision(stateService, autoDetachService, Event("interface-ar360", "scan-device-detected", BaseTime.AddSeconds(3)));

        Assert.True(stateService.GetOrCreate("interface-ar360").IsDetached);
    }

    [Fact]
    public void Evaluate_ShouldIgnoreGlobalMonitoringEvents()
    {
        var autoDetachService = new InterfaceProfileAutoDetachService();
        var state = new InterfaceProfileFloatingWindowState("interface-ar360");

        var decision = autoDetachService.Evaluate(
            Event("monitoring", "scan-ais-detected", BaseTime),
            state);

        Assert.False(decision.IsRelevantActivity);
        Assert.False(decision.ShouldDetach);
    }

    [Fact]
    public void Evaluate_ShouldTreatPairEventsAsRelevantActivity()
    {
        var autoDetachService = new InterfaceProfileAutoDetachService();
        var state = new InterfaceProfileFloatingWindowState("interface-ark1s");

        var decision = autoDetachService.Evaluate(
            Event("interface-ark1s", "pair:patient-device:status", BaseTime),
            state);

        Assert.True(decision.IsRelevantActivity);
        Assert.True(decision.ShouldDetach);
        Assert.True(decision.ShouldBringToFront);
    }

    [Fact]
    public void Evaluate_ShouldTreatAttachmentScanWarningsAsRelevantActivity()
    {
        var autoDetachService = new InterfaceProfileAutoDetachService();
        var state = new InterfaceProfileFloatingWindowState("interface-ark1s");

        var decision = autoDetachService.Evaluate(
            Event(
                "interface-ark1s",
                "scan-message:Dateipaar vollständig, warte auf XDT-Anhang.",
                BaseTime,
                "Dateipaar vollständig, warte auf XDT-Anhang.",
                InterfaceMonitoringEventSeverity.Warning),
            state);

        Assert.True(decision.IsRelevantActivity);
        Assert.True(decision.ShouldDetach);
    }

    [Fact]
    public void ResetProfile_ShouldClearCooldownOnlyForSelectedProfile()
    {
        var autoDetachService = new InterfaceProfileAutoDetachService();
        var ar360State = new InterfaceProfileFloatingWindowState("interface-ar360");
        var ark1sState = new InterfaceProfileFloatingWindowState("interface-ark1s");

        _ = autoDetachService.Evaluate(Event("interface-ar360", "scan-ais-detected", BaseTime), ar360State);
        _ = autoDetachService.Evaluate(Event("interface-ark1s", "scan-ais-detected", BaseTime), ark1sState);
        autoDetachService.ResetProfile("interface-ar360");
        var ar360Decision = autoDetachService.Evaluate(Event("interface-ar360", "scan-device-detected", BaseTime.AddSeconds(1)), ar360State);
        var ark1sDecision = autoDetachService.Evaluate(Event("interface-ark1s", "scan-device-detected", BaseTime.AddSeconds(1)), ark1sState);

        Assert.False(ar360Decision.IsSuppressedByCooldown);
        Assert.True(ark1sDecision.IsSuppressedByCooldown);
    }

    private static void ApplyDecision(
        InterfaceProfileFloatingWindowStateService stateService,
        InterfaceProfileAutoDetachService autoDetachService,
        InterfaceMonitoringEventEntry entry)
    {
        var state = stateService.GetOrCreate(entry.ScopeId);
        var decision = autoDetachService.Evaluate(entry, state);
        if (decision.ShouldDetach)
        {
            stateService.Detach(entry.ScopeId);
        }
    }

    private static InterfaceMonitoringEventEntry Event(
        string scopeId,
        string eventKey,
        DateTime timestamp,
        string? message = null,
        InterfaceMonitoringEventSeverity severity = InterfaceMonitoringEventSeverity.Info)
    {
        return new InterfaceMonitoringEventEntry(
            timestamp,
            scopeId,
            eventKey,
            message ?? eventKey,
            severity);
    }
}
