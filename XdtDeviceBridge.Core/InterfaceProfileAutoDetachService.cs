namespace XdtDeviceBridge.Core;

public sealed class InterfaceProfileAutoDetachService
{
    public static readonly TimeSpan DefaultCooldown = TimeSpan.FromSeconds(2);

    private readonly Dictionary<string, DateTime> _lastAutoDetachActivityByProfileId = new(StringComparer.OrdinalIgnoreCase);
    private readonly TimeSpan _cooldown;

    public InterfaceProfileAutoDetachService(TimeSpan? cooldown = null)
    {
        _cooldown = cooldown ?? DefaultCooldown;
    }

    public InterfaceProfileAutoDetachDecision Evaluate(
        InterfaceMonitoringEventEntry entry,
        InterfaceProfileFloatingWindowState state)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(state);

        if (!IsRelevantForProfile(entry, state.InterfaceProfileId)
            || !IsRelevantActivity(entry))
        {
            return new InterfaceProfileAutoDetachDecision(
                IsRelevantActivity: false,
                IsSuppressedByCooldown: false,
                ShouldDetach: false,
                ShouldBringToFront: false);
        }

        if (_lastAutoDetachActivityByProfileId.TryGetValue(entry.ScopeId, out var previousActivity)
            && entry.Timestamp - previousActivity < _cooldown)
        {
            return new InterfaceProfileAutoDetachDecision(
                IsRelevantActivity: true,
                IsSuppressedByCooldown: true,
                ShouldDetach: false,
                ShouldBringToFront: false);
        }

        _lastAutoDetachActivityByProfileId[entry.ScopeId] = entry.Timestamp;
        return new InterfaceProfileAutoDetachDecision(
            IsRelevantActivity: true,
            IsSuppressedByCooldown: false,
            ShouldDetach: !state.IsDetached,
            ShouldBringToFront: true);
    }

    public void ResetProfile(string interfaceProfileId)
    {
        if (string.IsNullOrWhiteSpace(interfaceProfileId))
        {
            return;
        }

        _lastAutoDetachActivityByProfileId.Remove(interfaceProfileId.Trim());
    }

    private static bool IsRelevantForProfile(InterfaceMonitoringEventEntry entry, string interfaceProfileId)
    {
        return !string.IsNullOrWhiteSpace(entry.ScopeId)
            && !string.Equals(entry.ScopeId, "monitoring", StringComparison.OrdinalIgnoreCase)
            && string.Equals(entry.ScopeId, interfaceProfileId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRelevantActivity(InterfaceMonitoringEventEntry entry)
    {
        var eventKey = entry.EventKey.Trim();
        if (string.Equals(eventKey, "scan-ais-detected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "scan-device-detected", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "scan-ready-pair", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "package-state", StringComparison.OrdinalIgnoreCase)
            || string.Equals(eventKey, "automatic-processing-export-profile-missing", StringComparison.OrdinalIgnoreCase)
            || eventKey.StartsWith("pair:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!eventKey.StartsWith("scan-message:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return entry.Severity != InterfaceMonitoringEventSeverity.Info
            || ContainsAny(entry.Message, "AIS", "Gerät", "Geraet", "XDT-Anhang", "Anhang", "warte", "wartet", "Timeout", "Fehler", "blockiert");
    }

    private static bool ContainsAny(string text, params string[] fragments)
    {
        return fragments.Any(fragment => text.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }
}
