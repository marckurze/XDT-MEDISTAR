namespace XdtDeviceBridge.Core;

public sealed record InterfaceProfileFloatingWindowState(
    string InterfaceProfileId,
    bool IsDetached = false,
    bool IsPinned = false,
    bool IsPositionMemoryEnabled = false,
    InterfaceProfileFloatingWindowBounds? Bounds = null);
