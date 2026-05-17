namespace XdtDeviceBridge.Core;

public sealed class InterfaceProfileNotificationSoundService
{
    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(2);

    private readonly Dictionary<string, DateTime> _lastSoundActivityByProfileId = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _cooldown;

    public InterfaceProfileNotificationSoundService(TimeSpan? cooldown = null, bool isEnabled = true)
    {
        _cooldown = cooldown ?? DefaultCooldown;
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }

    public InterfaceProfileNotificationSoundResult TryPlay(
        InterfaceMonitoringEventEntry entry,
        string soundFilePath,
        IInterfaceProfileNotificationSoundPlayer player)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(player);

        if (!IsEnabled || !IsSoundRelevantActivity(entry))
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: false,
                WasPlayed: false,
                IsSuppressedByCooldown: false);
        }

        if (_lastSoundActivityByProfileId.TryGetValue(entry.ScopeId, out var previousActivity)
            && entry.Timestamp - previousActivity < _cooldown)
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: true,
                WasPlayed: false,
                IsSuppressedByCooldown: true);
        }

        _lastSoundActivityByProfileId[entry.ScopeId] = entry.Timestamp;
        if (string.IsNullOrWhiteSpace(soundFilePath) || !File.Exists(soundFilePath))
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: true,
                WasPlayed: false,
                IsSuppressedByCooldown: false,
                Message: "Signalton-Datei fehlt.");
        }

        try
        {
            player.Play(soundFilePath);
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: true,
                WasPlayed: true,
                IsSuppressedByCooldown: false);
        }
        catch (Exception ex)
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: true,
                WasPlayed: false,
                IsSuppressedByCooldown: false,
                Message: $"Signalton konnte nicht abgespielt werden: {ex.Message}");
        }
    }

    private static bool IsSoundRelevantActivity(InterfaceMonitoringEventEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.ScopeId)
            || string.Equals(entry.ScopeId, "monitoring", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var eventKey = entry.EventKey.Trim();
        if (string.Equals(eventKey, "scan-ais-detected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "scan-device-detected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "scan-ready-pair", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (eventKey.StartsWith("scan-message:", StringComparison.OrdinalIgnoreCase)
            || eventKey.StartsWith("pair:", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "package-state", StringComparison.OrdinalIgnoreCase))
        {
            return ContainsAttachmentArrivalMessage(entry.Message);
        }

        return false;
    }

    private static bool ContainsAttachmentArrivalMessage(string message)
    {
        return ContainsAny(message, "XDT-Anhang", "Anhang", "Anhangdatei", "Anhänge")
            && ContainsAny(message, "erkannt", "gefunden", "vorbereitet", "Linkfelder");
    }

    private static bool ContainsAny(string text, params string[] fragments)
    {
        return fragments.Any(fragment => text.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
