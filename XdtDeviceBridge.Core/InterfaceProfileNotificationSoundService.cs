namespace XdtDeviceBridge.Core;

public sealed class InterfaceProfileNotificationSoundService
{
    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(2);

    private readonly Dictionary<string, DateTime> _lastSoundActivityByProfileId = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _handledDeviceFileKeys = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _cooldown;

    public InterfaceProfileNotificationSoundService(TimeSpan? cooldown = null, bool isEnabled = true)
    {
        _cooldown = cooldown ?? DefaultCooldown;
        IsEnabled = isEnabled;
    }

    public bool IsEnabled { get; }

    public InterfaceProfileNotificationSoundResult TryPlayForDeviceFileDetected(
        string interfaceProfileId,
        string deviceFilePath,
        DateTime deviceFileDetectedAtUtc,
        DateTime timestamp,
        string soundFilePath,
        IInterfaceProfileNotificationSoundPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (!IsEnabled)
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: false,
                WasPlayed: false,
                IsSuppressedByCooldown: false);
        }

        var normalizedProfileId = NormalizeId(interfaceProfileId);
        var normalizedDeviceFilePath = NormalizePath(deviceFilePath);
        var deviceFileKey = $"{normalizedProfileId}|{normalizedDeviceFilePath}|{deviceFileDetectedAtUtc.ToUniversalTime().Ticks}";
        if (!_handledDeviceFileKeys.Add(deviceFileKey))
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: false,
                WasPlayed: false,
                IsSuppressedByCooldown: false);
        }

        if (_lastSoundActivityByProfileId.TryGetValue(normalizedProfileId, out var previousActivity)
            && timestamp - previousActivity < _cooldown)
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: true,
                WasPlayed: false,
                IsSuppressedByCooldown: true);
        }

        _lastSoundActivityByProfileId[normalizedProfileId] = timestamp;
        return PlaySound(soundFilePath, player);
    }

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

        var normalizedProfileId = NormalizeId(entry.ScopeId);
        if (_lastSoundActivityByProfileId.TryGetValue(normalizedProfileId, out var previousActivity)
            && entry.Timestamp - previousActivity < _cooldown)
        {
            return new InterfaceProfileNotificationSoundResult(
                ShouldPlay: true,
                WasPlayed: false,
                IsSuppressedByCooldown: true);
        }

        _lastSoundActivityByProfileId[normalizedProfileId] = entry.Timestamp;
        return PlaySound(soundFilePath, player);
    }

    private static bool IsSoundRelevantActivity(InterfaceMonitoringEventEntry entry)
    {
        if (string.IsNullOrWhiteSpace(entry.ScopeId)
            || string.Equals(entry.ScopeId, "monitoring", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var eventKey = entry.EventKey.Trim();
        if (string.Equals(eventKey, "scan-device-detected", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static InterfaceProfileNotificationSoundResult PlaySound(
        string soundFilePath,
        IInterfaceProfileNotificationSoundPlayer player)
    {
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

    private static string NormalizeId(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            throw new ArgumentException("Schnittstellenprofil-ID fehlt.", nameof(interfaceProfileId));
        }

        return interfaceProfileId.Trim();
    }

    private static string NormalizePath(string deviceFilePath)
    {
        if (string.IsNullOrWhiteSpace(deviceFilePath))
        {
            throw new ArgumentException("Gerätedateipfad fehlt.", nameof(deviceFilePath));
        }

        return deviceFilePath.Trim();
    }
}
