namespace XdtDeviceBridge.Core;

public sealed record InterfaceProfileAutoRedockState(
    string InterfaceProfileId,
    bool IsAutoDetached = false,
    bool IsOperationActive = false,
    bool IsTerminalCompleted = false,
    bool IsCountdownRunning = false,
    DateTime? RedockDueAt = null);
