namespace XdtDeviceBridge.Core;

public sealed record InterfaceProfileAutoRedockDecision(
    bool IsOpenActivity,
    bool IsTerminalActivity,
    bool DidStartCountdown,
    bool DidCancelCountdown,
    bool ShouldRedockNow,
    DateTime? RedockDueAt = null);
