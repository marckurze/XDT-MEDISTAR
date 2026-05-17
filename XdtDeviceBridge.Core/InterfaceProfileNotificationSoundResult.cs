namespace XdtDeviceBridge.Core;

public sealed record InterfaceProfileNotificationSoundResult(
    bool ShouldPlay,
    bool WasPlayed,
    bool IsSuppressedByCooldown,
    string? Message = null);
