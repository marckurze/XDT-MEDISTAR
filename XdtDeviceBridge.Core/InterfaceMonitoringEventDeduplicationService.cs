namespace XdtDeviceBridge.Core;

public sealed class InterfaceMonitoringEventDeduplicationService
{
    private readonly Dictionary<string, string> _lastMessagesByEventKey = new(StringComparer.OrdinalIgnoreCase);

    public InterfaceMonitoringEventEntry? Record(
        string scopeId,
        string eventKey,
        string message,
        DateTime timestamp,
        InterfaceMonitoringEventSeverity severity = InterfaceMonitoringEventSeverity.Info)
    {
        if (string.IsNullOrWhiteSpace(scopeId))
        {
            throw new ArgumentException("Scope id must not be empty.", nameof(scopeId));
        }

        if (string.IsNullOrWhiteSpace(eventKey))
        {
            throw new ArgumentException("Event key must not be empty.", nameof(eventKey));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message must not be empty.", nameof(message));
        }

        var normalizedKey = $"{scopeId.Trim()}|{eventKey.Trim()}";
        var normalizedMessage = message.Trim();
        if (_lastMessagesByEventKey.TryGetValue(normalizedKey, out var previousMessage)
            && string.Equals(previousMessage, normalizedMessage, StringComparison.Ordinal))
        {
            return null;
        }

        _lastMessagesByEventKey[normalizedKey] = normalizedMessage;
        return new InterfaceMonitoringEventEntry(
            Timestamp: timestamp,
            ScopeId: scopeId.Trim(),
            EventKey: eventKey.Trim(),
            Message: normalizedMessage,
            Severity: severity);
    }

    public void Reset()
    {
        _lastMessagesByEventKey.Clear();
    }
}
