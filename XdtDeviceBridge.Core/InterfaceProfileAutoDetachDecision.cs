namespace XdtDeviceBridge.Core;

public sealed record InterfaceProfileAutoDetachDecision(
    bool IsRelevantActivity,
    bool IsSuppressedByCooldown,
    bool ShouldDetach,
    bool ShouldBringToFront);
